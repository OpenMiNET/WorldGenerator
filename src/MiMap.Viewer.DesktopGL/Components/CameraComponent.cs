using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Components
{
    [Flags]
    public enum CameraViewMode
    {
        Top          = 0x10,
        
        Isometric45  = Isometric | 0x0,
        Isometric135 = Isometric | 0x1,
        Isometric225 = Isometric | 0x2,
        Isometric315 = Isometric | 0x4,

        Isometric    = 0x20,
    }
    
    public class CameraComponent
    {
        public const float MinScale = 0.25f;
        public const float MaxScale = 16f;
        public const float MinDepth = 1f;
        public const float MaxDepth = 1000f;
        
        private readonly Game _game;
        private Vector3 _position = new Vector3(0, 512, 0);
        private CameraViewMode _viewMode = CameraViewMode.Top;
        private float _scale = 1f;

        public Vector3 Position
        {
            get => _position;
            set
            {
                if(value == _position) return;
                
                _position = value;
                UpdateTransforms();
            }
        }

        public CameraViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (value == _viewMode) return;
                
                _viewMode = value;
                
                UpdateDirection();
            }
        }

        public Vector3 Forward { get; private set; }
        public Vector3 Up { get; private set; }

        public float Scale
        {
            get => _scale;
            set
            {
                if (value == _scale) return;
                
                _scale = MathHelper.Clamp(value, MinScale, MaxScale);
                UpdateTransforms();
            }
        }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        
        public Rectangle VisibleWorldBounds { get; private set; }
        public BoundingFrustum BoundingFrustum { get; private set; }
        
        public CameraComponent(Game game)
        {
            _game = game;
            _game.GraphicsDevice.DeviceReset += (s, o) => UpdateTransforms();
            _game.Activated += (s, o) => UpdateTransforms();
            _game.Window.ClientSizeChanged += (s, o) => UpdateTransforms();
        }

        public void MoveLocal(Vector2 offset)
        {
            Position += new Vector3(offset.X, 0, offset.Y);
        }
        public void MoveLocal(Vector3 offset)
        {
            Position += offset;
        }

        public void Initialize()
        {
            UpdateDirection();
        }

        private void UpdateDirection()
        {
            GetDirection(out var forward, out var up);
            Forward = forward;
            Up = up;
            UpdateTransforms();
        }
        
        private void GetDirection(out Vector3 forward, out Vector3 up)
        {
            switch (ViewMode)
            {
                case CameraViewMode.Top:
                    forward = Vector3.Down;
                    up = Vector3.Backward;
                    break;
                case CameraViewMode.Isometric45:
                    GetIsometricDirection(45.0f, out forward, out up);
                    break;
                case CameraViewMode.Isometric135:
                    GetIsometricDirection(135.0f, out forward, out up);
                    break;
                case CameraViewMode.Isometric225:
                    GetIsometricDirection(225.0f, out forward, out up);
                    break;
                case CameraViewMode.Isometric315:
                    GetIsometricDirection(315.0f, out forward, out up);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
        }

        private void GetIsometricDirection(float angleInDegrees, out Vector3 forward, out Vector3 up)
        {
            var a = MathHelper.ToRadians(angleInDegrees);
            //var m = Matrix.CreateRotationY(a)
            //    * Matrix.CreateRotationX(MathHelper.Pi / 6f);

            var m = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(angleInDegrees), MathHelper.Pi / 6f, 0f);
            
            forward = Vector3.TransformNormal(Vector3.Backward, m);
            up = Vector3.TransformNormal(Vector3.Up, m);
        } 
        
        private void UpdateTransforms()
        {
            var p = Position;
            var f = Forward;
            var u = Up;
            var s = Scale;
            var b = (_game.Window.ClientBounds.Size.ToVector2() / _scale).ToPoint();

            var world = Matrix.CreateTranslation(-p)
                        * Matrix.CreateScale(s, s, s)
                        * Matrix.CreateTranslation(p * s);
            
            View = Matrix.CreateLookAt(p, p + f, u);
            Projection = Matrix.CreateOrthographic(b.X, b.Y, MinDepth, MaxDepth);
            BoundingFrustum = new BoundingFrustum(View * Projection);
            VisibleWorldBounds = CalculateVisibleWorldBounds();
        }

        private Rectangle CalculateVisibleWorldBounds()
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            foreach (var corner in BoundingFrustum.GetCorners())
            {
                min.X = MathHelper.Min(corner.X, min.X);
                min.Y = MathHelper.Min(corner.Y, min.Y);
                min.Z = MathHelper.Min(corner.Z, min.Z);
                
                max.X = MathHelper.Max(corner.X, max.X);
                max.Y = MathHelper.Max(corner.Y, max.Y);
                max.Z = MathHelper.Max(corner.Z, max.Z);
            }

            return new Rectangle((int)min.X, (int)min.Z, (int)(max.X - min.X), (int)(max.Z - min.Z));
        }

        public Vector3 Project(Vector3 worldPosition)
        {
            return _game.GraphicsDevice.Viewport.Project(worldPosition, Projection, View, Matrix.Identity);
        }
        
        public Vector3 Unproject(Point screenPosition)
        {
            return Unproject(new Vector3(screenPosition.X, screenPosition.Y, 1));
        }
        
        public Vector3 Unproject(Vector3 screenPosition)
        {
            return _game.GraphicsDevice.Viewport.Unproject(screenPosition, Projection, View, Matrix.Identity);
        }
        
    }
}