using System.IO;

namespace lokqlDx;

public class MruList
{
    private  string[] _items
        ;

    public static MruList LoadFromArray(string[] items)
    {
        return new MruList(items);
       
    }

    public MruList(string[] items)
    {
        _items = items.ToArray();
    }

    public void BringToTop(string path)
    {
        _items =  new [] {path}.Concat(_items.Where(i => i != path)).ToArray() ;
    }
    public readonly record struct MruItem(string Path,string Description);

    public MruItem[] GetItems()
    {
        return _items.Select(i=>
                new MruItem(i,GetHeaderForPath(i)))
            .Take(MaxItems)
            .ToArray();
    }
    private const int MaxItems = 10;
    private string GetHeaderForPath(string s)
    {
        return $"{Path.GetFileName(s)} ({Path.GetDirectoryName(s)})";
    }
}
