using System;
using OpenMiNET.Noise.Attributes;
using OpenMiNET.Noise.Maths;

namespace OpenMiNET.Noise.Modules
{
    public class VoronoiNoseModule : INoiseModule
    {
        public const float DefaultDisplacement = 1.0f;

        private INoiseModule Source { get; }

        public VoronoiNoseModule(INoiseModule source)
        {
            Source = source;
        }
        
        private float _displacement = DefaultDisplacement;

        private bool _distance;
        protected float _frequency = 1f;
        
        /// <summary>
        /// This noise module assigns each Voronoi cell with a random constant
        /// value from a coherent-noise function.  The <i>displacement
        /// value</i> controls the range of random values to assign to each
        /// cell.  The range of random values is +/- the displacement value.
        /// </summary>
        [Modifier]
        public float Displacement
        {
            get { return _displacement; }
            set { _displacement = value; }
        }

        /// <summary>
        /// Applying the distance from the nearest seed point to the output
        /// value causes the points in the Voronoi cells to increase in value
        /// the further away that point is from the nearest seed point.
        /// </summary>
        [Modifier]
        public bool Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        /// <summary>
        /// The number of cycles per unit length that a specific coherent-noise function outputs.
        /// </summary>
        [Modifier]
        public float Frequency
        {
            get { return this._frequency; }
            set { this._frequency = value; }
        }

        public int Size { get; set; } = 2;
        public float GetValue(float x, float y)
        {
            x *= _frequency;
            y *= _frequency;

            int xInt = (x > 0.0f ? (int)x : (int)x - 1);
            int yInt = (y > 0.0f ? (int)y : (int)y - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0.0f;
            float yCandidate = 0.0f;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int yCur = yInt - Size; yCur <= yInt + Size; yCur++)
            {
                for (int xCur = xInt - Size; xCur <= xInt + Size; xCur++)
                {
                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    var off = Source.GetValue(xCur, yCur);
                    
                    float xPos = xCur + off;
                    float yPos = yCur + off;
                    
                    float xDist = xPos - x;
                    float yDist = yPos - y;
                    float dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        // this seed point.
                        minDist = dist;
                        xCandidate = xPos;
                        yCandidate = yPos;
                    }
                }
            }

            float value;

            if (_distance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                value = (MathF.Sqrt(xDist * xDist + yDist * yDist)
                        ) * MathUtils.Sqrt2 - 1.0f;
            }
            else
                value = 0.0f;

            // Return the calculated distance with the displacement value applied.
            return value + (_displacement * Source.GetValue(
                                MathF.Floor(xCandidate),
                                MathF.Floor(yCandidate))
                   );
        }
        
        /// <summary>
        /// Generates an output value given the coordinates of the specified input value.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public float GetValue(float x, float y, float z)
        {
            //TODO This method could be more efficient by caching the seed values.
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            int xInt = (x > 0.0f ? (int) x : (int) x - 1);
            int yInt = (y > 0.0f ? (int) y : (int) y - 1);
            int zInt = (z > 0.0f ? (int) z : (int) z - 1);

            float minDist = 2147483647.0f;
            float xCandidate = 0.0f;
            float yCandidate = 0.0f;
            float zCandidate = 0.0f;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int zCur = zInt - Size; zCur <= zInt + Size; zCur++)
            {
                for (int yCur = yInt - Size; yCur <= yInt + Size; yCur++)
                {
                    for (int xCur = xInt - Size; xCur <= xInt + Size; xCur++)
                    {
                        // Calculate the position and distance to the seed point inside of
                        // this unit cube.
                        var off = Source.GetValue(xCur, yCur, zCur);
                        
                        float xPos = xCur + off;
                        float yPos = yCur + off;
                        float zPos = zCur + off;

                        float xDist = xPos - x;
                        float yDist = yPos - y;
                        float zDist = zPos - z;
                        float dist = xDist*xDist + yDist*yDist + zDist*zDist;

                        if (dist < minDist)
                        {
                            // This seed point is closer to any others found so far, so record
                            // this seed point.
                            minDist = dist;
                            xCandidate = xPos;
                            yCandidate = yPos;
                            zCandidate = zPos;
                        }
                    }
                }
            }

            float value;

            if (_distance)
            {
                // Determine the distance to the nearest seed point.
                float xDist = xCandidate - x;
                float yDist = yCandidate - y;
                float zDist = zCandidate - z;
                value = (MathF.Sqrt(xDist*xDist + yDist*yDist + zDist*zDist)
                    )*MathUtils.Sqrt3 - 1.0f;
            }
            else
                value = 0.0f;

            // Return the calculated distance with the displacement value applied.
            return value + (_displacement*Source.GetValue(
                 (MathF.Floor(xCandidate)),
                 (MathF.Floor(yCandidate)),
                (MathF.Floor(zCandidate)))
                );
        }
    }
}