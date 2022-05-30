using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;
namespace WorldGenerator.Tweaking
{
	public class TestGame : Game
	{
		private int                      _chunks, _resolution;

		private GraphicsDeviceManager _graphics;

		private OverworldGeneratorV2   _worldGen;
		public TestGame(OverworldGeneratorV2 worldGen, int chunks, int resolution)
		{
			_worldGen = worldGen;
			_chunks = chunks;
			_resolution = resolution;
			_graphics = new GraphicsDeviceManager(this);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			base.Initialize();
			Window.AllowUserResizing = true;
		}

		public static Texture2D      _pixel;
		private SpriteBatch    _spriteBatch;
		private SpriteFont _spriteFont;

		private Dictionary<TestGame.RenderStage, StageData> _stageDatas = new Dictionary<RenderStage, StageData>(); 

		/// <inheritdoc />
		protected override void LoadContent()
		{
			base.LoadContent();

			var width  = (_chunks * 16);
			var height = _chunks * 16;

			_pixel = new Texture2D(GraphicsDevice, 1, 1);
			_pixel.SetData(new[] {Color.White});

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			Content.RootDirectory = "Resources";
			_spriteFont = Content.Load<SpriteFont>("Arial");

			_stageDatas.Add(RenderStage.Biomes, new StageData(RenderStage.Biomes, GraphicsDevice, width, height, this));
			_stageDatas.Add(RenderStage.Temperature, new StageData(RenderStage.Temperature, GraphicsDevice, width, height, this));
			_stageDatas.Add(RenderStage.Height, new StageData(RenderStage.Height, GraphicsDevice, width, height, this));
			/*foreach (RenderStage stage in Enum.GetValues<RenderStage>())
			{
				_stageDatas.Add(stage, new StageData(stage, GraphicsDevice, width, height, this));
			}*/
		}
		
		private ConcurrentQueue<ChunkColumn> _newQueue = new ConcurrentQueue<ChunkColumn>();
		public void Add(ChunkColumn chunk)
		{
			_newQueue.Enqueue(chunk);
			//Chunks.Add(chunk);
		}

		private MouseState _mouseState = new MouseState();

		private int CursorHeight { get; set; } = 0;
		private BiomeBase ClickedBiome { get; set; } = null;
		private Point CursorPos { get; set; } = Point.Zero;

