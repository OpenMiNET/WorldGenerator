using ElementEngine;
using ElementEngine.EndlessTiles;
using ElementEngine.Tiled;
using MiMap.Viewer.Element.MiMapTiles;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators;
using Veldrid;

namespace MiMap.Viewer.Element
{
    public class Game : BaseGame
    {
        public IWorldGenerator WorldGenerator;
        public MiMapTilesWorld TilesWorld;
        public Texture2D TilesTexture;
        public MiMapTilesRenderer TilesRenderer;
        public Camera2D BackgroundCamera;

        public override void Load()
        {
            SettingsManager.LoadFromPath("Settings.xml");
            
            var windowRect = new ElementEngine.Rectangle()
            {
                X = SettingsManager.GetSetting<int>("Window", "X"),
                Y = SettingsManager.GetSetting<int>("Window", "Y"),
                Width = SettingsManager.GetSetting<int>("Window", "Width"),
                Height = SettingsManager.GetSetting<int>("Window", "Height")
            };

            var graphicsBackend = GraphicsBackend.Vulkan;

#if OPENGL
            graphicsBackend = GraphicsBackend.OpenGL;
#endif

            SetupWindow(windowRect, "Captain Shostakovich", graphicsBackend);
            SetupAssets();
            
            ClearColor = RgbaFloat.Black;
            
            InputManager.LoadGameControls();

            WorldGenerator = new OverworldGeneratorV2();
            
            TilesWorld = new MiMapTilesWorld(WorldGenerator);
            
            //todo: TilesTexture
            TilesRenderer = new MiMapTilesRenderer(TilesWorld, TilesTexture);
            
            BackgroundCamera = new Camera2D(new ElementEngine.Rectangle(0, 0, ElementGlobals.TargetResolutionWidth, ElementGlobals.TargetResolutionHeight))
            {
                Zoom = 3
            };
        }
    }
}