using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Beach;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Desert;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.ExtremeHills;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mushroom;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Swamp;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Taiga;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Biomes
{
    public class BiomeProvider
    {
        //public INoiseModule RainfallProvider { get; set; }
        //public INoiseModule TemperatureProvider { get; set; }
        
        public BiomeBase[] Biomes { get; }
        public BiomeProvider()
        {
            //Biomes = BiomeUtils.Biomes.Where(b => b.Terrain != null).ToArray();
            Biomes = new BiomeBase[]
            {
                new BirchForestBiome(),
                new BirchForestHillsBiome(),
                /* new BirchForestHillsMBiome(),
                 new BirchForestMBiome(),
                 new ColdTaigaBiome(),
                 new ColdTaigaHillsBiome(),
                 new ColdTaigaMBiome(),*/
                new DeepOceanBiome(),
                new DesertBiome(),
                new DesertHillsBiome(),
                //    new DesertMBiome(),
             //   new ExtremeHillsBiome(),
            //    new ExtremeHillsEdgeBiome(),
                /*    new ExtremeHillsMBiome(),
                    new ExtremeHillsPlusBiome(),
                    new ExtremeHillsPlusMBiome(),
                    new FlowerForestBiome(),*/
                new ForestBiome(),
                new ForestHillsBiome(),
                new FrozenOceanBiome(),
                // new IceMountainsBiome(),
                new IcePlainsBiome(),
                // new IcePlainsSpikesBiome(),
                new JungleBiome(),
                new JungleEdgeBiome(),
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

            Random rnd = new Random();
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
            }
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

        public BiomeBase GetBiome(float temperature, float downfall, float selector)
        {
            var biomes = GetBiomes();

            /*
             * 1. Calculate "Weight" for all biomes
             * 2. 
             */
            
            selector = 1f / MathF.Abs(selector);
            float[] weights = new float[biomes.Length];

            int totalItems = 0;
            float minDifference = float.MaxValue;
            for (int i = 0; i < biomes.Length; i++)
            {
                var temperatureDifference = biomes[i].Temperature - temperature;
                var humidityDifference = biomes[i].Downfall - downfall;

                temperatureDifference *= 12.5f;
                humidityDifference *= 3.25f;

                var difference =
                    MathF.Abs((temperatureDifference * temperatureDifference + humidityDifference * humidityDifference) );

                if (difference > 0f)
                {
                    if (difference < minDifference)
                        minDifference = difference;
                    
                    weights[i] = difference;
                    totalItems++;
                }
            }
            
            for (int biomeIndex = 0; biomeIndex < weights.Length; biomeIndex++)
            {
                if (weights[biomeIndex] > minDifference)
                {
                    weights[biomeIndex] = 0f;
                }
                else
                {
                    weights[biomeIndex] *= biomes[biomeIndex].Config.WeightMultiplier;
                }
            }
            
            float sum = weights.Sum();

            for (int biomeIndex = 0; biomeIndex < weights.Length; biomeIndex++)
            {
                weights[biomeIndex] /= sum;
            }

           // selector *= totalItems;
            // selector /= sum;
           int result = 0;

         //  while (selector > 0f)
           {
               for (int i = 0; i < weights.Length; i++)
               {
                   var value = weights[i];

                   if (value > 0f)
                   {
                       result = i;
                       selector -= value;
                       weights[i] = 0;
                       if (selector <= 0f)
                           break;
                   }
               }
           }

           return biomes[result];
        }

        public BiomeBase GetBiome(int id)
        {
            return Biomes.FirstOrDefault(x => x.Id == id);//.Where(x => x.Terrain != null && x.Surface != null).FirstOrDefault(x => x.Id == id);
        }
    }
}