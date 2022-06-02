using System.Numerics;
using ElementEngine.ECS;

namespace MiMap.Viewer.Element.Components
{

    public struct TileComponent
    {
        public Entity Parent;
        public Vector2 Position;

        public Vector2 TransformedPosition
        {
            get
            {
                if (!Parent.IsAlive)
                    return Position;
                else
                {
                    ref var parentTransform = ref Parent.GetComponent<TransformComponent>();
                    var transformMatrix =
                        Matrix3x2.CreateRotation(parentTransform.Rotation) *
                        Matrix3x2.CreateTranslation(parentTransform.TransformedPosition);

                    return Vector2.Transform(Position, transformMatrix);
                }
            }
        }
    } // TransformComponent
}