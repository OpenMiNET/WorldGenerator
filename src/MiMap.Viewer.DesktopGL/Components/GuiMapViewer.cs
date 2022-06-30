// /*
//  * MiMap.Viewer.DesktopGL
//  *
//  * Copyright (c) 2020 Dan Spiteri
//  *
//  * /

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiMap.Viewer.DesktopGL.Core;
using MiMap.Viewer.DesktopGL.Graphics;
using MiMap.Viewer.DesktopGL.Graphics.Effects;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;

namespace MiMap.Viewer.DesktopGL.Components
{
    public partial class GuiMapViewer : DrawableGameComponent
    {
        internal static GuiMapViewer Instance { get; private set; }

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private float _scale = 0.5f;
        private bool _chunksDirty = false;
        private Map _map;

        private TextureCube _textureCube;

        // private CustomEffect _effect;
        private BasicEffect _effect;
        private ImGuiRenderer _gui;
        private Rectangle _bounds;
        private MouseState _mouseState;
        private Texture2D _texture;
        private SpriteBatch _spriteBatch;
        private bool _autoGenerate = false;

        public CameraComponent Camera { get; }

        public float ZoomSensitivity { get; set; } = 0.025f;

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

        public Rectangle MapBounds
        {
            get => Camera.VisibleWorldBounds;
        }

        public Map Map
        {
            get => _map;
            set { _map = value; }
        }

        public GuiMapViewer(MiMapViewer game, Map map) : base(game)
        {
            Instance = this;
            Map = map;
            _gui = game.ImGuiRenderer;
            Camera = new CameraComponent(game);
        }

        private void EnableWireframe(bool enable)
        {
            _wireframe = enable;
        }

        private void UpdateRasterizerState()
        {
            var a = RasterizerState.CullClockwise;
            _rasterizerState = a;
           
            /*_rasterizerState = new RasterizerState()
            {
                CullMode = _cullMode,
                DepthBias = RasterizerState.CullNone.SlopeScaleDepthBias,
                FillMode = _wireframe ? FillMode.WireFrame : FillMode.Solid,
                DepthClipEnable = false,
                ScissorTestEnable = false,
                MultiSampleAntiAlias = RasterizerState.CullNone.MultiSampleAntiAlias,
                SlopeScaleDepthBias = RasterizerState.CullNone.SlopeScaleDepthBias,
            };*/
        }

        public override void Initialize()
        {
            base.Initialize();

            Camera.Initialize();

            EnableWireframe(false);

            // _effect = new CustomEffect()
            // {
            //     Tesselation = 1,
            //     Radius = 0.1f,
            //     Test = 1,
            // };
            //_textureCube = Game.Content.Load<TextureCube>("Cubemap");
            //_effect.CubeMap = _textureCube;
            _effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                // AmbientLightColor = new Vector3(1f, 1f, 1f),
                VertexColorEnabled = false,
                LightingEnabled = true,
                DiffuseColor = Vector3.One / 2f,
                AmbientLightColor = new Vector3(.75f, .75f, .75f),
                SpecularPower = 0,
                DirectionalLight0 = { Enabled = true, Direction = new Vector3(-3, -1, -2)},
                DirectionalLight1 = { Enabled = false},
                DirectionalLight2 = { Enabled = false}
            };
            //_effect.EnableDefaultLighting();
            
            InitializeTexture();

            Game.GraphicsDevice.DeviceReset += (s, o) => UpdateBounds();
            Game.Activated += (s, o) => UpdateBounds();
            Game.Window.ClientSizeChanged += (s, o) => UpdateBounds();

            UpdateBounds();

            _map.Initialize(GraphicsDevice);
            _map.StartBackgroundGeneration();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _gizmoModel = Game.Content.Load<Model>("Gizmos");
            foreach (var modelMesh in _gizmoModel.Meshes)
            {
                foreach (var modelMeshEffect in modelMesh.Effects)
                {
                    if (modelMeshEffect is BasicEffect basicEffect)
                    {
                        basicEffect.Alpha = 1.0f;
                        basicEffect.VertexColorEnabled = true;
                        basicEffect.TextureEnabled = false;
                        basicEffect.EnableDefaultLighting();
                    }
                }
            }
        }

