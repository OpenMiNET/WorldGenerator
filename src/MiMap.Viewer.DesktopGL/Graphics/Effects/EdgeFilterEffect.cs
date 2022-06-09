// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Components;

namespace MiMap.Viewer.DesktopGL.Graphics.Effects
{
    /// <summary>
    /// Renders silhouette outlines and creases edge using edge detection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="EdgeFilter"/> reads the G-buffer (depth and normals) and creates silhouette
    /// outlines and crease edges. For example:
    /// </para>
    /// <para>
    /// <img src="../media/EdgeFilter.jpg" />
    /// </para>
    /// </remarks>
    public class EdgeFilter
    {
        //--------------------------------------------------------------

        #region Fields

        //--------------------------------------------------------------

        private readonly Effect _effect;
        private readonly EffectParameter _parameterViewportSize;
        private readonly EffectParameter _parameterHalfEdgeWidth;
        private readonly EffectParameter _parameterDepthThreshold;
        private readonly EffectParameter _parameterDepthSensitivity;
        private readonly EffectParameter _parameterNormalThreshold;
        private readonly EffectParameter _parameterNormalSensitivity;
        private readonly EffectParameter _parameterCameraBackward;
        private readonly EffectParameter _parameterSilhouetteColor;
        private readonly EffectParameter _parameterCreaseColor;
        private readonly EffectPass _passEdge;
        private readonly EffectPass _passOnePixelEdge;

        #endregion


        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the width of the edge outline in pixel.
        /// </summary>
        /// <value>The width of the edges in pixel. The default value 2.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative or 0.
        /// </exception>
        public float EdgeWidth
        {
            get { return _halfEdgeWidth * 2; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "The edge width must be greater than 0.");

                _halfEdgeWidth = value / 2;
            }
        }

        private float _halfEdgeWidth;


        /// <summary>
        /// Gets or sets the depth threshold for edge detection.
        /// </summary>
        /// <value>
        /// The depth threshold for edge detection in the range [0, 1]. The default value is 0.001.
        /// </value>
        /// <remarks>
        /// <para>
        /// Edges with a depth difference below the threshold are not detected. Use the following 
        /// equation to convert a distance in world space to the depth threshold:
        /// </para>
        /// <para>
        /// <i>depthThreshold</i> = <i>minDistance</i> / <i>farPlaneDistance</i>
        /// </para>
        /// <para>
        /// where <i>minDistance</i> is the minimal distance in world space unit that is required to
        /// detect an edge. <i>farPlaneDistance</i> is the distance to the far view plane.
        /// </para>
        /// </remarks>
        public float DepthThreshold { get; set; }


        /// <summary>
        /// Gets or sets the depth sensitivity of the edge detection.
        /// </summary>
        /// <value>The depth sensitivity of the edge detection [0, ∞]. The default value is 100.</value>
        /// <remarks>
        /// <para>
        /// The sensitivity defines the range at which edges are fully detected. The right sensitivity 
        /// can be found through the following equation: If <i>minDistance</i> is the minimal distance 
        /// in world space unit that is required to detect edges, <i>maxDistance</i> is the distance at
        /// which an edge is fully detected (100% certainty), and <i>farPlaneDistance</i> is the 
        /// distance to the far view plane, then
        /// </para>
        /// <para>
        /// <i>depthSensitivity</i> = <i>farPlaneDistance</i> / (<i>maxDistance</i> - <i>minDistance</i>
        /// </para>
        /// </remarks>
        public float DepthSensitivity { get; set; }


        /// <summary>
        /// Gets or sets the normal vector threshold of the edge detection.
        /// </summary>
        /// <value>The normal vector threshold of the edge detection. The default value is 0.1.</value>
        /// <remarks>
        /// <para>
        /// The normal vector of the source image is used to find crease edges. The normal threshold
        /// determines the minimum difference between normal vectors that is required to detect an edge.
        /// Use the following equation to convert from angles to the normal threshold:
        /// </para>
        /// <para>
        /// <i>normalThreshold</i> = 1 - cos(α)
        /// </para>
        /// </remarks>
        public float NormalThreshold { get; set; }


        /// <summary>
        /// Gets or sets the normal vector sensitivity of the edge detection.
        /// </summary>
        /// <value>The normal vector sensitivity [0, ∞]. The default value is 2.</value>
        public float NormalSensitivity { get; set; }


        /// <summary>
        /// Gets or sets the color of the silhouette edges.
        /// </summary>
        /// <value>The color of the silhouette edges.</value>
        public Vector4 SilhouetteColor { get; set; }


        /// <summary>
        /// Gets or sets the color of the crease edges.
        /// </summary>
        /// <value>The color of the crease edges.</value>
        public Vector4 CreaseColor { get; set; }

        #endregion


        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeFilter"/> class.
        /// </summary>
        /// <param name="graphicsService">The graphics service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graphicsService"/> is <see langword="null"/>.
        /// </exception>
        public EdgeFilter(ContentManager contentManager)
        {
            _effect = contentManager.Load<Effect>("Effects/PostProcess/EdgeFilter");
            _parameterViewportSize = _effect.Parameters["ViewportSize"];
            _parameterHalfEdgeWidth = _effect.Parameters["HalfEdgeWidth"];
            _parameterDepthThreshold = _effect.Parameters["DepthThreshold"];
            _parameterDepthSensitivity = _effect.Parameters["DepthSensitivity"];
            _parameterNormalThreshold = _effect.Parameters["NormalThreshold"];
            _parameterNormalSensitivity = _effect.Parameters["NormalSensitivity"];
            _parameterCameraBackward = _effect.Parameters["CameraBackward"];
            _parameterSilhouetteColor = _effect.Parameters["SilhouetteColor"];
            _parameterCreaseColor = _effect.Parameters["CreaseColor"];
            _passEdge = _effect.Techniques[0].Passes["Edge"];
            _passOnePixelEdge = _effect.Techniques[0].Passes["OnePixelEdge"];

            EdgeWidth = 2.0f;
            DepthThreshold = 0.001f; // = minDistance / farPlaneDistance
            DepthSensitivity = 1000; // = farPlaneDistance / (maxDistance - minDistance)
            NormalThreshold = 0.1f;
            NormalSensitivity = 2f;
            SilhouetteColor = new Vector4(0, 0, 0, 1);
            CreaseColor = new Vector4(0, 0, 0, 1);
        }

        #endregion


        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void Apply(CameraComponent camera)
        {
            var viewport = camera.Viewport;
            var cameraInverseView = camera.InverseView;
            var cameraBackwards = new Vector3(cameraInverseView.M31, cameraInverseView.M32, cameraInverseView.M33);
            
            // graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
            _parameterHalfEdgeWidth.SetValue(_halfEdgeWidth);
            _parameterDepthThreshold.SetValue(DepthThreshold);
            _parameterDepthSensitivity.SetValue(DepthSensitivity);
            _parameterNormalThreshold.SetValue(NormalThreshold);
            _parameterNormalSensitivity.SetValue(NormalSensitivity);
            _parameterCameraBackward.SetValue((Vector3)(cameraBackwards));
            _parameterSilhouetteColor.SetValue((Vector4)SilhouetteColor);
            _parameterCreaseColor.SetValue((Vector4)CreaseColor);

            var pass = (_halfEdgeWidth <= 0.5f) ? _passOnePixelEdge : _passEdge;
            pass.Apply();
        }

        #endregion
    }
}