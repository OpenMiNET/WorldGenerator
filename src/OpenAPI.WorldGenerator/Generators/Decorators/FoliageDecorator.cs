using System;
using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Structures;
using OpenAPI.WorldGenerator.Utils.Noise;
using OpenAPI.WorldGenerator.Utils.Noise.Primitives;
using Biome = OpenAPI.WorldGenerator.Generators.Biomes.Biome;
using Structure = OpenAPI.WorldGenerator.Generators.Structures.Structure;

namespace OpenAPI.WorldGenerator.Generators.Decorators
{
    public class FoliageDecorator : ChunkDecorator
	{
		private WorldGeneratorPreset Preset { get; }
		public FoliageDecorator(WorldGeneratorPreset settings)
		{
			Preset = settings;
		}
		
		public static readonly string[] FlowerTypes = new []{"cornflower", "houstonia", "lily_of_the_valley", "allium", "tulip_pink", "tulip_red", "poppy", "tulip_white", "tulip_orange", "oxeye", "orchid"};

		private static readonly int SugerCaneId = new Reeds().GetRuntimeId();
		private static readonly int TallGrassId = new Tallgrass().GetRuntimeId();
		private static readonly int WaterBlockId = new Water().Id;
		private static readonly int JungleLeavesId = new Leaves() {OldLeafType = "jungle"}.GetRuntimeId();
		public override void Decorate(ChunkColumn column,
			int chunkX,
			int chunkZ,
			BiomeBase biome,
			float[] thresholdMap,
			int x,
			int y,
			int z,
			bool surface,
			bool isBelowMaxHeight)
		{

			var currentTemperature = biome.Temperature;

			if (y > Preset.SeaLevel)
			{
				var distanceToSeaLevel = y - Preset.SeaLevel;
				currentTemperature = biome.Temperature - (0.00166667f * distanceToSeaLevel);
			}

			int rx = chunkX * 16 + x;
			int rz = chunkZ * 16 + z;

			bool generated = false;

			if (surface && y >= Preset.SeaLevel)
			{
				var m = Math.Min(biome.Downfall * 0.32f, 0.03f);
				var noise = Simplex.GetValue(rx * m, rz * m);

				if (x >= 3 && x <= 13 && z >= 3 && z <= 13)
				{
					Structure tree = null;

					//if (biome.Config.)
					if (biome.Downfall <= 0f && biome.Temperature >= 2f)
					{
						if (GetRandom(64) == 16)
						{
							var randValue = GetRandom(3);
							tree = new CactusStructure(randValue);
						}
					}

					if (tree == null && biome.Downfall > 0 && (noise > (0.5f + (y / 512f))))
					{
						if (currentTemperature >= 1f && biome.Downfall >= 0.4f)
						{
							if (GetRandom(8) == 4)
							{
								tree = new LargeJungleTree();
							}
							else
							{
								tree = new SmallJungleTree();
							}
						}
						/*	else if (currentTemperature >= 0.7F && biome.Downfall >= 0.2f)
							{
								tree = new OakTree(true);
							}*/
						else if (currentTemperature >= 0.7F && biome.Downfall < 0.2f)
						{
							tree = new AcaciaTree();
						}
						else if (currentTemperature > 0.25f && biome.Downfall > 0f)
						{
							if (biome.Name.Contains("Birch", StringComparison.InvariantCultureIgnoreCase) || GetRandom(16) == 8)
							{
								tree = new BirchTree();
							}
							else
							{
								tree = new OakTree();
							}
						}
						else if (currentTemperature <= 0.25f && biome.Downfall > 0f)
						{
							tree = new PineTree();
						}
					}

					if (tree != null)
					{
						if (y + 1 < 254)
						{
							if (tree.CanCreate(column, x, y, z))
							{
								tree.Create(column, x, y + 1, z);
							}
						}

						generated = true;
					}
				}

				if (!generated)
				{
					if (noise > 0.5) //Threshold 1
					{
						if (currentTemperature > 0.3f && currentTemperature < 1.5f && biome.Downfall >= 0.85f)
						{
							column.SetBlockByRuntimeId(x, y + 1, z, JungleLeavesId); //Leaves
						}
						else if (currentTemperature > 0.3f && currentTemperature < 1.5f && biome.Downfall > 0)
						{
							var blockBeneath = column.GetBlockId(x, y, z); // column.GetBlock(x, y, z);
							
							if ( /*sugarPosibility <= 11*/ noise > 0.75f
							                               && (blockBeneath == 3 || blockBeneath == 2
								                               || blockBeneath == 12) && IsValidSugarCaneLocation(
								                               column, x, y, z))
							{
								var sugarPosibility = GetRandom(18);
								int height = 1;

								if (sugarPosibility <= 2)
								{
									height = 3;
								}
								else if (sugarPosibility <= 5)
								{
									height = 2;
								}

								//var growth = Rnd.Next(0x1, 0x15);
								for (int mY = y + 1; mY < y + 1 + height; mY++)
								{
									column.SetBlockByRuntimeId(x, mY, z, SugerCaneId); //SugarCane
									//blocks[OverworldGenerator.GetIndex(x, mY, z)] = 83;

									if (mY == y + 1 + height)
									{
										//metadata[OverworldGenerator.GetIndex(x, mY, z)] = (byte) Rnd.Next(0, 15);
										//column.SetMetadata(x, mY, z, (byte) Rnd.Next(0, 15));
									}
									else
									{
										//metadata[OverworldGenerator.GetIndex(x, mY, z)] = (byte) 0;
										//column.SetMetadata(x, mY, z, 0);
									}
								}
							}
							else if (noise > 0.8 && (blockBeneath == 3
							         || blockBeneath == 2)) //If above 0.8, we generate flowers :)
							{
								if (Simplex.GetValue(rx * 0.5f, rz * 0.5f) > 0.5)
								{
									column.SetBlockByRuntimeId(
										x, y + 1, z,
										new RedFlower() {FlowerType = FlowerTypes[GetRandom(FlowerTypes.Length - 1)]}
										   .GetRuntimeId());
									//blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 38;
									//metadata[OverworldGenerator.GetIndex(x, y + 1, z)] = (byte) GetRandom(8);
									//column.SetBlock(x, y + 1, z, 38); //Poppy
									//column.SetMetadata(x, y + 1, z, (byte) GetRandom(8));
								}
								else
								{
									//	blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 37;
									//	column.SetBlock(x, y + 1, z, new Dan());
									//	column.SetBlock(x, y + 1, z, 37); //Dandelion
								}
							}
							else if (blockBeneath == 3 || blockBeneath == 2)
							{
								column.SetBlockByRuntimeId(x, y + 1, z, TallGrassId);
								//blocks[OverworldGenerator.GetIndex(x, y + 1, z)] = 31;
								//metadata[OverworldGenerator.GetIndex(x, y + 1, z)] = (byte) 1;
								//column.SetBlock(x, y + 1, z, 31); //Grass
								//column.SetMetadata(x, y + 1, z, 1);
							}
						}

					}
				}
			}
		}

