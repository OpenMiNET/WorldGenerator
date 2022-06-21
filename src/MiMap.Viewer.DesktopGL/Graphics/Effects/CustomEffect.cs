using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Graphics.Effects
{
    public class CustomEffect : Effect
    {
        #region Dirty Flags

        [Flags]
        internal enum EffectDirtyFlags
        {
            CamPos = 0x0001,
            World = 0x0002,
            WorldRot = 0x0004,
            ViewProjection = 0x0008,
            Tesselation = 0x0010,
            Radius = 0x0020,
            Test = 0x0040,
            CubeMap = 0x0080,
            EnableTexture = 0x0100,
            Texture = 0x0200,

            All = 0x03FF
        }

        #endregion

        #region Effect Parameters

        private readonly EffectParameter _parameterCamPos;
        private readonly EffectParameter _parameterWorld;
        private readonly EffectParameter _parameterWorldRot;
        private readonly EffectParameter _parameterViewProjection;
        private readonly EffectParameter _parameterTesselation;
        private readonly EffectParameter _parameterRadius;
        private readonly EffectParameter _parameterTest;
        private readonly EffectParameter _parameterCubeMap;
        private readonly EffectParameter _parameterEnableTexture;
        private readonly EffectParameter _parameterTexture;

        #endregion

        #region Fields

        private EffectDirtyFlags _dirtyFlags = EffectDirtyFlags.All;

        private Vector3 _camPos;
        private Matrix _world;
        private Matrix _worldRot;
        private Matrix _viewProjection;
        private float _tesselation;
        private float _radius;
        private float _test;
        private TextureCube _cubeMap;
        private SamplerState _cubeSampler;
        private bool _enableTexture;
        private Texture2D _texture;
        private SamplerState _textureSampler;

        #endregion

        #region Public Properties

        public Vector3 CamPos
        {
            get { return _camPos; }
            set
            {
                _camPos = value;
                _dirtyFlags |= EffectDirtyFlags.CamPos;
            }
        }

        public Matrix World
        {
            get { return _world; }
            set
            {
                _world = value;
                _dirtyFlags |= EffectDirtyFlags.World;
            }
        }

        public Matrix WorldRot
        {
            get { return _worldRot; }
            set
            {
                _worldRot = value;
                _dirtyFlags |= EffectDirtyFlags.WorldRot;
            }
        }

        public Matrix ViewProjection
        {
            get { return _viewProjection; }
            set
            {
                _viewProjection = value;
                _dirtyFlags |= EffectDirtyFlags.ViewProjection;
            }
        }

        public float Tesselation
        {
            get { return _tesselation; }
            set
            {
                _tesselation = value;
                _dirtyFlags |= EffectDirtyFlags.Tesselation;
            }
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _dirtyFlags |= EffectDirtyFlags.Radius;
            }
        }

        public float Test
        {
            get { return _test; }
            set
            {
                _test = value;
                _dirtyFlags |= EffectDirtyFlags.Test;
            }
        }

        public TextureCube CubeMap
        {
            get { return _cubeMap; }
            set
            {
                _cubeMap = value;
                _dirtyFlags |= EffectDirtyFlags.CubeMap;
            }
        }

        public bool EnableTexture
        {
            get { return _enableTexture; }
            set
            {
                _enableTexture = value;
                _dirtyFlags |= EffectDirtyFlags.EnableTexture;
            }
        }

        public Texture2D Texture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                _dirtyFlags |= EffectDirtyFlags.Texture;
            }
        }

        #endregion

        #region Methods

        protected override void OnApply()
        {
            if ((_dirtyFlags & EffectDirtyFlags.CamPos) != 0)
            {
                _parameterCamPos.SetValue(_camPos);
                _dirtyFlags &= ~EffectDirtyFlags.CamPos;
            }

            if ((_dirtyFlags & EffectDirtyFlags.World) != 0)
            {
                _parameterWorld.SetValue(_world);
                _dirtyFlags &= ~EffectDirtyFlags.World;
            }

            if ((_dirtyFlags & EffectDirtyFlags.WorldRot) != 0)
            {
                _parameterWorldRot.SetValue(_worldRot);
                _dirtyFlags &= ~EffectDirtyFlags.WorldRot;
            }

            if ((_dirtyFlags & EffectDirtyFlags.ViewProjection) != 0)
            {
                _parameterViewProjection.SetValue(_viewProjection);
                _dirtyFlags &= ~EffectDirtyFlags.ViewProjection;
            }

            if ((_dirtyFlags & EffectDirtyFlags.Tesselation) != 0)
            {
                _parameterTesselation.SetValue(_tesselation);
                _dirtyFlags &= ~EffectDirtyFlags.Tesselation;
            }

            if ((_dirtyFlags & EffectDirtyFlags.Radius) != 0)
            {
                _parameterRadius.SetValue(_radius);
                _dirtyFlags &= ~EffectDirtyFlags.Radius;
            }

            if ((_dirtyFlags & EffectDirtyFlags.Test) != 0)
            {
                _parameterTest.SetValue(_test);
                _dirtyFlags &= ~EffectDirtyFlags.Test;
            }
            //
            // if ((_dirtyFlags & EffectDirtyFlags.CubeMap) != 0)
            // {
            //     _parameterCubeMap.SetValue(_cubeMap);
            //     _dirtyFlags &= ~EffectDirtyFlags.CubeMap;
            // }

            if ((_dirtyFlags & EffectDirtyFlags.EnableTexture) != 0)
            {
                _parameterEnableTexture.SetValue(_enableTexture);
                _dirtyFlags &= ~EffectDirtyFlags.EnableTexture;
            }

            if ((_dirtyFlags & EffectDirtyFlags.Texture) != 0)
            {
                _parameterTexture.SetValue(_texture);
                _dirtyFlags &= ~EffectDirtyFlags.Texture;
            }
        }

        #endregion

        #region Constructors

        public CustomEffect() : base(MiMapViewer.Instance.Content.Load<Effect>("Effects/CustomEffect"))
        {
            _parameterCamPos = Parameters["CamPos"];
            _parameterWorld = Parameters["World"];
            _parameterWorldRot = Parameters["WorldRot"];
            _parameterViewProjection = Parameters["ViewProjection"];
            _parameterTesselation = Parameters["Tesselation"];
            _parameterRadius = Parameters["Radius"];
            _parameterTest = Parameters["Test"];
            _parameterCubeMap = Parameters["CubeMap"];
            _parameterEnableTexture = Parameters["EnableTexture"];
            _parameterTexture = Parameters["Texture"];
        }
        
        public CustomEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            _parameterCamPos = Parameters["CamPos"];
            _parameterWorld = Parameters["World"];
            _parameterWorldRot = Parameters["WorldRot"];
            _parameterViewProjection = Parameters["ViewProjection"];
            _parameterTesselation = Parameters["Tesselation"];
            _parameterRadius = Parameters["Radius"];
            _parameterTest = Parameters["Test"];
            _parameterCubeMap = Parameters["CubeMap"];
            _parameterEnableTexture = Parameters["EnableTexture"];
            _parameterTexture = Parameters["Texture"];
        }

        #endregion
    }
}