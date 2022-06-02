using System.Numerics;
using ElementEngine;
using ElementEngine.ECS;
using MiMap.Viewer.Element.Components;

namespace MiMap.Viewer.Element.Systems
{

    public static class PlayerSystems
    {
        public static void ControllerMovementSystem(Entity player)
        {
            ref var physics = ref player.GetComponent<PhysicsComponent>();

            Vector2 MovementVelocity = new Vector2();
            if (InputManager.IsKeyDown(Veldrid.Key.W))
                MovementVelocity += new Vector2(0, -1);
            if (InputManager.IsKeyDown(Veldrid.Key.A))
                MovementVelocity += new Vector2(-1, 0);
            if (InputManager.IsKeyDown(Veldrid.Key.S))
                MovementVelocity += new Vector2(0, +1);
            if (InputManager.IsKeyDown(Veldrid.Key.D))
                MovementVelocity += new Vector2(+1, 0);

            MovementVelocity *= physics.Speed;
            physics.Velocity = MovementVelocity;


        }
    }
}