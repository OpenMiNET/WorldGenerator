// /*
//  * MiMap.Viewer.DesktopGL
//  *
//  * Copyright (c) 2020 Dan Spiteri
//  *
//  * /

using System;
using System.Collections.Concurrent;
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
    public enum MapViewMode
    {
        Region,
        Chunk
    }

    public class GuiMapViewer : DrawableGameComponent
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private Point _position = Point.Zero;
        private Rectangle _visibleChunkBounds;
        private Rectangle _mapBounds;
        private float _scale = 1f;
        private readonly object _chunkSync = new object();
        private List<CachedRegionMesh> _regions;
        private List<CachedChunkMesh> _chunks;
        private ConcurrentQueue<MapChunk> _pendingChunks;
        private bool _chunksDirty = false;
        private Map _map;
        private Point _mapPosition;
        private Matrix _transform;
        private int _drawOrder = 1;
        private bool _visible = true;
        private IRegionMeshManager _regionMeshManager;
        private BasicEffect _effect;
        private BasicEffect _spriteBatchEffect;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private ImGuiRenderer _gui;
        private readonly RasterizerState _rasterizerState;
        private Rectangle _bounds;
        private MouseState _mouseState;
        private Point _cursorPosition;
        private int[] _cursorBlock = new int[3];
        private BiomeBase _cursorBlockBiome;
        private int[] _cursorChunk = new int[2];
        private int[] _cursorRegion = new int[2];
        private int _cursorBlockBiomeId;

        private SpriteBatch _spriteBatch;

        private MapViewMode _mode = MapViewMode.Chunk;

        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                RecalculateMapBounds();
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
                    _map.ChunkGenerated -= OnChunkGenerated;
                }

                _map = value;

                if (_map != default)
                {
                    _map.RegionGenerated += OnRegionGenerated;
                    _map.ChunkGenerated += OnChunkGenerated;
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
            _chunks = new List<CachedChunkMesh>();
            _pendingChunks = new ConcurrentQueue<MapChunk>();
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

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = false,
                VertexColorEnabled = false,
                TextureEnabled = true,
                FogEnabled = false
            };
            _spriteBatchEffect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true
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
            UpdateBounds();
            UpdateMouseInput();

            if (Map == default) return;

            if (_chunksDirty)
            {
                if (_mode == MapViewMode.Region)
                {
                    var regions = Map.GetRegions(_mapBounds);
                    lock (_chunkSync)
                    {
                        _regions.Clear();
                        _regions.AddRange(regions.Where(r => r.IsComplete).Select(r => _regionMeshManager.CacheRegion(r)));
                    }
                }
                else if (_mode == MapViewMode.Chunk)
                {
                    Map.EnqueueChunks(_mapBounds); // generate the visible chunks
                }

                _chunksDirty = false;
            }

            if (_mode == MapViewMode.Chunk)
            {
                if (!_pendingChunks.IsEmpty)
                {
                    MapChunk c;
                    while (_pendingChunks.TryDequeue(out c))
                    {
                        _chunks.Add(_regionMeshManager.CacheChunk(c));
                    }
                }
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
                    if (BeginTable("mapviewtable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                    {
                        _mapPositions[0] = _mapPosition.X;
                        _mapPositions[1] = _mapPosition.Y;
                        TableNextRow();
                        TableNextColumn();
                        Text("Map Position");
                        TableNextColumn();
                        InputInt2("##value", ref _mapPositions[0]);
                        if (IsItemEdited())
                        {
                            MapPosition = new Point(_mapPositions[0], _mapPositions[1]);
                        }

                        var bounds = MapBounds;
                        var boundsValues = new int[] { bounds.X, bounds.Y, bounds.Width, bounds.Height };

                        TableNextRow();
                        TableNextColumn();
                        Text("Map Bounds");
                        TableNextColumn();
                        InputInt4("##value", ref boundsValues[0], ImGuiInputTextFlags.ReadOnly);

                        TableNextRow();
                        TableNextColumn();
                        Text("Scale");
                        TableNextColumn();
                        SliderFloat("##value", ref _scale, 0.01f, 8f);
                        if (IsItemEdited())
                        {
                            RecalculateTransform();
                        }

                        TableNextRow();

                        // PopID();
                        EndTable();
                    }

                    Spacing();

                    if (BeginTable("mapviewtable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                    {
                        var cursorPositionValues = new int[]
                        {
                            _cursorPosition.X,
                            _cursorPosition.Y
                        };
                        TableNextRow();
                        TableNextColumn();
                        Text("Cursor Position");
                        TableNextColumn();
                        InputInt2("##value", ref cursorPositionValues[0], ImGuiInputTextFlags.ReadOnly);


                        EndTable();
                    }

                    SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                    if (TreeNodeEx("Graphics"))
                    {
                        if (BeginTable("graphics", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit))
                        {
                            TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                            TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                            var v = GraphicsDevice.Viewport;
                            var viewportValues = new int[]
                            {
                                v.X,
                                v.Y,
                                v.Width,
                                v.Height
                            };

                            TableNextRow();
                            TableNextColumn();
                            Text("Viewport");
                            TableNextColumn();
                            InputInt4("##value", ref viewportValues[0], ImGuiInputTextFlags.ReadOnly);

                            EndTable();
                        }

                        TreePop();
                    }

                    SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                    if (TreeNodeEx("Window"))
                    {
                        if (BeginTable("window", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit))
                        {
                            TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                            TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                            var p = Game.Window.Position;
                            var windowPositionValues = new int[]
                            {
                                p.X,
                                p.Y
                            };
                            TableNextRow();
                            TableNextColumn();
                            Text("Position");
                            TableNextColumn();
                            InputInt2("##value", ref windowPositionValues[0], ImGuiInputTextFlags.ReadOnly);
                            
                            
                            var c = Game.Window.ClientBounds;
                            var windowClientBoundsValues = new int[]
                            {
                                c.X,
                                c.Y,
                                c.Width,
                                c.Height
                            };
                            TableNextRow();
                            TableNextColumn();
                            Text("Client Bounds");
                            TableNextColumn();
                            InputInt4("##value", ref windowClientBoundsValues[0], ImGuiInputTextFlags.ReadOnly);


                            EndTable();
                        }

                        TreePop();
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
                            var cfgWeight = cfg?.Weight ?? 0;

                            Checkbox("Is Edge Biome", ref cfgIsEdgeBiome);
                            Checkbox("Allow Rivers", ref cfgAllowRivers);
                            Checkbox("Allow Scenic Lakes", ref cfgAllowScenicLakes);
                            Checkbox("Surface Blend In", ref cfgSurfaceBlendIn);
                            Checkbox("Surface Blend Out", ref cfgSurfaceBlendOut);
                            InputInt("Weight", ref cfgWeight);

                            EndDisabled();

                            TreePop();
                        }


                        TreePop();
                    }

                    End();
                }

                if (Begin("Biome Colors"))
                {
                    if (BeginTable("biomeclr", 3, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                    {
                        foreach (var c in Map.BiomeRegistry.Biomes)
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
            if (_mode == MapViewMode.Region)
                DrawMap_Region(gameTime);
            else if (_mode == MapViewMode.Chunk)
                DrawMap_Chunk(gameTime);
        }

        private static readonly Point RegionSize = new Point(512, 512);
        private static readonly Rectangle ChunkSize = new Rectangle(0, 0, 16, 16);
        private int[] _mapPositions = new int[2];

        private void DrawMap_Chunk(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, _spriteBatchEffect, Transform);

            foreach (var chunk in _chunks)
            {
                _spriteBatch.Draw(chunk.Texture, chunk.Position.ToVector2(), Color.White);
                // _spriteBatch.Draw(chunk.Texture, chunk.Position.ToVector2(), ChunkSize, Color.White, 0, Vector2.Zero, _scale,SpriteEffects.FlipVertically, 0);
            }

            _spriteBatch.DrawRectangle(new Rectangle(_cursorBlock[0], _cursorBlock[2], 1, 1), Color.White);
            _spriteBatch.DrawRectangle(new Rectangle(_cursorChunk[0] << 4, _cursorChunk[1] << 4, 16, 16), Color.Yellow);
            _spriteBatch.DrawRectangle(new Rectangle(_cursorRegion[0] << 9, _cursorRegion[1] << 9, 512, 512), Color.PaleGreen);

            _spriteBatch.End();
        }

        private void DrawMap_Region(GameTime gameTime)
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

        private Rectangle _previousClientBounds = Rectangle.Empty;

        private void UpdateBounds()
        {
            var clientBounds = Game.Window.ClientBounds;
            if (clientBounds != _previousClientBounds)
            {
                Bounds = new Rectangle(Point.Zero, Game.Window.ClientBounds.Size);
                RecalculateMapBounds();
            }
        }

        private void RecalculateTransform()
        {
            var p = MapPosition;
            Transform = Matrix.Identity
                        // * Matrix.CreateRotationX(MathHelper.Pi)
                        * Matrix.CreateTranslation(-p.X, -p.Y, 0)
                        * Matrix.CreateScale(_scale, _scale, 1f)
                ;

            RecalculateMapBounds();
        }

        private void RecalculateMapBounds()
        {
            var screenBounds = _bounds;
            
            // var bbSize = (_bounds.Size.ToVector2()) / _scale;
            // var bbCenter = (_mapPosition.ToVector2() + _bounds.Center.ToVector2()) / _scale;
            // var bbMin = _mapPosition.ToVector2() / _scale;
            var bbMin = Unproject(Point.Zero);
            var bbSize = Unproject(_bounds.Size) - bbMin;
            
            MapBounds = new Rectangle(bbMin, bbSize);
            
            if (_effect != default)
            {
                _effect.Projection = Matrix.CreateOrthographic(screenBounds.Width, screenBounds.Height, 0.1f, 100f);
                
                _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.Up);
                _effect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            }

            if (_spriteBatchEffect != default)
            {
                _spriteBatchEffect.World = Transform;
                _spriteBatchEffect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
                // _spriteBatchEffect.Projection = Matrix.CreateOrthographicOffCenter(0, screenSize.X / _scale, 0, screenSize.Y / _scale, 0.1f, 100f);
                _spriteBatchEffect.Projection = Matrix.CreateOrthographicOffCenter(0, screenBounds.Width, screenBounds.Height, 0,  0f, 1f);
            }
        }

        private void OnMapBoundsChanged()
        {
            //_chunksDirty = true;
            var b = _mapBounds;
            var visibleChunkBounds = new Rectangle(b.X >> 4, b.Y >> 4, b.Width >> 4, b.Height >> 4);
            if (visibleChunkBounds != _visibleChunkBounds)
            {
                _chunksDirty = true;
            }

            _visibleChunkBounds = visibleChunkBounds;

            Log.Info($"Map bounds changed: {_mapBounds.X:000}, {_mapBounds.Y:000} => {_mapBounds.Width:0000} x {_mapBounds.Height:0000}");
        }

        private void OnRegionGenerated(object sender, Point regionPosition)
        {
            //_chunksDirty = true;
            // var region = Map.GetRegion(regionPosition);
            // if (region != null)
            // {
            //     _regions.Add(_regionMeshManager.CacheRegion(region));
            //
            //     Log.Info($"Region generated: {regionPosition.X:000}, {regionPosition.Y:000}");
            // }
        }

        private void OnChunkGenerated(object sender, Point chunkPosition)
        {
            // _chunksDirty = true;
            var chunk = Map.GetChunk(chunkPosition);
            if (chunk != null)
            {
                _pendingChunks.Enqueue(chunk);

                Log.Debug($"Chunk generated: {chunkPosition.X:000}, {chunkPosition.Y:000}");
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
                var bounds = Game.Window.ClientBounds;
                // var currPos = new Point(newState.X - bounds.X, bounds.Height - (newState.Y - bounds.Y));
                // var prevPos = new Point(_mouseState.X - bounds.X, bounds.Height - (_mouseState.Y - bounds.Y));
                //var currPos = new Point(newState.X - bounds.X, newState.Y - bounds.Y);
                //var prevPos = new Point(_mouseState.X - bounds.X, _mouseState.Y - bounds.Y);

                var currPos = new Point(newState.X, newState.Y);
                var prevPos = new Point(_mouseState.X, _mouseState.Y);
                OnCursorMove(currPos, prevPos, _mouseState.LeftButton == ButtonState.Pressed);
                _cursorPosition = currPos;
            }

            _mouseState = newState;
        }

        private Point Unproject(Point cursor)
        {
            return (cursor.ToVector2() / _scale).ToPoint() + _mapPosition;

//            return Vector2.Transform(cursor, InverseTransform);
            // return GraphicsDevice.Viewport.Unproject(new Vector3(cursor.X, cursor.Y, 0f), _effect.Projection, _effect.View, Matrix.CreateTranslation(-_mapPosition.X, -_mapPosition.Y, 0f));
            //return GraphicsDevice.Viewport.Unproject(new Vector3(cursor.X, cursor.Y, 0f), _effect.Projection, _effect.View, Transform);
        }

        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            var cursorBlockPos = Unproject(cursorPosition);
            // var cursorBlockPos = Vector3.Transform(new Vector3(cursorPosition.X, cursorPosition.Y, 0f), Transform*_effect.View);
            _cursorBlock[0] = cursorBlockPos.X;
            _cursorBlock[2] = cursorBlockPos.Y;
            _cursorChunk[0] = _cursorBlock[0] >> 4;
            _cursorChunk[1] = _cursorBlock[2] >> 4;
            _cursorRegion[0] = _cursorBlock[0] >> 9;
            _cursorRegion[1] = _cursorBlock[2] >> 9;

            var cursorBlockRegion = Map.GetRegion(new Point(_cursorRegion[0], _cursorRegion[1]));
            if (cursorBlockRegion?.IsComplete ?? false)
            {
                var cursorBlockChunk = cursorBlockRegion[_cursorChunk[0] % 32, _cursorChunk[1] % 32];
                var x = _cursorBlock[0] % 16;
                var z = _cursorBlock[2] % 16;
                _cursorBlock[1] = (int)cursorBlockChunk.GetHeight(x, z);
                _cursorBlockBiomeId = (int)cursorBlockChunk.GetBiome(x, z);
                _cursorBlockBiome = Map.BiomeRegistry.GetBiome(_cursorBlockBiomeId);
            }

            if (isCursorDown)
            {
                var p = (cursorPosition - previousCursorPosition);
                MapPosition += new Point(-p.X, -p.Y);
            }
        }

        #endregion
    }
}