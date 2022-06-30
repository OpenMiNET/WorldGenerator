namespace OpenMiNET.Noise.Api
{
	public interface IModule
	{
		
	}
	public interface IModule1D
	{
		
	}

	public interface IModule2D : IModule
	{
		float GetValue(float x, float y);
	}
	
	public interface IModule3D : IModule
	{
		float GetValue(float x, float y, float z);
	}
}