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
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiMap.Viewer.DesktopGL.Core;
using MiMap.Viewer.DesktopGL.Graphics;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;

namespace MiMap.Viewer.DesktopGL.Components
{
    public partial class GuiMapViewer : DrawableGameComponent
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private float _scale = 0.5f;
        private bool _chunksDirty = false;
        private Map _map;
        private IRegionMeshManager _regionMeshManager;
        private BasicEffect _effect;
        private ImGuiRenderer _gui;
        private Rectangle _bounds;
        private MouseState _mouseState;
        private Texture2D _texture;
        private SpriteBatch _spriteBatch;

        public CameraComponent Camera { get; }
        
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

        public GuiMapViewer(MiMapViewer game, Map map) : base(game)
        {
            Map = map;
            _regionMeshManager = game.RegionMeshManager;
            _gui = game.ImGuiRenderer;
            Camera = new CameraComponent(game);
        }

        private void EnableWireframe(bool enable)
        {
            var cn = RasterizerState.CullNone;
            _rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                DepthBias = cn.DepthBias,
                FillMode = enable ? FillMode.WireFrame : FillMode.Solid,
                DepthClipEnable = cn.DepthClipEnable,
                ScissorTestEnable = cn.ScissorTestEnable,
                MultiSampleAntiAlias = cn.MultiSampleAntiAlias,
                SlopeScaleDepthBias = cn.SlopeScaleDepthBias,
            };
            
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            Camera.Initialize();
            
            EnableWireframe(false);
            
            _effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                AmbientLightColor = new Vector3(.75f,.75f,.75f),
                LightingEnabled = true,
                VertexColorEnabled = true
            };
            _effect.DirectionalLight0.Direction = Vector3.Down;
            _effect.DirectionalLight0.SpecularColor = new Vector3(1f,1f,1f);
            _effect.DirectionalLight0.Enabled = true;

            InitializeTexture();
            
            Game.GraphicsDevice.DeviceReset += (s, o) => UpdateBounds();
            Game.Activated += (s, o) => UpdateBounds();
            Game.Window.ClientSizeChanged += (s, o) => UpdateBounds();
            
            UpdateBounds();

            _map.StartBackgroundGeneration();
        }

        private double _dt;

        public override void Update(GameTime gameTime)
        {
            UpdateBounds();
            UpdateMouseInput();

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

            using (GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullNone, SamplerState.LinearClamp))
            {
                _gui.BeforeLayout(gameTime);

                DrawImGui();

                _gui.AfterLayout();
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
        }
        
        private void DrawMap_Region(GameTime gameTime)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.DepthRead, _rasterizerState, SamplerState.PointClamp))
            {
                _effect.Projection = Camera.Projection;
                _effect.View = Camera.View;
                
                foreach (var chunk in Map.Chunks.Values)
                {
                    _effect.World = chunk.World;
                    
                    foreach (var pass in _effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        chunk.Draw();
                    }
                }
            }

            // _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _spriteBatchEffect, Transform);
            //
            // _spriteBatch.DrawRectangle(new Rectangle(_cursorBlock[0], _cursorBlock[2], 1, 1), Color.White);
            // _spriteBatch.DrawRectangle(new Rectangle(_cursorChunk[0] << 4, _cursorChunk[1] << 4, 16, 16), Color.Yellow);
            // _spriteBatch.DrawRectangle(new Rectangle(_cursorRegion[0] << 9, _cursorRegion[1] << 9, 512, 512), Color.PaleGreen);
            //
            // _spriteBatch.DrawLine(1f, new Vector2(_mapBounds.X, _cursorBlock[2]), new Vector2(_mapBounds.X + _mapBounds.Width, _cursorBlock[2]), Color.DarkSlateGray * 0.65f);
            // _spriteBatch.DrawLine(1f, new Vector2(_cursorBlock[0], _mapBounds.Y), new Vector2(_cursorBlock[0], _mapBounds.Y + _mapBounds.Height), Color.DarkSlateGray * 0.65f);
            //
            // _spriteBatch.End();
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
            // var chunk = Map.GetChunk(chunkPosition);
            // if (chunk != null)
            // {
            //     // _pendingChunks.Enqueue(chunk);
            //
            //     Log.Debug($"Chunk generated: {chunkPosition.X:000}, {chunkPosition.Y:000}");
            // }
        }

        #region Mouse Events

        private void UpdateMouseInput()
        {
            if (ImGui.GetIO().WantCaptureMouse)
                return;

            var newState = Mouse.GetState();

            if (_mouseState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Released)
            {
                OnCursorClicked();
            }

            if (_mouseState.Position != newState.Position)
            {
                var bounds = Game.Window.ClientBounds;
                // var currPos = new Point(newState.X - bounds.X, bounds.Height - (newState.Y - bounds.Y));
                // var prevPos = new Point(_mouseState.X - bounds.X, bounds.Height - (_mouseState.Y - bounds.Y));
                //var currPos = new Point(newState.X - bounds.X, newState.Y - bounds.Y);
                //var prevPos = new Point(_mouseState.X - bounds.X, _mouseState.Y - bounds.Y);

                var currPos = new Point(newState.X, newState.Y);
                var prevPos = new Point(_mouseState.X, _mouseState.Y);
                var isPressed = _mouseState.LeftButton == ButtonState.Pressed;

                OnCursorMove_ImGui(currPos, prevPos, isPressed);
                OnCursorMove(currPos, prevPos, isPressed);
                _cursorPosition = currPos;
            }

            _mouseState = newState;
        }

        private void OnCursorClicked()
        {
            var worldBounds = Camera.VisibleWorldBounds;
            var minX = worldBounds.X >> 4;
            var minY = worldBounds.Y >> 4;
            var maxX = (worldBounds.X + worldBounds.Width) >> 4; 
            var maxY = (worldBounds.Y + worldBounds.Height) >> 4;
            var chunkBounds = new Rectangle(minX, minY, maxX-minX, maxY-minY);
            Map.GenerateMissingChunks(chunkBounds);
        }

        private Point Unproject(Point cursor)
        {
            //return (cursor.ToVector2() / _scale).ToPoint() + _mapPosition;
            var v = Camera.Unproject(cursor);
            return new Point((int)v.X, (int)v.Z);
        }

        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            if (isCursorDown)
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