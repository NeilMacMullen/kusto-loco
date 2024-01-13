namespace SourceGeneration
{
    public class Param
    {
        public readonly string Name;
        public readonly string Type;

        public Param(int index, string name, string type)
        {
            Index = index;
            Name = name.Trim();
            Type = type.Trim();
        }

        public int Index { get; }
        public bool IsString => Type.Contains("string");
    }
}