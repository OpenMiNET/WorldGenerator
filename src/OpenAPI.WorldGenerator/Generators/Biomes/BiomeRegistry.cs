using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using MiNET.Blocks;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Desert;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.ExtremeHills;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mushroom;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Plains;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Savanna;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Swamp;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Taiga;
using OpenAPI.WorldGenerator.Generators.Surfaces;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public class BiomeRegistry
    {
        public BiomeBase[] Biomes { get; }
        private Dictionary<int, BiomeBase> ById { get; }
        public BiomeRegistry()
        {
            //Colors found here:
            //https://github.com/toolbox4minecraft/amidst/wiki/Biome-Color-Table
            
            //Biomes = BiomeUtils.Biomes.Where(b => b.Terrain != null).ToArray();
            Biomes = new BiomeBase[]
            {
                new BirchForestBiome(),
                new BirchForestHillsBiome(),
                 new BirchForestHillsMBiome(),
                 new BirchForestMBiome(),
               /*  new ColdTaigaBiome(),
                 new ColdTaigaHillsBiome(),
                 new ColdTaigaMBiome(),*/
                new DeepOceanBiome(),
                new DesertBiome(),
                new DesertHillsBiome(),
                //    new DesertMBiome(),
                //new ExtremeHillsBiome(),
               // new ExtremeHillsEdgeBiome(),
                /*    new ExtremeHillsMBiome(),
                    new ExtremeHillsPlusBiome(),
                    new ExtremeHillsPlusMBiome(),*/
                new FlowerForestBiome(),
                new ForestBiome(),
                new WoodedHillsBiome(),
                new FrozenOceanBiome(),
                // new IceMountainsBiome(),
                new IcePlainsBiome(),
                // new IcePlainsSpikesBiome(),
                new JungleBiome(),
                new JungleEdgeBiome().SetEdgeBiome(true),
                //  new JungleEdgeMBiome(),
                new JungleHillsBiome(),
                /*new JungleMBiome(),
                new MegaSpruceTaigaBiome(),
                new MegaTaigaBiome(),
                new MegaTaigaHillsBiome(),*/
                new MesaBiome(),
                //   new MesaBryceBiome(),
                new MesaPlateauBiome(),
                new MesaPlateauFBiome(),
               //   new MesaPlateauFMBiome(),
                //  new MesaPlateauMBiome(),
                new MushroomIslandBiome(),
                new MushroomIslandShoreBiome(),
                
                new OceanBiome(),
                
                new PlainsBiome(),
                //    new RedwoodTaigaHillsBiome(),
                //    new RoofedForestBiome(),
                //   new RoofedForestMBiome(),
                new SavannaBiome(),
                //   new SavannaMBiome(),
                new SavannaPlateauBiome(),
                //   new SavannaPlateauMBiome(),
                //   new SunflowerPlainsBiome(),
                new SwamplandBiome(),
                //   new SwamplandMBiome(),
                new TaigaBiome(),
                new TaigaHillsBiome(),
                //  new TaigaMBiome()
                
                new RiverBiome().SetEdgeBiome(true),
                new FrozenRiverBiome().SetEdgeBiome(true),
                
                new BeachBiome().SetEdgeBiome(true), 
                new ColdBeachBiome().SetEdgeBiome(true), 
                new StoneBeachBiome().SetEdgeBiome(true), 
            };

            FastRandom rnd = new FastRandom();
            for (int i = 0; i < Biomes.Length; i++)
            {
                var b = Biomes[i];
                if (Biomes[i].Terrain != null && Biomes[i].Surface == null)
                {
                    b.Surface = new SurfaceBase(b.Config, BlockFactory.GetBlockById(b.SurfaceBlock), BlockFactory.GetBlockById(b.SoilBlock));
                }

                if (!b.Color.HasValue)
                {
                    b.Color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                }

                Biomes[i] = b;
            }
            
            float minTemp = float.MaxValue;
            float maxTemp = float.MinValue;
            float minRain = float.MaxValue;
            float maxRain = float.MinValue;

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            Dictionary<int, BiomeBase> byId = new Dictionary<int, BiomeBase>();
            
            for (int i = 0; i < Biomes.Length; i++)
            {
                var biome = Biomes[i];
                var min   = MathF.Min(biome.MinHeight, biome.MaxHeight);
                var max   = MathF.Max(biome.MinHeight, biome.MaxHeight);

                minHeight = Math.Min(minHeight, min);
                maxHeight = Math.Max(maxHeight, max);

                //	min = MathUtils.ConvertRange(-2f, 2f, 0f, 128f, biome.MinHeight);
                //	max = MathUtils.ConvertRange(-2f, 2f, 0f, 128f, biome.MaxHeight);
				
                biome.MinHeight = min;
                biome.MaxHeight = max;

                if (biome.Temperature < minTemp)
                {
                    minTemp = biome.Temperature;
                }
                if (biome.Temperature > maxTemp)
                {
                    maxTemp = biome.Temperature;
                }
                if (biome.Downfall < minRain)
                {
                    minRain = biome.Downfall;
                }
                if (biome.Downfall > maxRain)
                {
                    maxRain = biome.Downfall;
                }

                Biomes[i] = biome;

                byId.TryAdd(biome.Id, biome);
            }

            foreach (var group in Biomes.Where(x => !x.Config.IsEdgeBiome).GroupBy(x => (1f + x.Temperature) * (1f + x.Downfall) * x.Config.Weight))
            {
                if (group.Count() > 1)
                {
                    Console.WriteLine($"Detected issues!");
                    foreach (var biome in group)
                    {
                        Console.WriteLine($"\t {biome.Name}");
                    }
                }
            }
            
            ById = byId;
            Console.WriteLine($"Temperature (min: {minTemp} max: {maxTemp}) Downfall (min:{minRain} max: {maxRain}) Height (min: {minHeight} max: {maxHeight})");
        }

        private BiomeBase[] _nonEdgeBiomes = null;
        public BiomeBase[] GetBiomes()
        {
            if (_nonEdgeBiomes == null)
            {
                _nonEdgeBiomes = Biomes.Where(x => x.Terrain != null && x.Surface != null && !x.Config.IsEdgeBiome)
                   .ToArray();
            }

            return _nonEdgeBiomes;
            //return Biomes.Where(x => x.Terrain != null && x.Surface != null && !x.Config.IsEdgeBiome).ToArray();
        }

        public int GetBiome(float temperature, float downfall, float selector)
        {
            var biomes = GetBiomes();
            var weights = ArrayPool<float>.Shared.Rent(biomes.Length);

            try
            {
                float sum = 0f;

                for (int i = 0; i < biomes.Length; i++)
                {
                    var temperatureDifference = (biomes[i].Temperature - temperature);
                    var humidityDifference = (biomes[i].Downfall - downfall);

                    //temperatureDifference *= 7.5f;
                    //humidityDifference *= 2.5f;

                    var weight = (float) biomes[i].Config.Weight * MathF.Abs(
                        (temperatureDifference * temperatureDifference + humidityDifference * humidityDifference));

                    if (weight > 0f)
                    {
                        weights[i] = weight;

                        sum += weight;
                    }
                }

                selector *= sum;

                float currentWeightIndex = 0;

                for (int i = 0; i < biomes.Length; i++)
                {
                    var value = weights[i];

                    if (value > 0f)
                    {
                        currentWeightIndex += value;

                        if (currentWeightIndex >= selector)
                            return biomes[i].Id;
                    }
                }

                return biomes[0].Id;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(weights, true);
            }
        }

        public BiomeBase GetBiome(int id)
        {
            if (ById.TryGetValue(id, out var value))
                return value;

            return default;
        }
    }
}