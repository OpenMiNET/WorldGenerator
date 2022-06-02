using System.Numerics;
using ElementEngine;
using ElementEngine.ECS;
using MiMap.Viewer.Element.Components;

namespace MiMap.Viewer.Element.Systems
{

    public static class GeneralSystems
    {
        private struct DrawItem
        {
            public Vector2 Position;
            public Vector2 Origin;
            public Vector2 Scale;
            public float Rotation;
            public Rectangle SourceRect;
            public Texture2D Texture;
            public int Layer;
        }

        private static List<DrawItem> _drawList = new List<DrawItem>();

        public static void DrawableSystem(Group group, SpriteBatch2D spriteBatch, Camera2D camera)
        {
            var cameraView = camera.ScaledView;

            _drawList.Clear();

            foreach (var entity in group.Entities)
            {
                ref var drawable = ref entity.GetComponent<DrawableComponent>();
                ref var transform = ref entity.GetComponent<TransformComponent>();

                if (!drawable.IsVisible)
                    continue;

                var Pos = transform.TransformedPosition - drawable.Origin;
                var Size = drawable.AtlasRect.Size;
                var entityRect = new Rectangle(Pos.ToVector2I(), Size);

                if (entityRect.Intersects(cameraView))
                {
                    _drawList.Add(new DrawItem()
                    {
                        Position = transform.TransformedPosition,
                        Origin = drawable.Origin,
                        Scale = drawable.Scale,
                        Rotation = transform.Rotation,
                        SourceRect = drawable.AtlasRect,
                        Texture = drawable.Texture,
                        Layer = drawable.Layer,
                    });
                }
            }

            if (_drawList.Count > 0)
            {
                // sort by layer then Y position
                _drawList.Sort((x, y) =>
                {
                    var val = x.Layer.CompareTo(y.Layer);

                    if (val == 0)
                        val = x.Position.Y.CompareTo(y.Position.Y);

                    return val;
                });

                foreach (var item in _drawList)
                    spriteBatch.DrawTexture2D(item.Texture, item.Position, item.SourceRect, item.Scale, item.Origin, item.Rotation);
            }

        } // DrawableSystem

        public static Vector2I GetEntityTile(Entity player)
        {
            ref var transform = ref player.GetComponent<TransformComponent>();
            var playerTile = transform.TransformedPosition.ToVector2I();

            return new Vector2I(playerTile.X << 4, playerTile.Y << 4);
        }


        public static void FourDirectionSystem(Group group, GameTimer gameTimer)
        {
            foreach (var entity in group.Entities)
            {
                ref var physics = ref entity.GetComponent<PhysicsComponent>();
                ref var drawable = ref entity.GetComponent<DrawableComponent>();
                ref var four = ref entity.GetComponent<FourDirectionComponent>();

                if (physics.Velocity != Vector2.Zero)
                {
                    // Update facing
                    if (physics.Velocity.Y < 0) four.Facing = FacingType.Up;
                    if (physics.Velocity.Y > 0) four.Facing = FacingType.Down;
                    if (physics.Velocity.X < 0) four.Facing = FacingType.Left;
                    if (physics.Velocity.X > 0) four.Facing = FacingType.Right;

                    switch (four.Facing)
                    {
                        case FacingType.Up: drawable.AtlasRect.Y = 0; break;
                        case FacingType.Down: drawable.AtlasRect.Y = 32; break;
                        case FacingType.Left: drawable.AtlasRect.Y = 64; break;
                        case FacingType.Right: drawable.AtlasRect.Y = 96; break;
                    }

                    four.CurrentFrameTime += gameTimer.DeltaS;

                    if (four.CurrentFrameTime >= four.BaseFrameTime)
                    {
                        four.CurrentFrameTime = 0f;
                        four.CurrentFrame += 1;

                        if (four.CurrentFrame > 3)
                            four.CurrentFrame -= 4;
                    }
                }
                else
                {
                    four.CurrentFrame = 0;
                }

                drawable.AtlasRect.X = four.CurrentFrame * 16;
            }
        }

    } // GeneralSystems
}