		private bool IsValidSugarCaneLocation(ChunkColumn column, int x, int y, int z)
		{
			if (y - 1 <= 0) return false;
			if (x - 1 >= 0 && x + 1 < 16)
			{
				if (z - 1 >= 0 && z + 1 < 16)
				{
					for (int mx = -1; mx < 1; mx++)
					{
						for (int mz = -1; mz < 1; mz++)
						{
							if (column.GetBlockId(x + mx, y, mz + z) == WaterBlockId)
							{
								return true;
							}
						}
					}
					/*if (blocks[OverworldGenerator.GetIndex(x + 1, y, z)] == 8 
						|| blocks[OverworldGenerator.GetIndex(x - 1, y, z)] == 8 
						          || blocks[OverworldGenerator.GetIndex(x, y, z + 1)] == 8 
						          || blocks[OverworldGenerator.GetIndex(x, y, z - 1)] == 8)
					{
						return true;
					}*/
				}
			}
			return false;
		}

		private int GetRandom(int max)
		{
			return Rnd.Next(max);
		}

		private FastRandom Rnd { get; set; }
		private INoiseModule Simplex { get; set; }
		protected override void InitSeed(int seed)
		{
			Simplex = new SimplexPerlin(seed, NoiseQuality.Fast);
		//	Simplex.SetScale(1.5);

			Rnd = new FastRandom(seed);
		}
	}
}