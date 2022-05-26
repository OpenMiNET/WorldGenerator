using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using log4net;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
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
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Utils
{
	public class BiomeUtils
	{
		public static BiomeBase[] Biomes =
		{
			new OceanBiome(), // default values of temp and rain
			new PlainsBiome(), 
			new DesertBiome(), 
			new ExtremeHillsBiome(), 
			new ForestBiome(), 
			new TaigaBiome(), 
			new SwamplandBiome(), //TODO!!!
			new RiverBiome(), // default values of temp and rain
			/*new Biome
			{
				Id = 8,
				Name = "Nether",
				Temperature = 2.0f,
				Downfall = 0.0f,
				MinHeight = 0.1f,
				MaxHeight = 0.2f, //TODO!
			},*/
			/*new Biome
			{
				Id = 9,
				Name = "End",
				Temperature = 0.5f,
				Downfall = 0.5f,
				MinHeight = 0.1f,
				MaxHeight = 0.2f, //TODO!
			},*/ // default values of temp and rain
			new FrozenOceanBiome(), 
			new FrozenRiverBiome(), //TODO
			new IcePlainsBiome(), //TODO
			/*new Biome
			{
				Id = 13,
				Name = "Ice Mountains",
				Temperature = 0.0f,
				Downfall = 0.5f,
				MinHeight = 0.2f,
				MaxHeight = 1.2f
			},*/
			new MushroomIslandBiome(), //TODO
			new MushroomIslandShoreBiome(), //TODO
			new BeachBiome(), 
			new DesertHillsBiome(), 
			new ForestHillsBiome(),
			new TaigaHillsBiome(), 
			new ExtremeHillsEdgeBiome(), 
			new JungleBiome(),
			new JungleHillsBiome(),
			
			//TODO: The rest of min/max
			new JungleEdgeBiome(), 
			new DeepOceanBiome(),
			new StoneBeachBiome(),
			new ColdBeachBiome(),
			new BirchForestBiome(),
			new BirchForestHillsBiome(),
			new Biome
			{
				Id = 29,
				Name = "Roofed Forest",
				Temperature = 0.7f,
				Downfall = 0.8f,
				MinHeight = 0.1f,
				MaxHeight = 0.2f,
				Terrain = new ForestTerrain()
			},
			/*new Biome
			{
				Id = 30,
				Name = "Cold Taiga",
				Temperature = -0.5f,
				Downfall = 0.4f,
				MinHeight = 0.2f,
				MaxHeight = 0.2f
			},
			new Biome
			{
				Id = 31,
				Name = "Cold Taiga Hills",
				Temperature = -0.5f,
				Downfall = 0.4f,
				MinHeight = 0.3f,
				MaxHeight = 0.45f
			},
			new Biome
			{
				Id = 32,
				Name = "Mega Taiga",
				Temperature = 0.3f,
				Downfall = 0.8f,
				MinHeight = 0.2f,
				MaxHeight = 0.2f
			},
			new Biome
			{
				Id = 33,
				Name = "Mega Taiga Hills",
				Temperature = 0.3f,
				Downfall = 0.8f,
				MinHeight = 0.3f,
				MaxHeight = 0.45f
			},*/
			new Biome
			{
				Id = 34,
				Name = "Extreme Hills+",
				Temperature = 0.2f,
				Downfall = 0.3f,
				MinHeight = 0.5f,
				MaxHeight = 1f,
				Terrain = new RidgedExtremeHillsTerrain(150f, 67f, 200f)
			},
			new SavannaBiome(),
			new SavannaPlateauBiome(),
			new MesaBiome(),
			new MesaPlateauFBiome(), 
			new MesaPlateauBiome(),
		//	new Biome {Id = 127, Name = "The Void", Temperature = 0.8f, Downfall = 0.4f},
		//	new Biome {Id = 128, Name = "Unknown Biome", Temperature = 0.8f, Downfall = 0.4f},
		//	new Biome {Id = 129, Name = "Sunflower Plains", Temperature = 0.8f, Downfall = 0.4f},
		
			new Biome
			{
				Id = 130,
				Name = "Desert M",
				Temperature = 2.0f,
				Downfall = 0.0f,

				SurfaceBlock = 12,
				SoilBlock = 24,
				
				Terrain = new DesertTerrain() //Change to Desert M
			},
			new Biome
			{
				Id = 131,
				Name = "Extreme Hills M",
				Temperature = 0.2f,
				Downfall = 0.3f,
				MinHeight = 0.2f,
				MaxHeight = 0.8f,
				Terrain = new RidgedExtremeHillsTerrain(190f, 67f, 200f)
			},
			
		/*	new Biome {Id = 132, Name = "Flower Forest", Temperature = 0.7f, Downfall = 0.8f},
			new Biome {Id = 133, Name = "Taiga M", Temperature = 0.05f, Downfall = 0.8f},
			new Biome {Id = 134, Name = "Swampland M", Temperature = 0.8f, Downfall = 0.9f},
			new Biome {Id = 140, Name = "Ice Plains Spikes", Temperature = 0.0f, Downfall = 0.5f},
			new Biome {Id = 149, Name = "Jungle M", Temperature = 1.2f, Downfall = 0.9f},
			new Biome {Id = 150, Name = "Unknown Biome", Temperature = 0.8f, Downfall = 0.4f},
			new Biome {Id = 151, Name = "JungleEdge M", Temperature = 0.95f, Downfall = 0.8f},
			new Biome {Id = 155, Name = "Birch Forest M", Temperature = 0.6f, Downfall = 0.6f},*/
			/*new Biome
			{
				Id = 156,
				Name = "Birch Forest Hills M",
				Temperature = 0.6f,
				Downfall = 0.6f,
				MinHeight = 0.2f,
				MaxHeight = 0.8f
			},
			/*new Biome {Id = 157, Name = "Roofed Forest M", Temperature = 0.7f, Downfall = 0.8f},
			new Biome {Id = 158, Name = "Cold Taiga M", Temperature = -0.5f, Downfall = 0.4f},
			new Biome {Id = 160, Name = "Mega Spruce Taiga", Temperature = 0.25f, Downfall = 0.8f},*
			// special exception, temperature not 0.3
			new Biome
			{
				Id = 161,
				Name = "Mega Spruce Taiga Hills",
				Temperature = 0.3f,
				Downfall = 0.8f,
				MinHeight = 0.2f,
				MaxHeight = 0.8f
			},*/
			/*new Biome {Id = 162, Name = "Extreme Hills+ M", Temperature = 0.2f, Downfall = 0.3f},
			new Biome {Id = 163, Name = "Savanna M", Temperature = 1.2f, Downfall = 0.0f},
			new Biome {Id = 164, Name = "Savanna Plateau M", Temperature = 1.0f, Downfall = 0.0f},
			new Biome {Id = 165, Name = "Mesa (Bryce)", Temperature = 2.0f, Downfall = 0.0f},
			new Biome {Id = 166, Name = "Mesa Plateau F M", Temperature = 2.0f, Downfall = 0.0f},
			new Biome {Id = 167, Name = "Mesa Plateau M", Temperature = 2.0f, Downfall = 0.0f},*/
		};

		public BiomeBase GetBiome(int biomeId)
		{
			return Biomes.FirstOrDefault(biome => biome.Id == biomeId) ?? new Biome { Id = biomeId };
		}

		public static BiomeBase GetBiomeById(int biomeId)
		{
			return Biomes.FirstOrDefault(biome => biome.Id == biomeId) ?? new Biome { Id = biomeId };
		}
	}
}