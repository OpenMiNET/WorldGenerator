using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
    public class SpruceTree : TreeStructure
    {
        public override string Name
        {
            get { return "SpruceTree"; }
        }

        public override int MaxHeight
        {
            get { return 8; }
        }
    }
}

