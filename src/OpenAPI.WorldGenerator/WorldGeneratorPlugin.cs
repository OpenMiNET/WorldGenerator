using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Blocks;
using MiNET.Effects;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Entities;
using OpenAPI.Events;
using OpenAPI.Events.Player;
using OpenAPI.Player;
using OpenAPI.Plugins;
using OpenAPI.World;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace OpenAPI.WorldGenerator
{
    [OpenPluginInfo(Name = "OpenAPI WorldGenerator", Description = "Provides an alternative world generator for MiNET", Author = "Kenny van Vulpen", Version = "1.0", Website = "https://github.com/OpenMiNET/WorldGenerator")]
    public class WorldGeneratorPlugin : OpenPlugin, IEventHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WorldGeneratorPlugin));

        private OpenApi Api { get; set; }
        private Timer _timer { get; set; }
      //  private WorldGeneratorPreset Preset { get; }
        private BiomeRegistry _biomeRegistry = new BiomeRegistry();
        public WorldGeneratorPlugin()
        {
        }

        [EventHandler(EventPriority.Monitor)]
        public void OnPlayerJoin(PlayerSpawnedEvent e)
        {
            SetSpeed(e.Player);
        }

        private void SetSpeed(OpenPlayer player)
        {
            return;
           player.SetEffect(new Speed()
            {
                Duration = 320,
                Level = 10,
                Particles = false,
            });
        }

        private void Callback(object state)
        {
            if (Api == null) return;
            foreach (var player in Api.PlayerManager.GetPlayers())
            {
                if (!player.IsSpawned)
                    continue;
                
                var pos = player.KnownPosition.GetCoordinates3D();
                var chunk = player.Level.GetChunk(pos, true);

                if (chunk == null)
                    continue;
                
                var biome = chunk.GetBiome(pos.X - (chunk.X * 16), pos.Z - (chunk.Z * 16));
                BiomeBase result = null;
                if (player.Level.WorldProvider is DebugWorldProvider debugWorldProvider)
                {
                    if (debugWorldProvider.Generator is OverworldGeneratorV2 gen)
                    {
                        result = gen.BiomeRegistry.GetBiome(biome);
                    }
                }
                
                if (result == null)
                    result = _biomeRegistry.GetBiome(biome);

                SetSpeed(player);
                player.SendTitle($"Biome: {(result?.Name ?? "N/A")}, Temperature: {(result?.Temperature.ToString() ?? "N/A")}, Downfall: {(result?.Downfall.ToString() ?? "N/A")}", TitleType.ActionBar, 0, 0, 25);
            }
        }

        public override void Enabled(OpenApi api)
        {
            Api = api;
            if (Config.GetProperty("WorldGen.IsDefault", false))
            {
                IWorldGenerator generator;

                /* generator = new MiNET.Worlds.SuperflatGenerator(Dimension.Overworld)
                 {
                     BlockLayers = new List<Block>()
                     {
                         new Bedrock(),
                         new Dirt(),
                         new Dirt(),
                         new Grass()
                     }
                 };*/

                generator = new OverworldGeneratorV2();

                var level = new OpenLevel(api, api.LevelManager, Dimension.Overworld.ToString(),
                    new DebugWorldProvider(generator), api.LevelManager.EntityManager, GameMode.Creative,
                    Difficulty.Peaceful);

                api.LevelManager.LoadLevel(level);
                api.LevelManager.SetDefaultLevel(level);
            }

            api.EventDispatcher.RegisterEvents(this);

            if (Config.GetProperty("WorldGen.Debug", false))
            {
                _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1.25));
            }
        }

        public override void Disabled(OpenApi api)
        {
            _timer?.Dispose();
            _timer = null;
            
            Log.Info($"WorldGenerator plugin disabled!");
        }
    }
}