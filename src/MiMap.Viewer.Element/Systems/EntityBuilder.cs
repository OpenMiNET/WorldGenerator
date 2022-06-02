using System.Numerics;
using ElementEngine;
using ElementEngine.ECS;
using MiMap.Viewer.Element.Components;
using SharpNeat.Utility;

namespace MiMap.Viewer.Element.Systems
{
    public enum LayerType
    {
        Terrain = 0,
        Loot,
        Guard,
        Player,
    }

    public class EntityBuilder
    {
        public static readonly Vector2I SpriteFrameSize = new Vector2I(1, 1);
        
        public Registry Registry;

        private FastRandom _rng = new FastRandom();

        public EntityBuilder(Registry registry)
        {
            Registry = registry;
        }

        public Entity CreatePlayer(Vector2I position)
        {
            var player = Registry.CreateEntity();
            player.TryAddComponent(new PlayerComponent());
            player.TryAddComponent(new TransformComponent()
            {
                Position = position.ToVector2(),
            });
            player.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(0, 0, SpriteFrameSize.X, SpriteFrameSize.Y),
                Texture = AssetManager.Instance.LoadTexture2D("Player.png"),
                Layer = (int)LayerType.Player,
                Scale = new Vector2(1f),
                IsVisible = true,
            });
            player.TryAddComponent(new FourDirectionComponent()
            {
                BaseFrameTime = 0.2f,
                CurrentFrame = 0,
                Facing = FacingType.Down
            });
            player.TryAddComponent(new PhysicsComponent()
            {
                Velocity = Vector2.Zero,
                Speed = 60f,
            });
            player.TryAddComponent(new VisionComponent()
            {
                Range = 10,
            });
            player.TryAddComponent(new ColliderComponent());

            return player;

        } // CreatePlayer
    }
}