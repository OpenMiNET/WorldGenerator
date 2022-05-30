using MiNET.Utils;
using OpenAPI.WorldGenerator.Utils.Noise.Api;
using OpenAPI.WorldGenerator.Utils.Noise.Primitives;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public abstract class SimplexData2D : ISimplexData2D
    {

        private float deltaX;
        private float deltaY;

        private SimplexData2D()
        {
            this.Clear();
        }

        /**
     * Gets a new {@link SimplexData2D.Disk} multi-evaluation data object for use in generating jitter effects.
     *
     * @return a new instance of SimplexData2D.Disk
     * @since 1.0.0
     */
        public static ISimplexData2D NewDisk()
        {
            return new Disk();
        }

        /**
     * Gets a new {@link SimplexData2D.Derivative} multi-evaluation data object for use in generating jitter effects.
     *
     * @return a new instance of SimplexData2D.Derivative
     * @since 1.0.0
     */
        public static ISimplexData2D NewDerivative()
        {
            return new Derivative();
        }

        public float GetDeltaX()
        {
            return this.deltaX;
        }

        public void SetDeltaX(float deltaX)
        {
            this.deltaX = deltaX;
        }

        public float GetDeltaY()
        {
            return this.deltaY;
        }

        public void SetDeltaY(float deltaY)
        {
            this.deltaY = deltaY;
        }

        public void AddToDeltaX(float val)
        {
            this.deltaX += val;
        }

        public void AddToDeltaY(float val)
        {
            this.deltaY += val;
        }

        public void Clear()
        {
            this.SetDeltaX(0.0f);
            this.SetDeltaY(0.0f);
        }

        public virtual ISimplexData2D.IDataRequest Request()
        {
            return new Disk.DiskDataRequest(this);
        }

        public class Disk : SimplexData2D
        {

            public Disk() : base()
            {

            }

            public override ISimplexData2D.IDataRequest Request()
            {
                return new DiskDataRequest(this);
            }

            public class DiskDataRequest : ISimplexData2D.IDataRequest
            {
                private SimplexData2D Data { get; }

                public DiskDataRequest(SimplexData2D data2D)
                {
                    Data = data2D;
                }

                public void Apply(float attn, float extrapolation, float gx, float gy, int gi_sph2, float dx,
                    float dy)
                {
                    float attnSq = attn * attn;
                    float extrap = attnSq * attnSq * extrapolation;
                    Data.AddToDeltaX((float) (extrap * SimplexPerlin.GradientsSph2[gi_sph2]));
                    Data.AddToDeltaY((float) (extrap * SimplexPerlin.GradientsSph2[gi_sph2 + 1]));
                }
            }
        }

        public class Derivative : SimplexData2D
        {

            public Derivative()
            {

            }

            public override ISimplexData2D.IDataRequest Request()
            {
                return new DerivativeDataRequest(this);
            }

            public class DerivativeDataRequest : ISimplexData2D.IDataRequest
            {
                private SimplexData2D Data { get; }

                public DerivativeDataRequest(SimplexData2D data2D)
                {
                    Data = data2D;
                }

                public void Apply(float attn, float extrapolation, float gx, float gy, int gi_sph2, float dx,
                    float dy)
                {
                    double attnSq = attn * attn;
                    Data.AddToDeltaX((float) ((gx * attn - 8f * dx * extrapolation) * attnSq * attn));
                    Data.AddToDeltaY((float) ((gy * attn - 8f * dy * extrapolation) * attnSq * attn));
                }
            }
        }
    }
}