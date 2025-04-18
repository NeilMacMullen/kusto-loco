namespace KustoLoco.Rendering.SixelSupport;

/// <summary>
///     Octree colour quantizer
/// </summary>
/// <remarks>
///     Taken from https://github.com/bacowan/cSharpColourQuantization
/// </remarks>
internal class Octree
{
    internal class PaletteQuantizer
    {
        private readonly IDictionary<int, List<Node>> _levelNodes;
        private readonly Node _root;

        internal PaletteQuantizer()
        {
            _root = new Node(this);
            _levelNodes = new Dictionary<int, List<Node>>();
            for (var i = 0; i < 8; i++) _levelNodes[i] = [];
        }

        internal void AddColour(Pixel colour) => _root.AddColour(colour, 0);

        internal void AddLevelNode(Node node, int level) => _levelNodes[level].Add(node);

        internal Pixel GetQuantizedColour(Pixel colour) => _root.GetColour(colour, 0);

        internal void Quantize(int colourCount)
        {
            var nodesToRemove = _levelNodes[7].Count - colourCount;
            var level = 6;
            var toBreak = false;
            while (level >= 0 && nodesToRemove > 0)
            {
                var leaves = _levelNodes[level]
                    .Where(n => n.ChildrenCount - 1 <= nodesToRemove)
                    .OrderBy(n => n.ChildrenCount)
                    .ToArray();
                foreach (var leaf in leaves)
                {
                    if (leaf.ChildrenCount > nodesToRemove)
                    {
                        toBreak = true;
                        continue;
                    }

                    nodesToRemove -= leaf.ChildrenCount - 1;
                    leaf.Merge();
                    if (nodesToRemove <= 0) break;
                }

                _levelNodes.Remove(level + 1);
                level--;
                if (toBreak) break;
            }
        }
    }

    internal class Node
    {
        private readonly PaletteQuantizer _parent;
        private Node?[] _children = new Node[8];

        internal Node(PaletteQuantizer parent)
        {
            _parent = parent;
        }

        private Pixel Colour { get; set; }
        private int Count { get; set; }

        internal int ChildrenCount => _children.Count(c => c != null);

        internal void AddColour(Pixel colour, int level)
        {
            if (level < 8)
            {
                var index = GetIndex(colour, level);
                if (_children[index] == null)
                {
                    var newNode = new Node(_parent);
                    _children[index] = newNode;
                    _parent.AddLevelNode(newNode, level);
                }

                _children[index]!.AddColour(colour, level + 1);
            }
            else
            {
                Colour = colour;
                Count++;
            }
        }

        internal Pixel GetColour(Pixel colour, int level)
        {
            if (ChildrenCount == 0) return Colour;

            var index = GetIndex(colour, level);
            return _children[index]!.GetColour(colour, level + 1);
        }

        private byte GetIndex(Pixel colour, int level)
        {
            byte ret = 0;
            var mask = Convert.ToByte(0b10000000 >> level);
            if ((colour.R & mask) != 0) ret |= 0b100;
            if ((colour.G & mask) != 0) ret |= 0b010;
            if ((colour.B & mask) != 0) ret |= 0b001;
            return ret;
        }

        internal void Merge()
        {
            var populatedNodes = _children
                .Where(c => c != null)
                .Select(c => new Tuple<Pixel, int>(c!.Colour, c.Count))
                .ToArray();
            Colour = Average(populatedNodes);
            Count = _children.Sum(c => c?.Count ?? 0);
            _children = new Node[8];
        }

        private static Pixel Average(Tuple<Pixel, int>[] colours)
        {
            var totals = colours.Sum(c => c.Item2);
            return Pixel.FromArgb(
                255,
                colours.Sum(c => c.Item1.R * c.Item2) / totals,
                colours.Sum(c => c.Item1.G * c.Item2) / totals,
                colours.Sum(c => c.Item1.B * c.Item2) / totals);
        }
    }
}
