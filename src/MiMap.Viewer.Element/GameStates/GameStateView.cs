using System.Numerics;
using ElementEngine;
using ElementEngine.ECS;
using ElementEngine.UI;
using ImGuiNET;
using MiMap.Viewer.Element.Components;
using MiMap.Viewer.Element.Graphics;
using MiMap.Viewer.Element.MiMapTiles;
using MiMap.Viewer.Element.Systems;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace MiMap.Viewer.Element.GameStates
{
    public class GameStateView : GameState
    {
        public IWorldGenerator WorldGenerator;
        public MiMapTilesWorld TilesWorld;
        public Texture2D TilesTexture;
        public MiMapTilesRenderer TilesRenderer;
        public Game Game { get; }
        public Camera2D Camera;
        public Registry Registry;
        public SpriteFont Font;
        public SpriteBatch2D SpriteBatch;

        public Group DrawableGroup;
        public Group MovementGroup;
        public Group FourDirectionSpriteGroup;

        public Entity Player;
        public BiomeRegistry BiomeRegistry;
        public Entity Tilemap;
        public EntityBuilder EntityBuilder;

        public GameStateView(Game game)
        {
            Game = game;
            SpriteBatch = new SpriteBatch2D();
            BiomeRegistry = new BiomeRegistry();
            Camera = new Camera2D(new Rectangle(0, 0, Game.Window.Width, Game.Window.Height));
            Camera.Zoom = _zoomLevels[_zoomIndex];
        }

        public override void Initialize()
        {
        }

        public override void Load()
        {
            Registry = new Registry();
            DrawableGroup = Registry.RegisterGroup<TransformComponent, DrawableComponent>();
            MovementGroup = Registry.RegisterGroup<TransformComponent>();
            FourDirectionSpriteGroup = Registry.RegisterGroup<FourDirectionComponent, PhysicsComponent, DrawableComponent>();

            Tilemap = Registry.CreateEntity();
            Tilemap.TryAddComponent(new TilemapComponent()
            {
            });
            
            TilesTexture = BiomeTileTexture.Generate(BiomeRegistry);
            EntityBuilder = new EntityBuilder(Registry);

            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            Player = EntityBuilder.CreatePlayer(Vector2I.Zero);
            
            ref var playerTransform = ref Player.GetComponent<TransformComponent>();
            playerTransform.Position = Vector2.Zero;

            WorldGenerator = new OverworldGeneratorV2();

            TilesWorld = new MiMapTilesWorld(WorldGenerator);

            TilesRenderer = new MiMapTilesRenderer(TilesWorld, TilesTexture, Tilemap);

            TilesWorld.StartBackgroundGeneration();
        }

        private float _dt;

        public override void Update(GameTimer gameTimer)
        {
            _dt += gameTimer.DeltaMS;

            if (_dt > 250f)
            {
                _dt = 0f;
                var r = Camera.ScaledView;
                TilesWorld.GenerateMissingChunks(new Rectangle((r.X >> 4)-1, (r.Y >> 4)-1, 2+(r.Width >> 4), 2+(r.Height >> 4)));
            }
            
            PlayerSystems.ControllerMovementSystem(Player);

//            GeneralSystems.VisionSystem(Player, Tilemap, GuardVisibleGroup, LootGroup);
            GeneralSystems.FourDirectionSystem(FourDirectionSpriteGroup, gameTimer);
            Registry.SystemsFinished();
            Camera.Center(Player.GetComponent<TransformComponent>().TransformedPosition.ToVector2I());

            //Camera.Update(gameTimer);
            TilesWorld.Update();
            TilesRenderer.Update(gameTimer);
        }

        private Vector2 _dbgPosition;
        private int[] _dbgScaledView;

        public override void Draw(GameTimer gameTimer)
        {
            SpriteBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());
            TilesRenderer.DrawLayers(0, 0, Camera);
            GeneralSystems.DrawableSystem(DrawableGroup, SpriteBatch, Camera);
            SpriteBatch.End();
            
            // ImGui.Begin("Windows", ImGuiWindowFlags.AlwaysAutoResize);
            //
            // if (ImUtil.BeginTableEx("View", 2))
            // {
            //     if (Camera != default)
            //     {
            //         _dbgPosition = new Vector2(Camera.Position.X, Camera.Position.Y);
            //         ImUtil.TableRowEx("Camera Position");
            //         ImGui.TableNextColumn();
            //
            //         var position = Camera.Position;
            //         ImGui.InputFloat2(null, ref position, null, ImGuiInputTextFlags.ReadOnly);
            //
            //         ImUtil.TableRowEx("Camera ScaledView");
            //         ImGui.TableNextColumn();
            //         var scaledView = Camera.ScaledView;
            //         var scaledViewInts = new int[]
            //         {
            //             scaledView.X,
            //             scaledView.Y,
            //             scaledView.Width,
            //             scaledView.Height,
            //         };
            //         ImUtil.TableRowEx("Camera Center", () => ImGui.InputInt4(null, ref scaledViewInts[0], ImGuiInputTextFlags.ReadOnly));
            //     }
            //     
            //     ImGui.EndTable();
            // }
            //
            // ImGui.End();
        }
        protected int _zoomIndex = 4;
        protected float[] _zoomLevels = new float[]
        {
            3f,
            2.5f,
            2f,
            1.5f,
            1f,
            0.75f,
            0.5f,
            0.4f,
            0.3f,
            0.25f,
            //0.125f,
        };

        public override void HandleGameControl(string controlName, GameControlState state, GameTimer gameTimer)
        {
            var mousePosition = InputManager.MousePosition;

            switch (controlName)
            {
                case "ZoomIn":
                    if (state == GameControlState.Released || state == GameControlState.WheelUp)
                    {
                        _zoomIndex -= 1;
                        if (_zoomIndex < 0)
                            _zoomIndex = 0;

                        Camera.Zoom = _zoomLevels[_zoomIndex];
                    }
                    break;

                case "ZoomOut":
                    if (state == GameControlState.Released || state == GameControlState.WheelDown)
                    {
                        _zoomIndex += 1;
                        if (_zoomIndex >= _zoomLevels.Length)
                            _zoomIndex = _zoomLevels.Length - 1;

                        Camera.Zoom = _zoomLevels[_zoomIndex];
                    }
                    break;
            }
        }

        protected bool _dragging = false;
        protected Vector2 _dragMousePosition = Vector2.Zero;
        public override void HandleMouseMotion(Vector2 mousePosition, Vector2 prevMousePosition, GameTimer gameTimer)
        {
            if (_dragging)
            {
                var difference = mousePosition - _dragMousePosition;
                difference /= Camera.Zoom;
                Camera.Position -= difference;

                _dragMousePosition = mousePosition;
            }
        }
    }
}