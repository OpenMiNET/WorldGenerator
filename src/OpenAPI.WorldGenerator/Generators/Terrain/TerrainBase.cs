using System;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public abstract class TerrainBase
    {
        private static float _minimumOceanFloor = 20.01f; // The lowest Y coord an ocean floor is allowed to be.

        private static float
            _minimumDuneHeight = 21f; // The strength factor to which the dune height config option is added.

        protected float MinDuneHeight; // The strength factor to which the dune height config option is added.
        protected float GroundNoiseAmplitudeHills;
        protected float GroundVariation;
        protected float RollingHillsMaxHeight;
        protected float BaseHeight; // added as most terrains have this;
        protected float GroundNoise;

        public TerrainBase(float baseHeight)
        {

            this.BaseHeight = baseHeight;
            this.MinDuneHeight = _minimumDuneHeight;
            this.GroundVariation = 2f;
            this.GroundNoise = this.BaseHeight;
            this.GroundNoiseAmplitudeHills = 6f;
            this.RollingHillsMaxHeight = 80f;
        }

        public TerrainBase() : this(68f)
        {

        }

        public static float BlendedHillHeight(float simplex)
        {
            // this takes a simplex supposed to vary from -1 to 1
            // and produces an output which varies from 0 to 1 non-linearly
            // with the value of 0 mapped to about 0.15 and smooth transition
            // the purpose is to make hills above plains without significant deadvalleys
            float result = simplex + 1;
            result = result * result * result + 10;
            result = (float) MathF.Pow(result, .33333333333333f);
            result = result / 0.46631f; // this is the different between the values for -1 and 1,
            //so normalizing to a distance of 1
            result = result - 4.62021f; // subtracting the result for input -1 so we actually get 0 to 1
            return result;
        }

        public static float BlendedHillHeight(float simplex, float turnAt)
        {
            // like blendedHillHeight, but the effect of zero occurs at the turnAt parameter instead
            float adjusted = (1f - (1f - simplex) / (1f - turnAt));
            return BlendedHillHeight(adjusted);
        }

        public static float Above(float limited, float limit)
        {

            if (limited > limit)
            {
                return limited - limit;
            }

            return 0f;
        }

        public static float UnsignedPower(float number, float power)
        {

            if (number > 0)
            {
                return (float) MathF.Pow(number, power);
            }

            //(else)
            return (-1f) * (float) MathF.Pow((-1f) * number, power);
        }

        public static float Hills(float x, float y, float hillStrength, OverworldGeneratorV2 generator)
        {

            float m = generator.SimplexInstance(0).GetValue(x / 150f, y / 150f);
            m = BlendedHillHeight(m, 0.2f);

            float sm = generator.SimplexInstance(2)
                .GetValue(x / 55, y / 55); // there are artifacts if this is close to a multiple of 16
            sm = BlendedHillHeight(sm, 0.2f);
            //sm = sm*0.8f;
            sm *= sm * m;
            m += sm / 3f;

            return m * hillStrength;
        }

        public static float GetTerrainBase(OverworldGeneratorV2 generator, float river) {

            return generator.Preset.GetTerrainBase() * river;
        }
        
        public static float GetGroundNoise(int x, int y, float amplitude, OverworldGeneratorV2 generator)
        {

            float h = BlendedHillHeight(generator.SimplexInstance(0).GetValue(x / 49f, y / 49f), 0.2f) * amplitude;
            h += BlendedHillHeight(generator.SimplexInstance(1).GetValue(x / 23f, y / 23f), 0.2f) * amplitude / 2f;
            h += BlendedHillHeight(generator.SimplexInstance(2).GetValue(x / 11f, y / 11f), 0.2f) * amplitude / 4f;
            return h;
        }

        public static float GetGroundNoise(float x, float y, float amplitude, OverworldGeneratorV2 generator)
        {

            float h = BlendedHillHeight(generator.SimplexInstance(0).GetValue(x / 49f, y / 49f), 0.2f) * amplitude;
            h += BlendedHillHeight(generator.SimplexInstance(1).GetValue(x / 23f, y / 23f), 0.2f) * amplitude / 2f;
            h += BlendedHillHeight(generator.SimplexInstance(2).GetValue(x / 11f, y / 11f), 0.2f) * amplitude / 4f;
            return h;
        }

        public static float MountainCap(float m)
        {
            // heights can "blow through the ceiling" so pull more extreme values down a bit

            if (m > 160)
            {
                m = 160 + (m - 160) * .75f;
                if (m > 180)
                {
                    m = 180 + (m - 180f) * .75f;
                }
            }

            return m;
        }

        public static float Riverized(OverworldGeneratorV2 generator, float height, float river)
        {
            var baseH = generator.Preset.SeaLevel + 0.45f;
            if (height < baseH)
            {
                return height;
            }

            // experimental adjustment to make riverbanks more varied
            float adjustment = (height - baseH) / 10f + .6f;
            river = BayesianAdjustment(river, adjustment);
            return baseH + (height - baseH) * river;
        }

        public static float TerrainBeach(int x, int y, OverworldGeneratorV2 generator, float river, float baseHeight)
        {
            return Riverized(generator, baseHeight + GetGroundNoise(x, y, 4f, generator), river);
        }

        public static float TerrainBryce(int x, int y, OverworldGeneratorV2 generator, float river, float height)
        {

            var simplex = generator.SimplexInstance(0);
            float sn = simplex.GetValue(x / 2f, y / 2f) * 0.5f + 0.5f;
            sn += simplex.GetValue(x, y) * 0.2f + 0.2f;
            sn += simplex.GetValue(x / 4f, y / 4f) * 4f + 4f;
            sn += simplex.GetValue(x / 8f, y / 8f) * 2f + 2f;
            float n = height / sn * 2;
            n += simplex.GetValue(x / 64f, y / 64f) * 4f;
            n = (sn < 6) ? n : 0f;
            return Riverized(generator, generator.Preset.GetTerrainBase() + n, river);
        }

        public static float TerrainFlatLakes(int x, int y, OverworldGeneratorV2 generator, float river, float baseHeight)
        {
            /*float h = simplex.GetValue(x / 300f, y / 300f) * 40f * river;
            h = h > hMax ? hMax : h;
            h += simplex.GetValue(x / 50f, y / 50f) * (12f - h) * 0.4f;
            h += simplex.GetValue(x / 15f, y / 15f) * (12f - h) * 0.15f;*/

            float ruggedNoise = generator.SimplexInstance(1).GetValue(
                x / generator.Preset.DepthNoiseScaleX,
                y / generator.Preset.DepthNoiseScaleZ
            );

            ruggedNoise = BlendedHillHeight(ruggedNoise);
            float h = GetGroundNoise(x, y, 2f * (ruggedNoise + 1f), generator); // ground noise
            return Riverized(generator, baseHeight + h, river);
        }

        public static float TerrainForest(int x, int y, OverworldGeneratorV2 generator, float river, float baseHeight)
        {
            var simplex = generator.SimplexInstance(0);

            double h = simplex.GetValue(x / 100f, y / 100f) * 8d;
            h += simplex.GetValue(x / 30f, y / 30f) * 4d;
            h += simplex.GetValue(x / 15f, y / 15f) * 2d;
            h += simplex.GetValue(x / 7f, y / 7f);

            return Riverized(generator, baseHeight + 20f + (float) h, river);
        }

        public static float TerrainGrasslandFlats(int x, int y, OverworldGeneratorV2 generator, float river,
            float mPitch, float baseHeight)
        {

            var simplex = generator.SimplexInstance(0);
            float h = simplex.GetValue(x / 100f, y / 100f) * 7;
            h += simplex.GetValue(x / 20f, y / 20f) * 2;

            float m = simplex.GetValue(x / 180f, y / 180f) * 35f * river;
            m *= m / mPitch;

            float sm = BlendedHillHeight(simplex.GetValue(x / 30f, y / 30f)) * 8f;
            sm *= m / 20f > 3.75f ? 3.75f : m / 20f;
            m += sm;

            return Riverized(generator, baseHeight + h + m, river);
        }

        public static float TerrainGrasslandHills(int x, int y, OverworldGeneratorV2 generator, float river,
            float vWidth, float vHeight, float hWidth, float hHeight, float bHeight)
        {

            float h = generator.SimplexInstance(0).GetValue(x / vWidth, y / vWidth);
            h = BlendedHillHeight(h, 0.3f);

            float m = generator.SimplexInstance(1).GetValue(x / hWidth, y / hWidth);
            m = BlendedHillHeight(m, 0.3f) * h;
            m *= m;

            h *= vHeight * river;
            m *= hHeight * river;

            h += TerrainBase.GetGroundNoise(x, y, 4f, generator);

            return Riverized(generator, bHeight + h, river) + m;
        }

        public static float TerrainGrasslandMountains(int x, int y, OverworldGeneratorV2 generator, float river,
            float hFactor, float mFactor, float baseHeight)
        {

            var simplex0 = generator.SimplexInstance(0);
            float h = simplex0.GetValue(x / 100f, y / 100f) * hFactor;
            h += simplex0.GetValue(x / 20f, y / 20f) * 2;

            float m = simplex0.GetValue(x / 230f, y / 230f) * mFactor * river;
            m *= m / 35f;
            m = m > 70f ? 70f + (m - 70f) / 2.5f : m;

            float c = generator.SimplexInstance(4).GetValue(x / 30f, y / 30f, 1f) * (m * 0.30f);

            float sm = simplex0.GetValue(x / 30f, y / 30f) * 8f + simplex0.GetValue(x / 8f, y / 8f);
            sm *= m / 20f > 2.5f ? 2.5f : m / 20f;
            m += sm;

            m += c;

            return Riverized(generator, baseHeight + h + m, river);
        }

        public static float TerrainHighland(float x, float y, OverworldGeneratorV2 generator, float river, float start,
            float width, float height, float baseAdjust)
        {

            float h = generator.SimplexInstance(0).GetValue(x / width, y / width) * height * river; //-140 to 140
            h = h < start ? start + ((h - start) / 4.5f) : h;

            if (h < 0f)
            {
                h = 0; //0 to 140
            }

            if (h > 0f)
            {
                float st = h * 1.5f > 15f ? 15f : h * 1.5f; // 0 to 15
                h += generator.SimplexInstance(4).GetValue(x / 70f, y / 70f, 1f) * st; // 0 to 155
                h = h * river;
            }

            h += BlendedHillHeight(generator.SimplexInstance(0).GetValue(x / 20f, y / 20f), 0f) * 4f;
            h += BlendedHillHeight(generator.SimplexInstance(0).GetValue(x / 12f, y / 12f), 0f) * 2f;
            h += BlendedHillHeight(generator.SimplexInstance(0).GetValue(x / 5f, y / 5f), 0f) * 1f;

            if (h < 0)
            {
                h = h / 2f;
            }

            if (h < -3)
            {
                h = (h + 3f) / 2f - 3f;
            }

            return (GetTerrainBase(generator, river)) + (h + baseAdjust) * river;
        }

        public static float TerrainLonelyMountain(int x, int y, OverworldGeneratorV2 generator, float river,
            float strength, float width, float terrainHeight)
        {

            var simplex0 = generator.SimplexInstance(0);
            float h = BlendedHillHeight(simplex0.GetValue(x / 20f, y / 20f), 0) * 3;
            h += BlendedHillHeight(simplex0.GetValue(x / 7f, y / 7f), 0) * 1.3f;

            float m = simplex0.GetValue(x / width, y / width) * strength * river;
            m *= m / 35f;
            m = m > 70f ? 70f + (m - 70f) / 2.5f : m;

            float st = m * 0.7f;
            st = st > 20f ? 20f : st;
            float c = generator.SimplexInstance(4).GetValue(x / 30f, y / 30f, 1f) * (5f + st);

            float sm = simplex0.GetValue(x / 30f, y / 30f) * 8f + simplex0.GetValue(x / 8f, y / 8f);
            sm *= (m + 10f) / 20f > 2.5f ? 2.5f : (m + 10f) / 20f;
            m += sm;

            m += c;

            // the parameters can "blow through the ceiling" so pull more extreme values down a bit
            // this should allow a height parameter up to about 120
            if (m > 90)
            {
                m = 90f + (m - 90f) * .75f;
                if (m > 110)
                {
                    m = 110f + (m - 110f) * .75f;
                }
            }

            return Riverized(generator, terrainHeight + h + m, river);
        }

        public static float TerrainMarsh(int x, int y, OverworldGeneratorV2 generator, float baseHeight, float river)
        {
            var simplex = generator.SimplexInstance(0);
            float h = simplex.GetValue(x / 130f, y / 130f) * 20f;

            h += simplex.GetValue(x / 12f, y / 12f) * 2f;
            h += simplex.GetValue(x / 18f, y / 18f) * 4f;

            h = h < 8f ? 0f : h - 8f;

            if (h == 0f)
            {
                h += simplex.GetValue(x / 20f, y / 20f) + simplex.GetValue(x / 5f, y / 5f);
                h *= 2f;
            }

            return Riverized(generator, baseHeight + h, river);
        }

        public static float TerrainMesa(int x, int y, OverworldGeneratorV2 generator, float river, float border) {

            var simplex = generator.SimplexInstance(0);
            float b = simplex.GetValue(x / 130f, y / 130f) * 50f * river;
            b *= b / 40f;

            float hn = simplex.GetValue(x / 12f, y / 12f);

            float sb = 0f;
            if (b > 2f) {
                sb = (b - 2f) / 2f;
                sb = sb < 0f ? 0f : sb > 5.5f ? 5.5f : sb;
                sb = hn * sb;
            }
            b += sb;

            b = b < 0.1f ? 0.1f : b;

            float c1 = 0f;
            if (b > 1f) {
                c1 = b > 5.5f ? 4.5f : b - 1f;
                c1 *= 3;
            }

            float c2 = 0f;
            if (b > 5.5f && border > 0.95f + hn * 0.09f) {
                c2 = b > 6f ? 0.5f : b - 5.5f;
                c2 *= 35;
            }

            float bn = 0f;
            if (b < 7f) {
                float bnh = 5f - b;
                bn += simplex.GetValue(x / 70f, y / 70f) * (bnh * 0.4f);
                bn += simplex.GetValue(x / 20f, y / 20f) * (bnh * 0.3f);
            }

            float w = simplex.GetValue(x / 80f, y / 80f) * 25f;
            w *= w / 25f;

            b += c1 + c2 + bn - w;

            return 74f + b;
        }
        
        public static float TerrainOcean(int x, int y, OverworldGeneratorV2 generator, float river, float averageFloor)
        {

            var simplex = generator.SimplexInstance(0);
            float h = simplex.GetValue(x / 300f, y / 300f) * 8f * river;
            //h = h > 3f ? 3f : h;
            h += simplex.GetValue(x / 50f, y / 50f) * 2f;
            h += simplex.GetValue(x / 15f, y / 15f) * 1f;

            float floNoise = averageFloor + h;
            floNoise = floNoise < _minimumOceanFloor ? _minimumOceanFloor : floNoise;

            return floNoise;
        }

        public static float TerrainOceanCanyon(int x, int y, OverworldGeneratorV2 generator, float river, float[] height,
            float border, float strength, int heightLength, bool booRiver)
        {
            //float b = simplex.GetValue(x / cWidth, y / cWidth) * cHeigth * river;
            //b *= b / cStrength;
            var simplex = generator.SimplexInstance(0);
            river *= 1.3f;
            river = river > 1f ? 1f : river;
            float r = simplex.GetValue(x / 100f, y / 100f) * 50f;
            r = r < -7.4f ? -7.4f : r > 7.4f ? 7.4f : r;
            float b = (17f + r) * river;

            float hn = simplex.GetValue(x / 12f, y / 12f) * 0.5f;
            float sb = 0f;
            if (b > 0f)
            {
                sb = b;
                sb = sb > 7f ? 7f : sb;
                sb = hn * sb;
            }

            b += sb;

            float cTotal = 0f;
            float cTemp;

            for (int i = 0; i < heightLength; i += 2)
            {
                cTemp = 0;
                if (b > height[i] && border > 0.6f + (height[i] * 0.015f) + hn * 0.2f)
                {
                    cTemp = b > height[i] + height[i + 1] ? height[i + 1] : b - height[i];
                    cTemp *= strength;
                }

                cTotal += cTemp;
            }


            float bn = 0f;
            if (booRiver)
            {
                if (b < 5f)
                {
                    bn = 5f - b;
                    for (int i = 0; i < 3; i++)
                    {
                        bn *= bn / 4.5f;
                    }
                }
            }
            else if (b < 5f)
            {
                bn = (simplex.GetValue(x / 7f, y / 7f) * 1.3f + simplex.GetValue(x / 15f, y / 15f) * 2f) * (5f - b) *
                     0.2f;
            }

            b += cTotal - bn;

            float floNoise = 30f + b;
            floNoise = floNoise < _minimumOceanFloor ? _minimumOceanFloor : floNoise;

            return floNoise;
        }

        public static float TerrainPlains(int x, int y, OverworldGeneratorV2 generator, float river, float stPitch,
            float stFactor, float hPitch, float heightScale, float baseHeight)
        {

            var simplex = generator.SimplexInstance(0);
            float floNoise;
            float st = (simplex.GetValue(x / stPitch, y / stPitch) + 0.38f) * stFactor * river;
            st = st < 0.2f ? 0.2f : st;

            float height = simplex.GetValue(x / hPitch, y / hPitch) * st * 2f;
            height = height > 0f ? -height : height;
            height += st;
            height *= height / heightScale;
            height += st;

            floNoise = Riverized(generator, baseHeight + height, river);
            return floNoise;
        }

        public static float TerrainPlateau(float x, float y, OverworldGeneratorV2 generator, float river, float[] height,
            float border, float strength, int heightLength, float selectorWaveLength, bool isM)
        {

            var simplex = generator.SimplexInstance(0);
            river = river > 1f ? 1f : river;
            float border2 = border * 4 - 2.5f;
            border2 = border2 > 1f ? 1f : (border2 < 0f) ? 0f : border2;
            float b = simplex.GetValue(x / 40f, y / 40f) * 1.5f;

            float sn = simplex.GetValue(x / selectorWaveLength, y / selectorWaveLength) * 0.5f + 0.5f;
            sn *= border2;
            sn *= river;
            sn += simplex.GetValue(x / 4f, y / 4f) * 0.01f + 0.01f;
            sn += simplex.GetValue(x / 2f, y / 2f) * 0.01f + 0.01f;
            float n, hn, stepUp;
            for (int i = 0; i < heightLength; i += 2)
            {
                n = (sn - height[i + 1]) / (1 - height[i + 1]);
                n = n * strength;
                n = (n < 0f) ? 0f : (n > 1f) ? 1f : n;
                hn = height[i] * 0.5f * ((sn * 2f) - 0.4f);
                hn = (hn < 0) ? 0f : hn;
                stepUp = 0f;
                if (sn > height[i + 1])
                {
                    stepUp += (height[i] * n);
                    if (isM)
                    {
                        stepUp += simplex.GetValue(x / 20f, y / 20f) * 3f * n;
                        stepUp += simplex.GetValue(x / 12f, y / 12f) * 2f * n;
                        stepUp += simplex.GetValue(x / 5f, y / 5f) * 1f * n;
                    }
                }

                if (i == 0 && stepUp < hn)
                {
                    b += hn;
                }

                stepUp = (stepUp < 0) ? 0f : stepUp;
                b += stepUp;
            }

            if (isM)
            {
                b += simplex.GetValue(x / 12, y / 12) * sn;
            }

            //Counteracts smoothing
            b /= border;

            return Riverized(generator, generator.Preset.GetTerrainBase(), river) + b;
        }

        public static float TerrainPolar(float x, float y, OverworldGeneratorV2 generator, float river, float stPitch,
            float stFactor, float hPitch, float hDivisor, float baseHeight)
        {

            var simplex = generator.SimplexInstance(0);
            float floNoise;
            float st = (simplex.GetValue(x / stPitch, y / stPitch) + 0.38f) * stFactor * river;
            st = st < 0.1f ? 0.1f : st;

            float h = simplex.GetValue(x / hPitch, y / hPitch) * st * 2f;
            h = h > 0f ? -h : h;
            h += st;
            h *= h / hDivisor;
            h += st;

            floNoise = Riverized(generator, baseHeight + h, river);
            return floNoise;
        }

        public static float TerrainRollingHills(int x, int y, OverworldGeneratorV2 generator, float river,
            float hillStrength, float addedHeight, float groundNoiseAmplitudeHills, float lift)
        {

            float groundNoise = GetGroundNoise(x, y, groundNoiseAmplitudeHills, generator);
            float m = Hills(x, y, hillStrength, generator);
            float floNoise = addedHeight + groundNoise + m;
            return Riverized(generator, floNoise + lift, river);
        }

        public static float TerrainRollingHills(int x, int y, OverworldGeneratorV2 generator, float river,
            float hillStrength, float groundNoiseAmplitudeHills, float baseHeight)
        {

            float groundNoise = GetGroundNoise(x, y, groundNoiseAmplitudeHills, generator);
            float m = Hills(x, y, hillStrength, generator);
            float floNoise = groundNoise + m;
            return Riverized(generator, floNoise + baseHeight, river);
        }

        public static float TerrainVolcano(int x, int y, OverworldGeneratorV2 generator, float border, float baseHeight)
        {

            var simplex = generator.SimplexInstance(0);
            var cellularNoise = generator.CellularInstance(0);

            float st = 15f - (float) (cellularNoise.Eval2D(x / 500d, y / 500d).ShortestDistance * 42d) +
                       (simplex.GetValue(x / 30f, y / 30f) * 2f);

            float h = st < 0f ? 0f : st;
            h = h < 0f ? 0f : h;
            h += (h * 0.4f) * ((h * 0.4f) * 2f);

            if (h > 10f)
            {
                float d2 = (h - 10f) / 1.5f > 30f ? 30f : (h - 10f) / 1.5f;
                h += (float)cellularNoise.Eval2D(x / 25D, y / 25D).ShortestDistance * d2;
            }

            h += simplex.GetValue(x / 18f, y / 18f) * 3;
            h += simplex.GetValue(x / 8f, y / 8f) * 2;

            return baseHeight + h * border;
        }

        public static float GetRiverStrength(int x, int z, OverworldGeneratorV2 generator)
        {

            int worldX = x;
            int worldZ = z;
            double pX = worldX;
            double pZ = worldZ;
            var jitterData = SimplexData2D.NewDisk();
            //New river curve function. No longer creates worldwide curve correlations along cardinal axes.
            generator.SimplexInstance(1).GetValue((float) worldX / 240.0f, (float) worldZ / 240.0f, jitterData);
            pX += jitterData.GetDeltaX() * generator.RiverLargeBendSize;
            pZ += jitterData.GetDeltaY() * generator.RiverLargeBendSize;

            generator.SimplexInstance(2).GetValue((float) worldX / 80.0f, (float) worldZ / 80.0f, jitterData);
            pX += jitterData.GetDeltaX() * generator.RiverSmallBendSize;
            pZ += jitterData.GetDeltaY() * generator.RiverSmallBendSize;

            pX /= generator.RiverSeperation;
            pZ /= generator.RiverSeperation;

            //New cellular noise.
            double riverFactor = generator.CellularInstance(0).Eval2D(pX, pZ).InteriorValue;

            // the output is a curved function of relative distance from the center, so adjust to make it flatter
            riverFactor = BayesianAdjustment((float) riverFactor, 0.5f);
            if (riverFactor > generator.RiverValleyLevel)
            {
                return 0;
            } // no river effect

            return (float) (riverFactor / generator.RiverValleyLevel - 1f);
        }
        
        public static float CalcCliff(int x, int z, float[] noise)
        {
            var   index = NoiseMap.GetIndex(x, z);
            
            float cliff = 0f;
            if (x > 0)
            {
                cliff = Math.Max(cliff, Math.Abs(noise[index] - noise[NoiseMap.GetIndex(x - 1, z)]));
            }

            if (z > 0)
            {
                cliff = Math.Max(cliff, Math.Abs(noise[index] - noise[NoiseMap.GetIndex(x, z - 1)]));
            }

            if (x < 15)
            {
                cliff = Math.Max(cliff, Math.Abs(noise[index] - noise[NoiseMap.GetIndex(x + 1, z)]));
            }

            if (z < 15)
            {
                cliff = Math.Max(cliff, Math.Abs(noise[index] - noise[NoiseMap.GetIndex(x, z + 1)]));
            }

            return cliff;
        }

        /*public static void calcSnowHeight(int x, int y, int z, ChunkPrimer primer, float[] noise) {
            if (y < 254) {
                byte h = (byte) ((noise[x * 16 + z] - ((int) noise[x * 16 + z])) * 8);
                if (h > 7) {
                    primer.setBlockState(x, y + 2, z, Blocks.SNOW_LAYER.getDefaultState());
                    primer.setBlockState(x, y + 1, z, Blocks.SNOW_LAYER.getDefaultState().withProperty(BlockSnow.LAYERS, 7));
                }
                else if (h > 0) {
                    primer.setBlockState(x, y + 1, z, Blocks.SNOW_LAYER.getDefaultState().withProperty(BlockSnow.LAYERS, (int) h));
                }
            }
        }*/

        public static float BayesianAdjustment(float probability, float multiplier)
        {
            // returns the original probability adjusted for the multiplier to the confidence ratio
            // useful for computationally cheap remappings within [0,1]
            if (probability >= 1)
            {
                return probability;
            }

            if (probability <= 0)
            {
                return probability;
            }

            float newConfidence = probability * multiplier / (1f - probability);
            return newConfidence / (1f + newConfidence);
        }

        
        public abstract float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river);
    }

}