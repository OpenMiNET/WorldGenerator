using OpenMiNET.Noise.Api;
using OpenMiNET.Noise.Primitives;

namespace OpenMiNET.Noise
{
    public class SimplexData2D : ISimplexData2D
    {
        private float _deltaX;
        private float _deltaY;

        public SimplexData2D()
        {
            this.Clear();
        }

        public float GetDeltaX()
        {
            return this._deltaX;
        }

        public void SetDeltaX(float deltaX)
        {
            this._deltaX = deltaX;
        }

        public float GetDeltaY()
        {
            return this._deltaY;
        }

        public void SetDeltaY(float deltaY)
        {
            this._deltaY = deltaY;
        }

        public void AddToDeltaX(float val)
        {
            this._deltaX += val;
        }

        public void AddToDeltaY(float val)
        {
            this._deltaY += val;
        }

        public void Clear()
        {
            this.SetDeltaX(0.0f);
            this.SetDeltaY(0.0f);
        }

        public virtual void Request(float attn, float extrapolation, float gx, float gy, int giSph2, float dx, float dy)
        {
            float attnSq = attn * attn;
            float extrap = attnSq * attnSq * extrapolation;
            AddToDeltaX((float) (extrap * SimplexPerlin.GradientsSph2[giSph2]));
            AddToDeltaY((float) (extrap * SimplexPerlin.GradientsSph2[giSph2 + 1]));
        }
    }
}