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
using MiMap.Viewer.DesktopGL.Graphics;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Utils;

namespace MiMap.Viewer.DesktopGL.Components
{
    public partial class GuiMapViewer : DrawableGameComponent
    {
        public const float MinScale = 0.2f;
        public const float MaxScale = 8f;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private Rectangle _visibleChunkBounds;
        private Rectangle _mapBounds;
        private float _scale = 0.5f;
        private ConcurrentQueue<MapRegion> _pendingRegions;
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
        private Rectangle _bounds;
        private MouseState _mouseState;

        private SpriteBatch _spriteBatch;
        
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
            _pendingRegions = new ConcurrentQueue<MapRegion>();
            _pendingChunks = new ConcurrentQueue<MapChunk>();
            _regionMeshManager = game.RegionMeshManager;
            _gui = game.ImGuiRenderer;
        }

        public override void Initialize()
        {
            base.Initialize();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteBatchEffect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true
            };
            _effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = false
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
                1, 3, 2
            });
        }

        public override void Update(GameTime gameTime)
        {
            UpdateBounds();
            UpdateMouseInput();

            if (Map == default) return;

            if (_chunksDirty)
            {
                Map.EnqueueChunks(_mapBounds); // generate the visible chunks

                _chunksDirty = false;
            }

            if (!_pendingChunks.IsEmpty || !_pendingRegions.IsEmpty)
            {
                var sw = Stopwatch.StartNew();
                MapRegion r;
                while (_pendingRegions.TryDequeue(out r) && sw.ElapsedMilliseconds < 36f)
                {
                    //_regions.Add(_regionMeshManager.CacheRegion(r));
                }
                
                MapChunk c;
                while (_pendingChunks.TryDequeue(out c) && sw.ElapsedMilliseconds < 50f)
                {
                    _regionMeshManager.CacheRegionChunk(c);
//                    _chunks.Add(_regionMeshManager.CacheChunk(c));
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

        private void DrawMap(GameTime gameTime)
        {
                DrawMap_Region(gameTime);
        }

        private static readonly Point RegionSize = new Point(512, 512);
        private static readonly Rectangle ChunkSize = new Rectangle(0, 0, 16, 16);
        private int[] _mapPositions = new int[2];

        // private void DrawMap_Chunk(GameTime gameTime)
        // {
        //     _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, _spriteBatchEffect, Transform);
        //
        //     foreach (var chunk in _chunks)
        //     {
        //         _spriteBatch.Draw(chunk.Texture, chunk.Position.ToVector2(), Color.White);
        //         // _spriteBatch.Draw(chunk.Texture, chunk.Position.ToVector2(), ChunkSize, Color.White, 0, Vector2.Zero, _scale,SpriteEffects.FlipVertically, 0);
        //     }
        //
        //     _spriteBatch.DrawRectangle(new Rectangle(_cursorBlock[0], _cursorBlock[2], 1, 1), Color.White);
        //     _spriteBatch.DrawRectangle(new Rectangle(_cursorChunk[0] << 4, _cursorChunk[1] << 4, 16, 16), Color.Yellow);
        //     _spriteBatch.DrawRectangle(new Rectangle(_cursorRegion[0] << 9, _cursorRegion[1] << 9, 512, 512), Color.PaleGreen);
        //
        //     _spriteBatch.End();
        // }

        private void DrawMap_Region(GameTime gameTime)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullNone, SamplerState.LinearClamp))
            {
                foreach (var region in _regionMeshManager.Regions)
                {
                    cxt.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                    cxt.GraphicsDevice.Indices = _indexBuffer;

                    _effect.World = region.World * Transform;
                    _effect.Texture = region.Texture;
                    
                    foreach (var p in _effect.CurrentTechnique.Passes)
                    {
                        p.Apply();
                        cxt.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                    }
                }
            }
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _spriteBatchEffect, Transform);
            
            _spriteBatch.DrawRectangle(new Rectangle(_cursorBlock[0], _cursorBlock[2], 1, 1), Color.White);
            _spriteBatch.DrawRectangle(new Rectangle(_cursorChunk[0] << 4, _cursorChunk[1] << 4, 16, 16), Color.Yellow);
            _spriteBatch.DrawRectangle(new Rectangle(_cursorRegion[0] << 9, _cursorRegion[1] << 9, 512, 512), Color.PaleGreen);

            _spriteBatch.DrawLine(1f, new Vector2(_mapBounds.X, _cursorBlock[2]), new Vector2(_mapBounds.X + _mapBounds.Width, _cursorBlock[2]), Color.DarkSlateGray * 0.65f);
            _spriteBatch.DrawLine(1f, new Vector2(_cursorBlock[0], _mapBounds.Y), new Vector2(_cursorBlock[0], _mapBounds.Y + _mapBounds.Height), Color.DarkSlateGray * 0.65f);

            _spriteBatch.End();
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
            if (_scale < MinScale) _scale = MinScale;
            if (_scale > MaxScale) _scale = MaxScale;
            
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
                _effect.World = Transform;
                _effect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
                _effect.Projection = Matrix.CreateOrthographicOffCenter(0, screenBounds.Width, screenBounds.Height, 0,  0f, 1f);
            }
            
            if (_spriteBatchEffect != default)
            {
                _spriteBatchEffect.World = Transform;
                _spriteBatchEffect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
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
            if (ImGui.GetIO().WantCaptureMouse)
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
                var isPressed = _mouseState.LeftButton == ButtonState.Pressed;
                
                OnCursorMove_ImGui(currPos, prevPos, isPressed);
                OnCursorMove(currPos, prevPos, isPressed);
                _cursorPosition = currPos;
            }

            _mouseState = newState;
        }

        private Point Unproject(Point cursor)
        {
            return (cursor.ToVector2() / _scale).ToPoint() + _mapPosition;
        }

        private void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {

            if (isCursorDown)
            {
                var p = ((cursorPosition - previousCursorPosition).ToVector2() / _scale).ToPoint();
                MapPosition += new Point(-p.X, -p.Y);
            }
        }

        #endregion
    }
}