namespace OpenMiNET.Noise.Api
{
    public interface ISimplexData2D {

        void AddToDeltaX(float val);

        void AddToDeltaY(float val);

        float GetDeltaX();

        void SetDeltaX(float deltaX);

        float GetDeltaY();

        void SetDeltaY(float deltaY);

        void Clear();

        void Request(float attn, float extrapolation, float gx, float gy, int giSph2, float dx, float dy);
        
        public interface IDataRequest {

            void Apply(float attn, float extrapolation, float gx, float gy, int giSph2, float dx, float dy);
        }
    }
}