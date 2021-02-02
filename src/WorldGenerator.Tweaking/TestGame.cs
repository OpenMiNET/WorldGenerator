using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
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
		public float[]     Temperature = new float[256];
		public float[]     Humidity    = new float[256];
		public ChunkColumn Chunk;

		public int X;
		public int Z;
	}
	
	public class TestGame : Game
	{
		public  ConcurrentBag<NoiseData> Chunks { get; } = new ConcurrentBag<NoiseData>();
		private int                      _chunks, _resolution;

		private GraphicsDeviceManager _graphics;

		private OverworldGeneratorV2   _worldGen;
		private Dictionary<int, Color> _biomeColors = new Dictionary<int, Color>();
		public TestGame(OverworldGeneratorV2 worldGen, int chunks, int resolution)
		{
			_worldGen = worldGen;
			_chunks = chunks;
			_resolution = resolution;
			_graphics = new GraphicsDeviceManager(this);

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

		private RenderTarget2D _humidityMap, _heatmap, _heightMap, _combined;
		private Texture2D      _pixel;
		private SpriteBatch    _spriteBatch;

		/// <inheritdoc />
		protected override void LoadContent()
		{
			base.LoadContent();

			var width  = _chunks * 16;
			var height = _chunks * 16;

			_humidityMap = new RenderTarget2D(GraphicsDevice, width, height);
			_heatmap = new RenderTarget2D(GraphicsDevice, width, height);
			_heightMap = new RenderTarget2D(GraphicsDevice, width, height);
			_combined = new RenderTarget2D(GraphicsDevice, width, height);
			
			_pixel = new Texture2D(GraphicsDevice, 1, 1);
			_pixel.SetData(new[] {Color.White});

			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		public  object      Lock = new object();
		private NoiseData[] _noise = new NoiseData[0];
		private Stopwatch _sw = Stopwatch.StartNew();

		/// <inheritdoc />
		protected override void Draw(GameTime gameTime)
		{
			var width  = GraphicsDevice.Viewport.Width / 2;
			var height = GraphicsDevice.Viewport.Height / 2;
			GraphicsDevice.Clear(Color.Black);


			if (_sw.ElapsedMilliseconds >= 250 && Monitor.TryEnter(Lock, 1))
			{
				_noise = Chunks.ToArray();

				DrawChunks(_noise, _heatmap, RenderStage.Temperature);
				DrawChunks(_noise, _humidityMap, RenderStage.Humidity);
				DrawChunks(_noise, _heightMap, RenderStage.Height);
				DrawChunks(_noise, _combined, RenderStage.HumidityAndTemperature);

				Monitor.Exit(Lock);
				_sw.Restart();
			}

			//using (SpriteBatch sb = new SpriteBatch(GraphicsDevice))
			{
				_spriteBatch.Begin();

				_spriteBatch.Draw(
					_heatmap, new Rectangle(0, 0, width, height),
					Color.White);
				
				_spriteBatch.Draw(
					_humidityMap, new Rectangle(0, height, width, height),
					Color.White);

				_spriteBatch.Draw(
					_heightMap, new Rectangle(width, 0, width, height),
					Color.White);
				
				_spriteBatch.Draw(
					_combined, new Rectangle(width, height, width, height),
					Color.White);

				_spriteBatch.End();
				//}

				base.Draw(gameTime);
			}
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

		private void DrawChunks(NoiseData[] chunks, RenderTarget2D target, RenderStage renderStage)
		{
			GraphicsDevice.SetRenderTarget(target);

			//using (SpriteBatch sb = new SpriteBatch(GraphicsDevice))
			{
				_spriteBatch.Begin();

				foreach (var data in chunks)
				{
					var column = data.Chunk;
					var worldX = data.X << 4;
					var worldZ = data.Z << 4;

					for (int cx = 0; cx < 16; cx++)
					{
						var rx = (data.X << 4) + (cx * _resolution);

						for (int cz = 0; cz < 16; cz++)
						{
							var rz = (data.Z << 4) + (cz * _resolution);

							var pixelPosition = new Rectangle(rx, rz, _resolution, _resolution);
							var biome         = _worldGen.BiomeProvider.GetBiome(column.GetBiome(cx, cz));
							
						var temp  = (int) Math.Max(0, Math.Min(255, (255 * (biome.Temperature / 2f))));
						var humid = (int) Math.Max(0, Math.Min(255, (255 * biome.Downfall)));
						
							/*var t    = data.Temperature[NoiseMap.GetIndex(cx, cz)];
							var temp = (int) Math.Max(0,
								Math.Min(255, (255 * (t / 2f))));

							var r = data.Humidity[NoiseMap.GetIndex(cx, cz)];// MathF.Abs(_worldGen.RainfallNoise.GetValue(rx, rz));
							var humid = (int) Math.Max(0,
								Math.Min(255, (255 * r)));*/

							switch (renderStage)
							{
								case RenderStage.Humidity:
									_spriteBatch.Draw(_pixel, pixelPosition, new Color(humid, humid, humid));
									break;

								case RenderStage.Temperature:
									_spriteBatch.Draw(_pixel, pixelPosition, new Color(temp, 0, humid));
									break;

								case RenderStage.Height:
									int height = column.GetHeight(cx, cz);
									_spriteBatch.Draw(_pixel, pixelPosition, new Color(height, height, height));
								//	_spriteBatch.Draw(_pixel, pixelPosition, new Color(temp, temp, temp));
									break;
								case RenderStage.HumidityAndTemperature:
								//	var biome = _worldGen.BiomeProvider.GetBiome(t, r);
								//	var c = biome.Color.GetValueOrDefault(System.Drawing.Color.White);
									if (_biomeColors.TryGetValue(biome.Id, out var c))
									{
										_spriteBatch.Draw(_pixel, pixelPosition, new Color(c.R, c.G, c.B));
									}

									break;
							}

							/*var t = WorldGen.TemperatureNoise.GetValue(rx, rz) + 1f;
							var r = MathF.Abs(WorldGen.RainfallNoise.GetValue(rx, rz));
							        
							var humid = (int) Math.Max(0,
							    Math.Min(255, (255 * r)));
							        
							humidityMap.SetPixel(rx, rz, Color.FromArgb(humid, humid, humid));
							        
							var temp = (int) Math.Max(0,
							    Math.Min(255, (255 * (t / 2f))));
							        
							heatmap.SetPixel(rx, rz, Color.FromArgb(temp, temp, temp));*/


							// humidityMap.SetPixel(rx, rz, Color.FromArgb(humid, humid, humid));
							// heatmap.SetPixel(rx, rz, Color.FromArgb(temp, temp, temp));

							// chunkHeight.SetPixel(rx, rz, Color.FromArgb(height, height, height));

							/*height = (int) Math.Max(0,
							    Math.Min(255,
							        (255 * MathUtils.ConvertRange(-2f, 2f, 0f, 1f,
							             ((biome.MinHeight + biome.MaxHeight) / 2f)))));*/

							// heightmap.SetPixel(rx, rz, Color.FromArgb(height, height, height));
						}
					}
					
					//DrawBorder(new Rectangle(worldX,worldZ, 16, 16), 1, Color.Magenta);
				}

				DrawBorder(new Rectangle(0,0, target.Width, target.Height), 1, Color.Red);
;				_spriteBatch.End();
			}

			GraphicsDevice.SetRenderTarget(null);
		}

		public enum RenderStage
		{
			Humidity,
			Temperature,
			Height,
			HumidityAndTemperature
		}
	}
}