		private Stopwatch _biomeInfoSw = Stopwatch.StartNew();
		/// <inheritdoc />
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_biomeInfoSw.ElapsedMilliseconds >= 500)
			{
				var state = Mouse.GetState(Window);

				var width = GraphicsDevice.Viewport.Width / 2;
				var height = GraphicsDevice.Viewport.Height;

				if (state != _mouseState)
				{
					var pos = state.Position;

					if (pos.X >= 0 && pos.X <= width && pos.Y >= 0 && pos.Y <= height)
					{
						if (_stageDatas.TryGetValue(RenderStage.Biomes, out var stageData))
						{
							var scale = new Vector2(
								(float) stageData.Texture.Width / width, (float) stageData.Texture.Height / height);

							pos = new Point((int) (pos.X * scale.X), (int) (pos.Y * scale.Y));
							//var chunkPos = new ChunkCoordinates(pos.X >> 4, pos.Y >> 4);
							var color = stageData.GetColorAt(pos.X, pos.Y);
							var biome = _worldGen.BiomeRegistry.Biomes.FirstOrDefault(x =>
							{
								var c = x.Color.GetValueOrDefault();

								return c.R == color.R && c.B == color.B && c.G == color.G;
							});
							
							ClickedBiome = biome;

							CursorPos = pos;
						}
					}
					else if (pos.X >= width && pos.X <= width * 2f && pos.Y >= height && pos.Y <= height * 2f)
					{
						pos.X -= width;
						pos.Y -= height;
						if (_stageDatas.TryGetValue(RenderStage.Height, out var stageData))
						{
							var scale = new Vector2(
								(float) stageData.Texture.Width / width, (float) stageData.Texture.Height / height);

							pos = new Point((int) (pos.X * scale.X), (int) (pos.Y * scale.Y));
							var color = stageData.GetColorAt(pos.X, pos.Y);
							CursorHeight = color.R;
						}
					}
				}

				_biomeInfoSw.Restart();
				_mouseState = state;
			}

		}

		/// <inheritdoc />
		protected override void Draw(GameTime gameTime)
		{
			IsMouseVisible = true;
			var width = GraphicsDevice.Viewport.Width / 2;
			var height = GraphicsDevice.Viewport.Height / 2;
			GraphicsDevice.Clear(Color.Black);


			Stopwatch sw = Stopwatch.StartNew();

			int updates = 0;
			while (_newQueue.TryDequeue(out var chunk) && sw.ElapsedMilliseconds <= (1000f / 25))
			{
				foreach (var stage in _stageDatas)
				{
					stage.Value.AddChunk(chunk);
				}
				
				foreach (var subChunk in chunk) 
				{ 
					subChunk.Dispose();
				}

				chunk.Dispose();
				updates++;
			}

			_spriteBatch.Begin();
			
			DrawChunks(RenderStage.Biomes, new Rectangle(0, 0, width, height * 2));
			//DrawChunks(RenderStage.Temperature, new Rectangle(0, height, width, height));
			DrawChunks(RenderStage.Temperature, new Rectangle(width, 0, width, height));
			DrawChunks(RenderStage.Height, new Rectangle(width, height, width, height));

			if (ClickedBiome != null)
			{
				string text = $"Cursor: {CursorPos}\nBiome: {ClickedBiome.Name ?? "N/A"}";
				var size = _spriteFont.MeasureString(text);
				_spriteBatch.DrawString(_spriteFont, text, new Vector2(width - size.X, 10), Color.White);
			}
			
			DrawHeight(width * 2f, height + 10f);

			var updateText = $"Updates: {updates:000}\n" 
			                 + $"Temp: min={_worldGen.MinTemp:F3} max={_worldGen.MaxTemp:F3} range={(_worldGen.MaxTemp - _worldGen.MinTemp):F3}\n"
			                 + $"Rain: min={_worldGen.MinRain:F3} max={_worldGen.MaxRain:F3} range={(_worldGen.MaxRain - _worldGen.MinRain):F3}\n"
			                 + $"Selector: min={_worldGen.MinSelector:F3} max={_worldGen.MaxSelector:F3} range={(_worldGen.MaxSelector - _worldGen.MinSelector):F3}\n"/*
			                 + $"Height: min={_worldGen.MinHeight:F3} max={_worldGen.MaxHeight:F3} range={(_worldGen.MaxHeight - _worldGen.MinHeight):F3}\n"*/;
			var updateTextSize = _spriteFont.MeasureString(updateText);
			_spriteBatch.DrawString(_spriteFont, updateText, new Vector2(GraphicsDevice.Viewport.Width - (updateTextSize.X + 5), 5), Color.White);
			_spriteBatch.End();


			base.Draw(gameTime);
		}

		private void DrawHeight(float x, float y)
		{
			string text = $"Height: {CursorHeight:000}";
			var size = _spriteFont.MeasureString(text);
			_spriteBatch.DrawString(_spriteFont, text, new Vector2(x - size.X  + 10, y), Color.White);
		}

		private void DrawBorder(Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor) 
		{ 
			// Draw top line 
			_spriteBatch.Draw(_pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor); 

			// Draw left line 
			_spriteBatch.Draw(_pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor); 

			// Draw right line 
			_spriteBatch.Draw(_pixel, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder), 
				rectangleToDraw.Y, 
				thicknessOfBorder, 
				rectangleToDraw.Height), borderColor);
 
			// Draw bottom line 
			_spriteBatch.Draw(_pixel, new Rectangle(rectangleToDraw.X, 
				rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder, 
				rectangleToDraw.Width, 
				thicknessOfBorder), borderColor); 
		}

		private void DrawChunks(RenderStage renderStage, Rectangle offset)
		{
			if (!_stageDatas.TryGetValue(renderStage, out var stageData))
				return;
			
			_spriteBatch.Draw(stageData.Texture, offset, Color.White);
			_spriteBatch.DrawString(_spriteFont, $"Stage: {renderStage.ToString()}", new Vector2(offset.X + 5, offset.Y + 5), Color.White);

			if (renderStage == RenderStage.Biomes)
			{
				int yOffset = offset.Y + 32;
				int xOffset = offset.X + 5;
				foreach (var biome in _worldGen.BiomeRegistry.Biomes)
				{
					if (!string.IsNullOrWhiteSpace(biome?.Name))
					{
						var biomeColor = biome.Color.GetValueOrDefault();
						var textSize = _spriteFont.MeasureString(biome.Name ?? "Unknown");
						_spriteBatch.Draw(_pixel, new Rectangle(xOffset, yOffset, (int)textSize.X + 20, ((int)textSize.Y)), Color.Black * 0.75f);
						_spriteBatch.Draw(_pixel, new Rectangle(xOffset, yOffset + 2, 16, 16), new Color(biomeColor.R, biomeColor.G, biomeColor.B));

						_spriteBatch.DrawString(
							_spriteFont, biome?.Name ?? "Unknown", new Vector2(xOffset + 18, yOffset), Color.White, 0f,
							Vector2.Zero, 1f, SpriteEffects.None, 0);
						
						yOffset += ((int)textSize.Y) + 2;
					}
				}
			}
			
			DrawBorder(new Rectangle(offset.X,offset.Y, offset.Width, offset.Height), 1, Color.Red);
			//GraphicsDevice.SetRenderTarget(null);
		}

		public enum RenderStage
		{
			Temperature,
			Height,
			Biomes
		}
		
		private class StageData
		{
			private readonly RenderStage _stage;
			private readonly GraphicsDevice _device;
			private readonly TestGame _parent;
			public RenderTarget2D RenderTarget2D { get; }
			public Texture2D Texture { get; set; }

			private SpriteBatch _spriteBatch;
			public StageData(RenderStage stage, GraphicsDevice device, int width, int height, TestGame parent)
			{
				_stage = stage;
				_device = device;
				_parent = parent;
				RenderTarget2D = new RenderTarget2D(device, 16, 16);
				Texture = new Texture2D(device, width, height);
				_spriteBatch = new SpriteBatch(device);
			}

			public Color GetColorAt(int x, int y)
			{
				Color[] colors = new Color[1];
				Texture.GetData(0, new Rectangle(x, y, 1, 1), colors, 0, 1);

				return colors[0];
			}

			public void AddChunk(ChunkColumn data)
			{
				_device.SetRenderTarget(RenderTarget2D);
				_spriteBatch.Begin();
				//_spriteBatch.Draw(Texture, Vector2.Zero, Color.White);
				try
				{
					DrawChunk(data);
				}
				finally
				{
					_spriteBatch.End();
					_device.SetRenderTarget(null);

					var index = (data.X * 16) + (data.Z * 16);
					int[] d = new int[RenderTarget2D.Width * RenderTarget2D.Height];
					RenderTarget2D.GetData(0, new Rectangle(0, 0, 16, 16), d, 0, d.Length);
					
					Texture.SetData(0, new Rectangle((data.X * (16)), data.Z * (16), 16, 16), d, 0, d.Length);
				}
			}

			Color InterpolateColors(Color c1, Color c2, float value)
			{
				var v1 = c1.ToVector3();
				var v2 = c2.ToVector3();
				return new Color(MathUtils.Lerp(v1.X, v2.X, value), MathUtils.Lerp(v1.Y, v2.Y, value), MathUtils.Lerp(v1.Z, v2.Z, value));
			}
			
			Color GetHeatMapColor(float value)
			{
				const int NUM_COLORS = 4;

				float[][] color = new float[NUM_COLORS][]
				{
					new float[] {0, 0, 1}, new float[] {0, 1, 0}, new float[] {1, 1, 0}, new float[] {1, 0, 0}
				};
				// A static array of 4 colors:  (blue,   green,  yellow,  red) using {r,g,b} for each.
  
				int idx1;        // |-- Our desired color will be between these two indexes in "color".
				int idx2;        // |
				float fractBetween = 0;  // Fraction between "idx1" and "idx2" where our value is.
  
				if(value <= 0)      {  idx1 = idx2 = 0;            }    // accounts for an input <=0
				else if(value >= 1)  {  idx1 = idx2 = NUM_COLORS-1; }    // accounts for an input >=0
				else
				{
					value = value * (NUM_COLORS-1);        // Will multiply value by 3.
					idx1  = (int)MathF.Floor(value);                  // Our desired color will be after this index.
					idx2  = idx1+1;                        // ... and before this index (inclusive).
					fractBetween = value - idx1;    // Distance between the two indexes (0-1).
				}
    
				var red   = (color[idx2][0] - color[idx1][0])*fractBetween + color[idx1][0];
				var green = (color[idx2][1] - color[idx1][1])*fractBetween + color[idx1][1];
				var blue  = (color[idx2][2] - color[idx1][2])*fractBetween + color[idx1][2];

				return new Color(red, green, blue);
			}
			
			public static Color ChangeColorBrightness(Color color, float correctionFactor)
			{
				float red = (float)color.R;
				float green = (float)color.G;
				float blue = (float)color.B;

				if (correctionFactor < 0)
				{
					correctionFactor = 1 + correctionFactor;
					red *= correctionFactor;
					green *= correctionFactor;
					blue *= correctionFactor;
				}
				else
				{
					red = (255 - red) * correctionFactor + red;
					green = (255 - green) * correctionFactor + green;
					blue = (255 - blue) * correctionFactor + blue;
				}

				return new Color((int)red, (int)green, (int)blue, color.A);
			}
			
			private void DrawChunk(ChunkColumn data)
			{
				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 16; y++)
					{
						var pixelPosition = new Rectangle(x, y, 1, 1);

						switch (_stage)
						{
							case RenderStage.Temperature:
							{
								var biome = _parent._worldGen.BiomeRegistry.GetBiome(data.GetBiome(x, y));

								_spriteBatch.Draw(
									_pixel, pixelPosition, new Color(biome.Temperature / 2f, 0f, biome.Downfall));
							} break;

							case RenderStage.Height:
							{
								float height = (1f / 255f) * data.GetHeight(x, y);
								_spriteBatch.Draw(_pixel, pixelPosition, new Color(height, height, height));
							} break;

							case RenderStage.Biomes:
							{
								var biome = _parent._worldGen.BiomeRegistry.GetBiome(data.GetBiome(x, y));
								var c = biome.Color.GetValueOrDefault();
								_spriteBatch.Draw(_pixel, pixelPosition, new Color(c.R, c.G, c.B));
							} break;
						}
					}
				}
			}
		}
	}
}