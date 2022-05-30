namespace OpenAPI.WorldGenerator.Generators.Effects
{
    public abstract class HeightEffect
    {
        public abstract float Added(OverworldGeneratorV2 generator, float x, float y);

        public HeightEffect Plus(HeightEffect added)
        {
            return new SummedHeightEffect(this, added);
        }
    }
}