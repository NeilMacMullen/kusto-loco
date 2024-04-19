namespace KustoLoco.FileFormats;

public class KustoSettings
{
    
    private readonly Dictionary<string,string> _settings = new Dictionary<string,string>();
    public void Set(string setting, string value) => _settings[setting] = value;
    public void Set(string setting, bool value) => Set(setting,value.ToString());

    public void Set(string setting, int value)
        => Set(setting, value.ToString());


    public string Get(string setting, string fallbackValue)
    {
        return _settings.GetValueOrDefault(setting, fallbackValue);
    }


    public int Get(string setting, int fallbackValue)
    {
        var s = Get(setting, string.Empty);
        return int.TryParse(s,out var v) ?
            v : fallbackValue;
    }

    public bool Get(string setting, bool fallbackValue)
    {

        var s = Get(setting, string.Empty);
        if(bool.TryParse(s,out var b))
            return b;
        if (int.TryParse(s, out var v))
            return v > 0;
        return fallbackValue;
    }

}
