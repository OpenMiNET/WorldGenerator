namespace OpenMiNET.Noise.Modules
{
	public class AverageSelectorModule : FilterNoise, INoiseModule
	{
		private INoiseModule _input1, _input2;
		public AverageSelectorModule(INoiseModule input1, INoiseModule input2)
		{
			_input1 = input1;
			_input2 = input2;
		}
		
		/// <inheritdoc />
		public float GetValue(float x, float y)
		{
			return (_input1.GetValue(x, y) + _input2.GetValue(x, y)) / 2f;
		}

		/// <inheritdoc />
		public float GetValue(float x, float y, float z)
		{
			return (_input1.GetValue(x, y, z) + _input2.GetValue(x, y, z)) / 2f;
		}
	}
}