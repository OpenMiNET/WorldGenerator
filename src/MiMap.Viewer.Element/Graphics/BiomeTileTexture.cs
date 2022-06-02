using ElementEngine;
using ElementEngine.TexturePacker;
using MiNET.Worlds;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;
using SixLabors.ImageSharp;
using Veldrid;

namespace MiMap.Viewer.Element.Graphics
{
    public static class BiomeTileTexture
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public static int Width { get; private set; }
        public static int Height { get; private set; }

        public static Texture2D Generate(BiomeRegistry registry)
        {
            var biomes = registry.GetBiomes().ToDictionary(ks => ks.Id, vs => vs);
            var width = Math.Max(ChunkColumn.WorldHeight, biomes.Keys.Max() + 1);
            var height = 4;

            Width = width;
            Height = height;

            RgbaByte[] biomeColors = new RgbaByte[width * height];

            float f;
            for (int i = 0; i < width; i++)
            {
                if (biomes.TryGetValue(i, out var biome))
                {
                    var temp = biome.Temperature / 2f;
                    biomeColors[(0 * width) + i] = biome.Color.GetValueOrDefault(System.Drawing.Color.Black).ToRgbaByte();
                    biomeColors[(1 * width) + i] = new RgbaFloat(0f, 0f, biome.Downfall, biome.Downfall).ToRgbaByte();
                    biomeColors[(2 * width) + i] = new RgbaFloat(temp, 0f, 0f, temp).ToRgbaByte();
                }
                else
                {
                    biomeColors[(0 * width) + i] = RgbaByte.Clear;
                    biomeColors[(1 * width) + i] = RgbaByte.Clear;
                    biomeColors[(2 * width) + i] = RgbaByte.Clear;
                }

                if (i < ChunkColumn.WorldHeight)
                {
                    f = ((float)i / ChunkColumn.WorldHeight);
                    biomeColors[(3 * width) + i] = new RgbaFloat(f, f, f, f).ToRgbaByte();
                }
                else
                {
                    biomeColors[(3 * width) + i] = RgbaByte.Black;
                }
            }

            var atlas = new Texture2D(new Vector2I(width, height), "BiomeTiles", PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging);
            atlas.SetData<RgbaByte>(biomeColors.AsSpan());

            var path = Path.GetTempFileName();
            atlas.SaveAsPng(path);
            Logging.Information($"Saved biome tile texture to '{path}'");

            atlas.Dispose();
            return AssetManager.Instance.LoadTexture2DFromPath(path, TexturePremultiplyType.None, "BiomeTiles");

            return atlas;
        }
    }
}