        private double _dt;

        public override void Update(GameTime gameTime)
        {
            UpdateBounds();
            UpdateMouseInput(gameTime);

            if (Map == default) return;

            Map.Update();
        }

        #region Drawing

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.HotPink);
            base.Draw(gameTime);

            if (Map != default)
            {
                DrawMap(gameTime);
            }

            DrawGizmo(gameTime);

            using (GraphicsContext.CreateContext(GraphicsDevice, BlendState.NonPremultiplied, DepthStencilState.None, RasterizerState.CullNone, SamplerState.LinearClamp))
            {
                _gui.BeforeLayout(gameTime);

                DrawImGui();

                _gui.AfterLayout();
            }
        }

        private Model _gizmoModel;
        private Viewport _gizmoViewport;

        private void DrawGizmo(GameTime gameTime)
        {
            if (_gizmoModel != null)
            {
                var cameraViewport = Camera.Viewport;
                _gizmoViewport = new Viewport(cameraViewport.Bounds.Right - 225, 25, 200, 200, 0f, 5f);

                using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullNone, SamplerState.LinearClamp))
                {
                    cxt.Viewport = _gizmoViewport;

                    _gizmoModel.Draw(
                        Matrix.Identity
                        * Matrix.CreateTranslation(Vector3.Zero)
                        * Matrix.CreateScale(1f)
                        ,
                        // Matrix.CreateWorld(Camera.Position, Vector3.Backward, Vector3.Up),
                        //Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up),
                        Matrix.CreateBillboard(Vector3.Zero, Vector3.Backward, Camera.Up, Camera.Forward),
                        // Camera.View,
                        Matrix.CreateOrthographicOffCenter(-1, 1, -1, 1, _gizmoViewport.MinDepth, _gizmoViewport.MaxDepth)
                        );
                }
            }
        }

        private void DrawMap(GameTime gameTime)
        {
            DrawMap_Region(gameTime);
        }

        private RasterizerState _rasterizerState;

        private void InitializeTexture()
        {
            var biomes = _map.BiomeRegistry.GetBiomes();
            var maxBiome = biomes.Max(b => b.Id);
            var raw = new Color[maxBiome + 1];

            for (int i = 0; i <= maxBiome; i++)
            {
                var biome = _map.BiomeRegistry.GetBiome(i);
                if (biome != default)
                {
                    raw[i] = biome.Color.GetValueOrDefault(System.Drawing.Color.Magenta).ToXnaColor();
                }
                else
                {
                    raw[i] = Color.DarkMagenta;
                }
            }

            var texture = new Texture2D(GraphicsDevice, raw.Length, 1, false, SurfaceFormat.Color);
            texture.SetData(raw);
            _texture = texture;
            _effect.Texture = texture;
            
            // _effect.EnableTexture = true;
            _effect.TextureEnabled = true;
        }

        private SamplerState _samplerState = new SamplerState()
        {
            Filter = TextureFilter.PointMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            FilterMode = TextureFilterMode.Default,
            AddressW = TextureAddressMode.Wrap,
            ComparisonFunction = CompareFunction.Never,
            MaxAnisotropy = 16,
            BorderColor = Color.Black
        };

        private DepthStencilState _depthStencilState = new DepthStencilState()
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.Less,
        };

        private void DrawMap_Region(GameTime gameTime)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.Opaque, DepthStencilState.Default, _rasterizerState, SamplerState.LinearWrap))
            {
                // _effect.CamPos = Camera.Position;
                // _effect.WorldRot = Matrix.Identity;
                // _effect.ViewProjection = Camera.View * Camera.Projection;
                _effect.View = Camera.View;
                _effect.Projection = Camera.Projection;

                foreach (var chunk in Map.Chunks.Values)
                {
                    _effect.World = chunk.World;

                    var min = new Vector3(chunk.ChunkCoordinates.X * 16, 0f, chunk.ChunkCoordinates.Z * 16);
                    var max = min + new Vector3(16f);

                    if (Camera.BoundingFrustum.Contains(new BoundingBox(min, max)) != ContainmentType.Disjoint)
                    {

                        foreach (var pass in _effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            chunk.Draw(GraphicsDevice);
                        }
                    }
                }
                // foreach (var chunk in Map.Chunks.Values)
                // {
                //     _blockEffect.World = chunk.World;
                //     
                //     foreach (var pass in _blockEffect.CurrentTechnique.Passes)
                //     {
                //         pass.Apply();
                //
                //         chunk.Draw(GraphicsDevice);
                //     }
                // }
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
            }
        }

        private void RecalculateMapBounds()
        {
            var screenBounds = _bounds;

            // var bbSize = (_bounds.Size.ToVector2()) / _scale;
            // var bbCenter = (_mapPosition.ToVector2() + _bounds.Center.ToVector2()) / _scale;
            // var bbMin = _mapPosition.ToVector2() / _scale;
            var bbMin = Unproject(Point.Zero);

            if (_effect != default)
            {
                _effect.World = Matrix.Identity;
                //_effect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
                //_effect.Projection = Matrix.CreateOrthographicOffCenter(0, screenBounds.Width, screenBounds.Height, 0, 0f, 1000f);
            }
        }


        #region Mouse Events

        private void UpdateMouseInput(GameTime gameTime)
        {
            if (ImGui.GetIO().WantCaptureMouse)
                return;

            var newState = Mouse.GetState();

            if (_mouseState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Released)
            {
                OnCursorClicked();
            }

            var vScrollDelta = newState.ScrollWheelValue - _mouseState.ScrollWheelValue;
            if (Math.Abs(vScrollDelta) > 0.01)
            {
                Camera.Scale += (float)(vScrollDelta * ZoomSensitivity);
            }

            var isPressed = _mouseState.LeftButton == ButtonState.Pressed;
            if (_isDragging && !isPressed) _isDragging = false;

            if (_mouseState.Position != newState.Position)
            {
                var bounds = Game.Window.ClientBounds;
                // var currPos = new Point(newState.X - bounds.X, bounds.Height - (newState.Y - bounds.Y));
                // var prevPos = new Point(_mouseState.X - bounds.X, bounds.Height - (_mouseState.Y - bounds.Y));
                //var currPos = new Point(newState.X - bounds.X, newState.Y - bounds.Y);
                //var prevPos = new Point(_mouseState.X - bounds.X, _mouseState.Y - bounds.Y);

                var currPos = new Point(newState.X, newState.Y);
                var prevPos = new Point(_mouseState.X, _mouseState.Y);

                OnCursorMove_ImGui(currPos, prevPos, isPressed);
                OnCursorMove(currPos, prevPos, isPressed);
                _cursorPosition = currPos;
            }

            _mouseState = newState;
        }


        private void Generate()
        {
            var worldBounds = Camera.VisibleWorldBounds;
            var minX = worldBounds.X >> 4;
            var minY = worldBounds.Y >> 4;
            var maxX = (worldBounds.X + worldBounds.Width) >> 4;
            var maxY = (worldBounds.Y + worldBounds.Height) >> 4;
            var chunkBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            Map.GenerateMissingChunks(chunkBounds);
        }

        private void OnCursorClicked()
        {
            if (!_autoGenerate) return;

            Generate();
        }

        private Point Unproject(Point cursor)
        {
            //return (cursor.ToVector2() / _scale).ToPoint() + _mapPosition;
            var v = Camera.Unproject(cursor);
            return new Point((int)v.X, (int)v.Z);
        }

        private bool _isDragging;

        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            if (!_isDragging && isCursorDown)
            {
                if (Bounds.Contains(cursorPosition))
                {
                    _isDragging = true;
                }
            }

            if (_isDragging)
            {
                var oldPos = Camera.Unproject(previousCursorPosition);
                var newPos = Camera.Unproject(cursorPosition);

                var delta = oldPos - newPos;

                Camera.MoveLocal(delta);
            }
        }

        #endregion
    }
}