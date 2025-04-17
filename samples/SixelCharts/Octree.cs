internal class Octree
{
    public static IPixelSource Quantize(IPixelSource bitmap, int colourCount)
    {
        var quantizer = new PaletteQuantizer();
        for (var x = 0; x < bitmap.Width; x++)
        for (var y = 0; y < bitmap.Height; y++)
        {
            var colour = bitmap.GetPixel(x, y);
            quantizer.AddColour(colour);
        }

        quantizer.Quantize(colourCount);
        var ret = new QuantizedPixelSource(bitmap.Width, bitmap.Height);
        for (var x = 0; x < bitmap.Width; x++)
        for (var y = 0; y < bitmap.Height; y++)
        {
            var colour = quantizer.GetQuantizedColour(bitmap.GetPixel(x, y));
            ret.SetPixel(x, y, colour);
        }

        return ret;
    }

    private class PaletteQuantizer
    {
        private readonly Node Root;
        private readonly IDictionary<int, List<Node>> levelNodes;

        public PaletteQuantizer()
        {
            Root = new Node(this);
            levelNodes = new Dictionary<int, List<Node>>();
            for (var i = 0; i < 8; i++) levelNodes[i] = new List<Node>();
        }

        public void AddColour(Pixel colour)
        {
            Root.AddColour(colour, 0);
        }

        public void AddLevelNode(Node node, int level)
        {
            levelNodes[level].Add(node);
        }

        public Pixel GetQuantizedColour(Pixel colour)
        {
            return Root.GetColour(colour, 0);
        }

        public void Quantize(int colourCount)
        {
            var nodesToRemove = levelNodes[7].Count - colourCount;
            var level = 6;
            var toBreak = false;
            while (level >= 0 && nodesToRemove > 0)
            {
                var leaves = levelNodes[level]
                    .Where(n => n.ChildrenCount - 1 <= nodesToRemove)
                    .OrderBy(n => n.ChildrenCount);
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

                levelNodes.Remove(level + 1);
                level--;
                if (toBreak) break;
            }
        }
    }

    private class Node
    {
        private readonly PaletteQuantizer parent;
        private Node[] Children = new Node[8];

        public Node(PaletteQuantizer parent)
        {
            this.parent = parent;
        }

        private Pixel Colour { get; set; }
        private int Count { get; set; }

        public int ChildrenCount => Children.Count(c => c != null);

        public void AddColour(Pixel colour, int level)
        {
            if (level < 8)
            {
                var index = GetIndex(colour, level);
                if (Children[index] == null)
                {
                    var newNode = new Node(parent);
                    Children[index] = newNode;
                    parent.AddLevelNode(newNode, level);
                }

                Children[index].AddColour(colour, level + 1);
            }
            else
            {
                Colour = colour;
                Count++;
            }
        }

        public Pixel GetColour(Pixel colour, int level)
        {
            if (ChildrenCount == 0) return Colour;

            var index = GetIndex(colour, level);
            return Children[index].GetColour(colour, level + 1);
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

        public void Merge()
        {
            Colour = Average(Children.Where(c => c != null).Select(c => new Tuple<Pixel, int>(c.Colour, c.Count)));
            Count = Children.Sum(c => c?.Count ?? 0);
            Children = new Node[8];
        }

        private static Pixel Average(IEnumerable<Tuple<Pixel, int>> colours)
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