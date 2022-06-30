using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;
using OpenMiNET.Noise;
using OpenMiNET.Noise.Maths;

namespace WorldGenerator.Tweaking
{
	public class NoisePreviewGame : Game
	{
		private int                      _chunks, _resolution;

		private GraphicsDeviceManager _graphics;

		private OverworldGeneratorV2   _worldGen;
		public NoisePreviewGame(OverworldGeneratorV2 worldGen, int chunks, int resolution)
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

		private Dictionary<NoisePreviewGame.NoiseStage, NoiseStageData> _stageDatas = new Dictionary<NoiseStage, NoiseStageData>(); 
		
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

			_stageDatas.TryAdd(
				NoiseStage.TemperatureAndHumidity,
				new NoiseStageData(NoiseStage.TemperatureAndHumidity, GraphicsDevice, width, height, this));
			
			new Thread(
				() =>
				{
					float max = float.MinValue;
					float min = float.MaxValue;
					
					Parallel.For(
						(long) 0, (_chunks * _chunks), i =>
						{
							var cx = (int)Math.Floor(i / (double)_chunks);
							var cz = (int)(i % _chunks);
							
							ChunkNoiseData noiseData = new ChunkNoiseData();
							noiseData.X = cx;
							noiseData.Z = cz;
							noiseData.Humidity = new float[256];
							noiseData.Temperatures = new float[256];
							noiseData.SelectorNoise = new float[256];
							
							int idx = 0;
							for (int x = 0; x < 16; x++)
							{
								var rx = (cx * 16f) + x;
								for (int z = 0; z < 16; z++)
								{
									var rz = (cz * 16f) + z;
									idx = NoiseMap.GetIndex(x, z);
									var temp = MathF.Abs(_worldGen.TemperatureNoise.GetValue(rx , rz));

									var rain = MathF.Abs(_worldGen.RainfallNoise.GetValue(rx , rz));

									var selector = MathF.Abs(_worldGen.SelectorNoise.GetValue(rx, rz));

									noiseData.Humidity[idx] = rain;
									noiseData.Temperatures[idx] = temp;
									noiseData.SelectorNoise[idx] = selector;

									if (selector < min)
									{
										min = selector;
										Console.WriteLine($"Min= {min:F3} Max={max:F3}");
									}
									
									if (selector > max)
									{
										max = selector;
										Console.WriteLine($"Min= {min:F3} Max={max:F3}");
									}
								}
							}
							
							_newQueue.Enqueue(noiseData);
						});
				}).Start();
		}

		private ConcurrentQueue<ChunkNoiseData> _newQueue = new ConcurrentQueue<ChunkNoiseData>();
		public void Add(ChunkNoiseData chunk)
		{
			_newQueue.Enqueue(chunk);
			//Chunks.Add(chunk);
		}

		private MouseState _mouseState = new MouseState();

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
						/*if (_stageDatas.TryGetValue(NoiseStage.Biomes, out var stageData))
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
						}*/
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
			GraphicsDevice.Clear(Color.Magenta);


			Stopwatch sw = Stopwatch.StartNew();

			int updates = 0;
			while (_newQueue.TryDequeue(out var chunk) && sw.ElapsedMilliseconds <= (1000f / 25))
			{
				foreach (var stage in _stageDatas)
				{
					stage.Value.AddChunk(chunk);
				}
				
				updates++;
			}

			_spriteBatch.Begin();
			
			DrawChunks(NoiseStage.TemperatureAndHumidity, new Rectangle(0,0, width * 2, height * 2));
			
			_spriteBatch.End();


			base.Draw(gameTime);
		}

		private void DrawChunks(NoiseStage noiseStage, Rectangle offset)
		{
			if (!_stageDatas.TryGetValue(noiseStage, out var stageData))
				return;
			
			_spriteBatch.Draw(stageData.Texture, offset, Color.White);
			_spriteBatch.DrawString(_spriteFont, $"Stage: {noiseStage.ToString()}", new Vector2(offset.X + 5, offset.Y + 5), Color.White);
		}

		public enum NoiseStage
		{
			TemperatureAndHumidity
		}
		
		private class NoiseStageData
		{
			private readonly NoiseStage _stage;
			private readonly GraphicsDevice _device;
			private readonly NoisePreviewGame _parent;
			public RenderTarget2D RenderTarget2D { get; }
			public Texture2D Texture { get; set; }

			private SpriteBatch _spriteBatch;
			public NoiseStageData(NoiseStage stage, GraphicsDevice device, int width, int height, NoisePreviewGame parent)
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

			public void AddChunk(in ChunkNoiseData data)
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
			
			private void DrawChunk(in ChunkNoiseData data)
			{
				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 16; y++)
					{
						var index = NoiseMap.GetIndex(x, y);
						var pixelPosition = new Rectangle(x, y, 1, 1);

						switch (_stage)
						{
							case NoiseStage.TemperatureAndHumidity:
							{
								/*var biome = _parent._worldGen.BiomeRegistry.GetBiome(data.GetBiome(x, y));*/
								var temperature = data.Temperatures[index];
								var humidity = data.Humidity[index];
								var selector = data.SelectorNoise[index];
								//new Color(temperature / 2f, 0f, humidity)
								_spriteBatch.Draw(
									_pixel, pixelPosition, new Color(selector, selector, selector));
							} break;
						}
					}
				}
			}
		}
		
		public struct ChunkNoiseData
		{
			public int X;
			public int Z;
			public float[] Temperatures;
			public float[] Humidity;
			public float[] SelectorNoise;
		}
	}
}