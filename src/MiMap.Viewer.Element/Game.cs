using ElementEngine;
using ElementEngine.EndlessTiles;
using ElementEngine.Tiled;
using ElementEngine.UI;
using MiMap.Viewer.Element.GameStates;
using MiMap.Viewer.Element.MiMapTiles;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators;
using Veldrid;

namespace MiMap.Viewer.Element
{
    public class Game : BaseGame
    {
        public GameStateView GameStateView;

        public override void Load()
        {
            SettingsManager.LoadFromPath("Settings.xml");
            
            var windowRect = new ElementEngine.Rectangle()
            {
                X = 100,
                Y = 100,
                Width = SettingsManager.GetSetting<int>("Window", "Width"),
                Height = SettingsManager.GetSetting<int>("Window", "Height")
            };

            var graphicsBackend = GraphicsBackend.Direct3D11;

#if OPENGL
            graphicsBackend = GraphicsBackend.OpenGL;
#endif

            SetupWindow(windowRect, "MiMap Viewer", graphicsBackend);
            SetupAssets("Content");
            
            
            ClearColor = RgbaFloat.Black;
            
            IMGUIManager.Setup();
            
            InputManager.LoadGameControls();

            GameStateView = new GameStateView(this);
            
            SetGameState(GameStateView);
        }

        public override void Update(GameTimer gameTimer)
        {
            IMGUIManager.Update(gameTimer);
        }

        public override void Draw(GameTimer gameTimer)
        {
            IMGUIManager.Draw();
        }
    }
}