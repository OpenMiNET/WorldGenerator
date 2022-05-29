using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiMap.Viewer.DesktopGL.Components;
using MiMap.Viewer.DesktopGL.Graphics;
using MiMap.Viewer.DesktopGL.Models;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators;

namespace MiMap.Viewer.DesktopGL
{
    public class MiMapViewer : Game
    {
        public static MiMapViewer Instance { get; private set; }

        private readonly GraphicsDeviceManager _graphics;
        public ImGuiRenderer ImGuiRenderer { get; private set; }
        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        public IWorldGenerator WorldGenerator { get; private set; }
        public Map Map { get; private set; }
        public IRegionMeshManager RegionMeshManager { get; private set; }
        private GuiMapViewer _mapViewer;

        public MiMapViewer() : base()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 720,
                PreferredBackBufferWidth = 1280,
                PreferMultiSampling = true,
                GraphicsProfile = GraphicsProfile.HiDef
            };
            _graphics.PreparingDeviceSettings += (sender, args) =>
            {
                //_graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
                _graphics.PreferMultiSampling = true;
                _graphics.PreferredBackBufferHeight = 720;
                _graphics.PreferredBackBufferWidth = 1280;
                _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            };
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            
            WorldGenerator = new OverworldGeneratorV2();
            Map = new Map(WorldGenerator);
        }

        protected override void Initialize()
        {
            // _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            // _graphics.SynchronizeWithVerticalRetrace = false;
//            _graphics.PreferMultiSampling = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;

            UpdateViewport();
            Window.ClientSizeChanged += (s, o) => UpdateViewport();

            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();
            
            RegionMeshManager = new RegionMeshManager(GraphicsDevice);
            _mapViewer = new GuiMapViewer(this, Map);
            _mapViewer.Initialize();
            Components.Add(_mapViewer);
            
            
            // Initialize biome colors
            InitializeBiomeColors();

        }

        private void InitializeBiomeColors()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "biomeColors.json"));
            var map = BiomeColors.FromJson(json);

            foreach (var mapping in map.ColorMap)
            {
                var biome = Map.BiomeProvider.GetBiome(mapping.BiomeId);
                if (biome != default)
                {
                    biome.Color = System.Drawing.Color.FromArgb(mapping.JColor.R, mapping.JColor.G, mapping.JColor.B);
                }
            }
        }

        private void UpdateViewport(bool apply = true)
        {
            var bounds = Window.ClientBounds;

            if (
                _graphics.PreferredBackBufferWidth != bounds.Width
                ||
                _graphics.PreferredBackBufferHeight != bounds.Height)
            {
                if (_graphics.IsFullScreen)
                {
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }
                else
                {
                    _graphics.PreferredBackBufferWidth = bounds.Width;
                    _graphics.PreferredBackBufferHeight = bounds.Height;
                }

                if (apply)
                    _graphics.ApplyChanges();
            }

            ImGuiRenderer?.RebuildFontAtlas();
            //_graphics.GraphicsDevice.Viewport = new Viewport(bounds);
        }

        protected override void LoadContent()
        {
            // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                var red = (pixel % 300) / 2;
                return new Color(red, 1, 1);
            });
            
            // Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
            _imGuiTexture = ImGuiRenderer.BindTexture(_xnaTexture);
            
            base.LoadContent();

            UpdateViewport();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _mapViewer?.Dispose();
            RegionMeshManager?.Dispose();
            Map?.Dispose();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.HotPink);
            base.Draw(gameTime);
        }

        private static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            //initialize a texture
            var texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for(var pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint( pixel );
            }

            //set the color
            texture.SetData( data );

            return texture;
        }
    }
}