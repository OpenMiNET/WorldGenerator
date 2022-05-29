// /*
//  * MiMap.Viewer.DesktopGL
//  *
//  * Copyright (c) 2020 Dan Spiteri
//  *
//  * /

using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiMap.Viewer.DesktopGL.Graphics;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;
using static ImGuiNET.ImGui;

namespace MiMap.Viewer.DesktopGL.Components
{
    public class GuiMapViewer : DrawableGameComponent
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private Point _position = Point.Zero;
        private Rectangle _mapBounds;
        private float _scale = 1f;
        private readonly object _chunkSync = new object();
        private List<CachedRegionMesh> _regions;
        private List<MapChunk> _chunks;
        private bool _chunksDirty = false;
        private Map _map;
        private Point _mapPosition;
        private Matrix _transform;
        private int _drawOrder = 1;
        private bool _visible = true;
        private IRegionMeshManager _regionMeshManager;
        private BasicEffect _effect;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private ImGuiRenderer _gui;
        private readonly RasterizerState _rasterizerState;
        private Rectangle _bounds;
        private MouseState _mouseState;
        private int[] _cursorBlock = new int[3];
        private BiomeBase _cursorBlockBiome;
        private int[] _cursorChunk = new int[2];
        private int[] _cursorRegion = new int[2];
        private int _cursorBlockBiomeId;

        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                RecalculateTransform();
            }
        }

        public Point MapPosition
        {
            get => _mapPosition;
            set
            {
                if (_mapPosition == value) return;
                _mapPosition = value;
                RecalculateTransform();
            }
        }

        public Rectangle MapBounds
        {
            get => _mapBounds;
            private set
            {
                if (_mapBounds == value)
                    return;

                _mapBounds = value;
                OnMapBoundsChanged();
            }
        }

        public Map Map
        {
            get => _map;
            set
            {
                if (_map != default)
                {
                    _map.RegionGenerated -= OnRegionGenerated;
                }

                _map = value;

                if (_map != default)
                {
                    _map.RegionGenerated += OnRegionGenerated;
                }
            }
        }

        public Matrix Transform
        {
            get => _transform;
            private set
            {
                _transform = value;
                InverseTransform = Matrix.Invert(value);
            }
        }

        private Matrix InverseTransform { get; set; }

        public GuiMapViewer(MiMapViewer game, Map map) : base(game)
        {
            Map = map;
            _regions = new List<CachedRegionMesh>();
            _chunks = new List<MapChunk>();
            _regionMeshManager = game.RegionMeshManager;
            _gui = game.ImGuiRenderer;
            _rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                ScissorTestEnable = true,
                DepthClipEnable = false,
                MultiSampleAntiAlias = true
            };
        }

        public override void Initialize()
        {
            base.Initialize();

            _effect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = false,
                VertexColorEnabled = false,
                TextureEnabled = true,
                FogEnabled = false
            };

            Game.GraphicsDevice.DeviceReset += (s, o) => UpdateBounds();
            Game.Activated += (s, o) => UpdateBounds();
            Game.Window.ClientSizeChanged += (s, o) => UpdateBounds();
            UpdateBounds();

            RecalculateTransform();
            _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), 6, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(new[]
            {
                new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f)),
                new VertexPositionTexture(new Vector3(512f, 0f, 0f), new Vector2(1f, 0f)),
                new VertexPositionTexture(new Vector3(0f, 512f, 0f), new Vector2(0f, 1f)),
                new VertexPositionTexture(new Vector3(512f, 512f, 0f), new Vector2(1f, 1f)),
            });
            _indexBuffer.SetData(new ushort[]
            {
                0, 1, 2,
                2, 1, 3
            });
        }

        public override void Update(GameTime gameTime)
        {
            UpdateMouseInput();
            if (Map == default) return;

            if (_chunksDirty)
            {
                var regions = Map.GetRegions(_mapBounds);
                lock (_chunkSync)
                {
                    _regions.Clear();
                    _regions.AddRange(regions.Where(r => r.IsComplete).Select(r => _regionMeshManager.CacheRegion(r)));
                }

                _chunksDirty = false;
            }
        }

        #region Drawing

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Map != default)
            {
                DrawMap(gameTime);
            }

            _gui.BeforeLayout(gameTime);

            DrawImGui();

            _gui.AfterLayout();
        }

        private void DrawImGui()
        {
            try
            {
                if (Begin("Map Viewer"))
                {
                    if (BeginTable("mapviewtable", 2))
                    {
                        // PushID("mapviewtable_Scale");
                        TableNextRow();
                        TableNextColumn();
                        Text("Scale");
                        TableNextColumn();
                        // TableSetColumnIndex(1);
                        // SetNextItemWidth(-float.MinValue);
                        SliderFloat("##value", ref _scale, 0.01f, 8f);

                        TableNextRow();

                        // PopID();
                        EndTable();
                    }

                    End();
                }

                if (Begin("Info"))
                {
                    Text("At Cursor");
                    InputInt3("Block", ref _cursorBlock[0], ImGuiInputTextFlags.ReadOnly);
                    InputInt2("Chunk", ref _cursorChunk[0], ImGuiInputTextFlags.ReadOnly);
                    InputInt2("Region", ref _cursorRegion[0], ImGuiInputTextFlags.ReadOnly);

                    SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                    if (TreeNode("Biome Info"))
                    {
                        var biome = _cursorBlockBiome;

                        var biomeId = biome?.Id ?? _cursorBlockBiomeId;
                        var biomeName = biome?.Name ?? string.Empty;
                        var biomeDefinitionName = biome?.DefinitionName ?? string.Empty;
                        var biomeMinHeight = biome?.MinHeight ?? 0;
                        var biomeMaxHeight = biome?.MaxHeight ?? 0;
                        var biomeTemperature = biome?.Temperature ?? 0;
                        var biomeDownfall = biome?.Downfall ?? 0;

                        InputInt("ID", ref biomeId, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        InputText("Name", ref biomeName, 0, ImGuiInputTextFlags.ReadOnly);
                        InputText("Definition Name", ref biomeDefinitionName, 0, ImGuiInputTextFlags.ReadOnly);
                        InputFloat("Min Height", ref biomeMinHeight, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                        InputFloat("Max Height", ref biomeMaxHeight, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                        InputFloat("Temperature", ref biomeTemperature, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                        InputFloat("Downfall", ref biomeDownfall, 0, 0, null, ImGuiInputTextFlags.ReadOnly);

                        SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                        if (TreeNode("Config"))
                        {
                            var cfg = biome?.Config;
                            BeginDisabled();

                            var cfgIsEdgeBiome = cfg?.IsEdgeBiome ?? false;
                            var cfgAllowRivers = cfg?.AllowRivers ?? false;
                            var cfgAllowScenicLakes = cfg?.AllowScenicLakes ?? false;
                            var cfgSurfaceBlendIn = cfg?.SurfaceBlendIn ?? false;
                            var cfgSurfaceBlendOut = cfg?.SurfaceBlendOut ?? false;
                            var cfgWeightMultiplier = cfg?.WeightMultiplier ?? 0;

                            Checkbox("Is Edge Biome", ref cfgIsEdgeBiome);
                            Checkbox("Allow Rivers", ref cfgAllowRivers);
                            Checkbox("Allow Scenic Lakes", ref cfgAllowScenicLakes);
                            Checkbox("Surface Blend In", ref cfgSurfaceBlendIn);
                            Checkbox("Surface Blend Out", ref cfgSurfaceBlendOut);
                            InputFloat("Weight Multiplier", ref cfgWeightMultiplier);

                            EndDisabled();

                            TreePop();
                        }


                        TreePop();
                    }

                    End();
                }

                if (Begin("Biome Colors"))
                {
                    if (BeginTable("biomeclr", 3))
                    {
                        foreach (var c in Map.BiomeProvider.Biomes)
                        {
                            TableNextRow();
                            TableNextColumn();
                            Text(c.Id.ToString());
                            TableNextColumn();
                            Text(c.Name);
                            TableNextColumn();
                            TableSetBgColor(ImGuiTableBgTarget.CellBg, GetColor(c.Color ?? System.Drawing.Color.Transparent));
                            Text(" ");
                        }
                        
                        EndTable();
                    }
                    
                    End();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Drawing exception.");
            }
        }

        private static uint GetColor(System.Drawing.Color color)
        {
//            return 0xFFFF0000;
            return (uint)(
                0xFF << 24
                | ((color.B & 0xFF) << 16)
                | ((color.G & 0xFF) << 8)
                | ((color.R & 0xFF) << 0)
            );
            // return (uint)(
            //     ((color.G & 0xFF) << 24)
            //     | ((color.B & 0xFF) << 16)
            //     | ((color.R & 0xFF) << 8)
            //     | 0xFF
            // );
        }
        
        private void DrawMap(GameTime gameTime)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.None, _rasterizerState, SamplerState.PointClamp))
            {
                cxt.ScissorRectangle = Bounds;
                var scaleMatrix =
                        Matrix.Identity
                        * Matrix.CreateTranslation(-_mapPosition.X, -_mapPosition.Y, 0f)
                        * Matrix.CreateScale(_scale, _scale, 1f)
                    ;
                foreach (var region in _regions)
                {
                    cxt.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                    cxt.GraphicsDevice.Indices = _indexBuffer;

                    _effect.World = region.World * Transform;
                    _effect.Texture = region.Texture;

                    if (_cursorRegion[0] == region.X && _cursorRegion[1] == region.Z)
                    {
                        _effect.DiffuseColor = Color.SteelBlue.ToVector3();
                    }
                    else
                    {
                        _effect.DiffuseColor = Color.White.ToVector3();
                    }

                    foreach (var p in _effect.CurrentTechnique.Passes)
                    {
                        p.Apply();
                        cxt.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                    }
                }
            }
        }

        #endregion

        private void UpdateBounds()
        {
            Bounds = new Rectangle(Point.Zero, Game.Window.ClientBounds.Size);
        }

        private void RecalculateTransform()
        {
            var p = MapPosition;
            Transform = Matrix.Identity
                        * Matrix.CreateTranslation(-p.X, -p.Y, 0)
                        * Matrix.CreateScale(_scale, _scale, 1f)
                ;

            RecalculateMapBounds();
        }

        private void RecalculateMapBounds()
        {
            var screenBounds = Bounds;
            var screenSize = new Vector2(screenBounds.Width, screenBounds.Height);

            var blockBoundsMin = Unproject(Vector2.Zero);
            var blockBoundsSize = Unproject(screenSize) - blockBoundsMin;

            var bbSize = new Point((int)Math.Ceiling(blockBoundsSize.X), (int)Math.Ceiling(blockBoundsSize.Y));
            var bbMin = new Point((int)Math.Floor(blockBoundsMin.X), (int)Math.Floor(blockBoundsMin.Y));

            MapBounds = new Rectangle(bbMin, bbSize);

            if (_effect != default)
            {
                _effect.Projection = Matrix.CreateOrthographic(screenBounds.Width, screenBounds.Height, 0.1f, 100f);

                var p = Vector3.Transform(new Vector3(0f, 0f, 0f), Transform);

                _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.Up);
                _effect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            }
        }

        private void OnMapBoundsChanged()
        {
            _chunksDirty = true;
            Log.Info($"Map bounds changed: {_mapBounds.X:000}, {_mapBounds.Y:000} => {_mapBounds.Width:0000} x {_mapBounds.Height:0000}");
        }

        private void OnRegionGenerated(object sender, Point regionPosition)
        {
            //_chunksDirty = true;
            var region = Map.GetRegion(regionPosition);
            if (region != null)
            {
                _regions.Add(_regionMeshManager.CacheRegion(region));
                Log.Info($"Region generated: {regionPosition.X:000}, {regionPosition.Y:000}");
            }
        }

        #region Mouse Events

        private void UpdateMouseInput()
        {
            if (GetIO().WantCaptureMouse)
                return;

            var newState = Mouse.GetState();

            if (_mouseState.Position != newState.Position)
            {
                OnCursorMove(newState.Position, _mouseState.Position, _mouseState.LeftButton == ButtonState.Pressed);
            }

            _mouseState = newState;
        }

        private Vector3 Unproject(Vector2 cursor)
        {
            // return GraphicsDevice.Viewport.Unproject(new Vector3(cursor.X, cursor.Y, 0f), _effect.Projection, _effect.View, Matrix.CreateTranslation(-_mapPosition.X, -_mapPosition.Y, 0f));
            return GraphicsDevice.Viewport.Unproject(new Vector3(cursor.X, cursor.Y, 0f), _effect.Projection, _effect.View, Transform);
        }

        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            var cursorBlockPos = Unproject(cursorPosition.ToVector2());
            // var cursorBlockPos = Vector3.Transform(new Vector3(cursorPosition.X, cursorPosition.Y, 0f), Transform*_effect.View);
            _cursorBlock[0] = (int)cursorBlockPos.X;
            _cursorBlock[2] = (int)cursorBlockPos.Y;
            _cursorChunk[0] = _cursorBlock[0] >> 5;
            _cursorChunk[1] = _cursorBlock[2] >> 5;
            _cursorRegion[0] = _cursorBlock[0] >> 9;
            _cursorRegion[1] = _cursorBlock[2] >> 9;

            var cursorBlockRegion = Map.GetRegion(new Point(_cursorRegion[0], _cursorRegion[1]));
            if (cursorBlockRegion?.IsComplete ?? false)
            {
                var cursorBlockChunk = cursorBlockRegion[_cursorChunk[0] & 31, _cursorChunk[1] & 31];
                var x = _cursorBlock[0] & 16;
                var z = _cursorBlock[2] & 16;
                _cursorBlock[1] = (int)cursorBlockChunk.GetHeight(x, z);
                _cursorBlockBiomeId = (int)cursorBlockChunk.GetBiome(x, z);
                _cursorBlockBiome = Map.BiomeProvider.GetBiome(_cursorBlockBiomeId);
            }

            if (isCursorDown)
            {
                var p = (cursorPosition - previousCursorPosition);
                MapPosition += new Point(-p.X, p.Y);
            }
        }

        #endregion
    }
}