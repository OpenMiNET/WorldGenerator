using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;
using QuickType;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace WorldGenerator.Tweaking
{
	public class NoiseData
	{
		public ChunkColumn Chunk;
		public int X;
		public int Z;
	}
	
	public class TestGame : Game
	{
		private int                      _chunks, _resolution;

		private GraphicsDeviceManager _graphics;

		private OverworldGeneratorV2   _worldGen;
		private Dictionary<int, Color> _biomeColors = new Dictionary<int, Color>();
		private Dictionary<string, uint> _blockColors;
		public TestGame(OverworldGeneratorV2 worldGen, int chunks, int resolution)
		{
			_worldGen = worldGen;
			_chunks = chunks;
			_resolution = resolution;
			_graphics = new GraphicsDeviceManager(this);

			_blockColors = JsonConvert.DeserializeObject<Dictionary<string, uint>>(
				File.ReadAllText(Path.Combine("Resources", "blockColors.json")));
			
			var data =
				"{ \"name\":\"test\", \"colorMap\":[\r\n[ 0, { \"r\":0, \"g\":0, \"b\":112 } ],\r\n[ 1, { \"r\":141, \"g\":179, \"b\":96 } ],\r\n[ 2, { \"r\":250, \"g\":148, \"b\":24 } ],\r\n[ 3, { \"r\":96, \"g\":96, \"b\":96 } ],\r\n[ 4, { \"r\":5, \"g\":102, \"b\":33 } ],\r\n[ 5, { \"r\":11, \"g\":2, \"b\":89 } ],\r\n[ 6, { \"r\":7, \"g\":249, \"b\":178 } ],\r\n[ 7, { \"r\":0, \"g\":0, \"b\":255 } ],\r\n[ 8, { \"r\":255, \"g\":0, \"b\":0 } ],\r\n[ 9, { \"r\":128, \"g\":128, \"b\":255 } ],\r\n[ 10, { \"r\":112, \"g\":112, \"b\":214 } ],\r\n[ 11, { \"r\":160, \"g\":160, \"b\":255 } ],\r\n[ 12, { \"r\":255, \"g\":255, \"b\":255 } ],\r\n[ 13, { \"r\":160, \"g\":160, \"b\":160 } ],\r\n[ 14, { \"r\":255, \"g\":0, \"b\":255 } ],\r\n[ 15, { \"r\":160, \"g\":0, \"b\":255 } ],\r\n[ 16, { \"r\":250, \"g\":222, \"b\":85 } ],\r\n[ 17, { \"r\":210, \"g\":95, \"b\":18 } ],\r\n[ 18, { \"r\":34, \"g\":85, \"b\":28 } ],\r\n[ 19, { \"r\":22, \"g\":57, \"b\":51 } ],\r\n[ 20, { \"r\":114, \"g\":120, \"b\":154 } ],\r\n[ 21, { \"r\":83, \"g\":123, \"b\":9 } ],\r\n[ 22, { \"r\":44, \"g\":66, \"b\":5 } ],\r\n[ 23, { \"r\":98, \"g\":139, \"b\":23 } ],\r\n[ 24, { \"r\":0, \"g\":0, \"b\":48 } ],\r\n[ 25, { \"r\":162, \"g\":162, \"b\":132 } ],\r\n[ 26, { \"r\":250, \"g\":240, \"b\":192 } ],\r\n[ 27, { \"r\":48, \"g\":116, \"b\":68 } ],\r\n[ 28, { \"r\":31, \"g\":5, \"b\":50 } ],\r\n[ 29, { \"r\":64, \"g\":81, \"b\":26 } ],\r\n[ 30, { \"r\":49, \"g\":85, \"b\":74 } ],\r\n[ 31, { \"r\":36, \"g\":63, \"b\":54 } ],\r\n[ 32, { \"r\":89, \"g\":102, \"b\":81 } ],\r\n[ 33, { \"r\":69, \"g\":7, \"b\":62 } ],\r\n[ 34, { \"r\":80, \"g\":112, \"b\":80 } ],\r\n[ 35, { \"r\":189, \"g\":18, \"b\":95 } ],\r\n[ 36, { \"r\":167, \"g\":157, \"b\":100 } ],\r\n[ 37, { \"r\":217, \"g\":69, \"b\":21 } ],\r\n[ 38, { \"r\":17, \"g\":151, \"b\":101 } ],\r\n[ 39, { \"r\":202, \"g\":140, \"b\":101 } ],\r\n[ 40, { \"r\":128, \"g\":128, \"b\":255 } ],\r\n[ 41, { \"r\":128, \"g\":128, \"b\":255 } ],\r\n[ 42, { \"r\":128, \"g\":128, \"b\":255 } ],\r\n[ 43, { \"r\":128, \"g\":128, \"b\":255 } ],\r\n[ 44, { \"r\":0, \"g\":0, \"b\":172 } ],\r\n[ 45, { \"r\":0, \"g\":0, \"b\":144 } ],\r\n[ 46, { \"r\":32, \"g\":32, \"b\":112 } ],\r\n[ 47, { \"r\":0, \"g\":0, \"b\":80 } ],\r\n[ 48, { \"r\":0, \"g\":0, \"b\":64 } ],\r\n[ 49, { \"r\":32, \"g\":32, \"b\":56 } ],\r\n[ 50, { \"r\":64, \"g\":64, \"b\":144 } ],\r\n[ 127, { \"r\":0, \"g\":0, \"b\":0 } ],\r\n[ 129, { \"r\":181, \"g\":219, \"b\":136 } ],\r\n[ 130, { \"r\":255, \"g\":188, \"b\":64 } ],\r\n[ 131, { \"r\":136, \"g\":136, \"b\":136 } ],\r\n[ 132, { \"r\":45, \"g\":142, \"b\":73 } ],\r\n[ 133, { \"r\":51, \"g\":142, \"b\":19 } ],\r\n[ 134, { \"r\":47, \"g\":255, \"b\":18 } ],\r\n[ 140, { \"r\":180, \"g\":20, \"b\":220 } ],\r\n[ 149, { \"r\":123, \"g\":13, \"b\":49 } ],\r\n[ 151, { \"r\":138, \"g\":179, \"b\":63 } ],\r\n[ 155, { \"r\":88, \"g\":156, \"b\":108 } ],\r\n[ 156, { \"r\":71, \"g\":15, \"b\":90 } ],\r\n[ 157, { \"r\":104, \"g\":121, \"b\":66 } ],\r\n[ 158, { \"r\":89, \"g\":125, \"b\":114 } ],\r\n[ 160, { \"r\":129, \"g\":142, \"b\":121 } ],\r\n[ 161, { \"r\":109, \"g\":119, \"b\":102 } ],\r\n[ 162, { \"r\":120, \"g\":52, \"b\":120 } ],\r\n[ 163, { \"r\":229, \"g\":218, \"b\":135 } ],\r\n[ 164, { \"r\":207, \"g\":197, \"b\":140 } ],\r\n[ 165, { \"r\":255, \"g\":109, \"b\":61 } ],\r\n[ 166, { \"r\":216, \"g\":191, \"b\":141 } ],\r\n[ 167, { \"r\":242, \"g\":180, \"b\":141 } ],\r\n[ 168, { \"r\":118, \"g\":142, \"b\":20 } ],\r\n[ 169, { \"r\":59, \"g\":71, \"b\":10 } ],\r\n[ 170, { \"r\":82, \"g\":41, \"b\":33 } ],\r\n[ 171, { \"r\":221, \"g\":8, \"b\":8 } ],\r\n[ 172, { \"r\":73, \"g\":144, \"b\":123 } ] ] }";

			var colors = BiomeColors.FromJson(data);

			foreach (var element in colors.ColorMap)
			{
				var e0 = element[0].Integer.Value;
				var e1 = element[1].ColorMapClass;

				_biomeColors.TryAdd(e0, new Color(e1.R, e1.G, e1.B));
			}
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

			foreach (RenderStage stage in Enum.GetValues<RenderStage>())
			{
				_stageDatas.Add(stage, new StageData(stage, GraphicsDevice, width, height, this));
			}
		}

		private Stopwatch _sw = Stopwatch.StartNew();

		private ConcurrentQueue<NoiseData> _newQueue = new ConcurrentQueue<NoiseData>();
		public void Add(NoiseData chunk)
		{
			_newQueue.Enqueue(chunk);
			//Chunks.Add(chunk);
		}

		private MouseState _mouseState = new MouseState();

		private BiomeBase ClickedBiome { get; set; } = null;
		private Point CursorPos { get; set; } = Point.Zero;
		/// <inheritdoc />
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			
			var state = Mouse.GetState(Window);

			var width = GraphicsDevice.Viewport.Width / 2;
			var height = GraphicsDevice.Viewport.Height / 2;
			if (state != _mouseState && state.LeftButton == ButtonState.Released && _mouseState.LeftButton == ButtonState.Pressed)
			{
				var pos = state.Position;

				if (pos.X <= width && pos.Y <= height)
				{
					if (_stageDatas.TryGetValue(RenderStage.Biomes, out var stageData))
					{
						var scale = new Vector2((float)width / (float)stageData.Texture.Width, (float)height / (float)stageData.Texture.Height);
						pos = new Point((int)(pos.X * scale.X), (int)(pos.Y * scale.Y));
						var color = stageData.GetColorAt(pos.X, pos.Y);
						var biomeId = _biomeColors.FirstOrDefault(x => x.Value == color).Key;
						var biome = _worldGen.BiomeProvider.GetBiome(biomeId);
						ClickedBiome = biome;
						
						CursorPos = pos;
					}
				}
			}
			
			_mouseState = state;
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
				
				chunk.Chunk?.Dispose();
				updates++;
			}

			_spriteBatch.Begin();
			
			DrawChunks(RenderStage.Biomes, new Rectangle(0, 0, width, height * 2));
			//DrawChunks(RenderStage.Temperature, new Rectangle(0, height, width, height));
			DrawChunks(RenderStage.Temperature, new Rectangle(width, 0, width, height * 2));
			//DrawChunks(RenderStage.Humidity, new Rectangle(width, height, width, height));

			if (ClickedBiome != null)
			{
				string text = $"Cursor: {CursorPos}\nBiome: {ClickedBiome.DefinitionName ?? "N/A"}";
				var size = _spriteFont.MeasureString(text);
				_spriteBatch.DrawString(_spriteFont, text, new Vector2(0, height - size.Y), Color.White);
			}

			var updateText = $"Updates: {updates:000}\nTemp: min={_worldGen.MinTemp:F3} max={_worldGen.MaxTemp:F3}  range={(_worldGen.MaxTemp - _worldGen.MinTemp):F3}\nRain: min={_worldGen.MinRain:F3} max={_worldGen.MaxRain:F3} range={(_worldGen.MaxRain - _worldGen.MinRain):F3}";
			var updateTextSize = _spriteFont.MeasureString(updateText);
			_spriteBatch.DrawString(_spriteFont, updateText, new Vector2(GraphicsDevice.Viewport.Width - (updateTextSize.X + 5), 5), Color.White);
			_spriteBatch.End();


			base.Draw(gameTime);
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
				foreach (var biome in _biomeColors)
				{
					var b = _worldGen.BiomeProvider.GetBiome(biome.Key);

					if (!string.IsNullOrWhiteSpace(b?.Name))
					{
						var textSize = _spriteFont.MeasureString(b.Name);
						_spriteBatch.Draw(_pixel, new Rectangle(xOffset, yOffset + 2, (int)textSize.X + 10, ((int)textSize.Y)), Color.Black * 0.75f);
						_spriteBatch.Draw(_pixel, new Rectangle(xOffset, yOffset + 2, 16, 16), biome.Value);

						_spriteBatch.DrawString(
							_spriteFont, b?.Name ?? "Unknown", new Vector2(xOffset + 16, yOffset), Color.White, 0f,
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
			Humidity,
			Temperature,
			Height,
			Biomes,
			//Blocks
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

			public void AddChunk(NoiseData data)
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
			
			private void DrawChunk(NoiseData data)
			{
				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 16; y++)
					{
						var pixelPosition = new Rectangle(x, y, 1, 1);
						var biome         = _parent._worldGen.BiomeProvider.GetBiome(data.Chunk.GetBiome(x, y));
							
						var temp  = (int) Math.Max(0, Math.Min(255, (255 * (biome.Temperature / 2f))));
						var humid = (int) Math.Max(0, Math.Min(255, (255 * biome.Downfall)));
						
						/*var t    = data.Temperature[NoiseMap.GetIndex(cx, cz)];
						var temp = (int) Math.Max(0,
							Math.Min(255, (255 * (t / 2f))));
	
						var r = data.Humidity[NoiseMap.GetIndex(cx, cz)];// MathF.Abs(_worldGen.RainfallNoise.GetValue(rx, rz));
						var humid = (int) Math.Max(0,
							Math.Min(255, (255 * r)));*/

						switch (_stage)
						{
							case RenderStage.Humidity:
								_spriteBatch.Draw(_pixel, pixelPosition, new Color(0, 0, humid));
								break;

							case RenderStage.Temperature:
								_spriteBatch.Draw(_pixel, pixelPosition, new Color(temp, 0, humid));
								break;

							case RenderStage.Height:
							{
								var column = data.Chunk;
								int height = column.GetHeight(x, y);
								_spriteBatch.Draw(_pixel, pixelPosition, new Color(height, height, height));

								//	_spriteBatch.Draw(_pixel, pixelPosition, new Color(temp, temp, temp));
								break;
							}

							case RenderStage.Biomes:
								//	var biome = _worldGen.BiomeProvider.GetBiome(t, r);
								//	var c = biome.Color.GetValueOrDefault(System.Drawing.Color.White);
								if (_parent._biomeColors.TryGetValue(biome.Id, out var c))
								{
									_spriteBatch.Draw(_pixel, pixelPosition, new Color(c.R, c.G, c.B));
								}

								break;

							/*case RenderStage.Blocks:
							{
								var column = data.Chunk;
								var block = column.GetBlockObject(x, column.GetHeight(x,y), y);

								if (_parent._blockColors.TryGetValue(block.Name, out uint color))
								{
									_spriteBatch.Draw(_pixel, pixelPosition, new Color(color));
								}
							} break;*/
						}
					}
				}
			}
		}
	}
}