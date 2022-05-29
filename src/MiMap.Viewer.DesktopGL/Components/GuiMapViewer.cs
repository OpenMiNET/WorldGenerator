// /*
//  * MiMap.Viewer.DesktopGL
//  *
//  * Copyright (c) 2020 Dan Spiteri
//  *
//  * /

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiMap.Viewer.DesktopGL.Graphics;
using NLog;
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
                new VertexPositionTexture(new Vector3(512f, 0f, 0f), new Vector2(0f, 1f)),
                new VertexPositionTexture(new Vector3(0f, 512f, 0f), new Vector2(1f, 0f)),
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
                    DragFloat("##value", ref _scale, 0.01f, 8f);
                    // PopID();
                    EndTable();
                }

                End();
            }
        }
        private void DrawMap(GameTime gameTime)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.None, _rasterizerState, SamplerState.PointClamp))
            {
                cxt.ScissorRectangle = Bounds;
                var scaleMatrix = Matrix.CreateScale(_scale, _scale, 1f);
                foreach (var region in _regions)
                {
                    cxt.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                    cxt.GraphicsDevice.Indices = _indexBuffer;

                    _effect.World = region.World * scaleMatrix;
                    _effect.Texture = region.Texture;

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
                        * Matrix.CreateTranslation(p.X, p.Y, 0)
                ;

            RecalculateMapBounds();
        }
        private void RecalculateMapBounds()
        {
            var screenBounds = Bounds;
            var screenSize = new Vector2(screenBounds.Width, screenBounds.Height);

            var blockBoundsMin = Vector2.Transform(Vector2.Zero, Transform);
            var blockBoundsSize = screenSize / _scale;

            var bbSize = new Point((int)Math.Ceiling(blockBoundsSize.X), (int)Math.Ceiling(blockBoundsSize.Y));
            var bbMin = new Point((int)Math.Floor(blockBoundsMin.X - (bbSize.X / 2f)), (int)Math.Floor(blockBoundsMin.Y - (bbSize.Y / 2f)));

            MapBounds = new Rectangle(bbMin, bbSize);

            if (_effect != default)
            {
                _effect.Projection = Matrix.CreateOrthographic(screenBounds.Width, screenBounds.Height, 0.1f, 100f);

                var p = Vector3.Transform(Vector3.Zero, Transform);

                _effect.View = Matrix.CreateLookAt(new Vector3(_mapPosition.X, _mapPosition.Y, 10), new Vector3(_mapPosition.X, _mapPosition.Y, 0), Vector3.Up);
                _effect.World = Matrix.CreateWorld(p + (Vector3.Forward * 10), Vector3.Forward, Vector3.Up);
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
            var newState = Mouse.GetState();

            if (_mouseState.Position != newState.Position)
            {
                OnCursorMove(newState.Position, _mouseState.Position, _mouseState.LeftButton == ButtonState.Pressed);
            }
            
            _mouseState = newState;
        }
        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            if (isCursorDown)
            {
                var p = (cursorPosition - previousCursorPosition);
                MapPosition += new Point(-p.X, p.Y);
            }
        }

        #endregion

    }
}