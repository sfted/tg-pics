namespace TgPics.Desktop.Services;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

internal interface ISettingsService
{
    T Get<T>(string key);
    void Set(string key, object value);
}

internal class SettingsService : ISettingsService
{
    public SettingsService()
    {
        filename = $"{ApplicationData.Current.LocalFolder.Path}/settings.json";

        if (File.Exists(filename))
            values = JsonConvert.DeserializeObject<Dictionary<string, object>>
                (File.ReadAllText(filename));
        else
            values = new();
    }

    readonly string filename;
    readonly Dictionary<string, object> values;

    public void Set(string key, object value)
    {
        if (values.ContainsKey(key))
            values[key] = value;
        else
            values.Add(key, value);

        Save();
    }

    public T Get<T>(string key) =>
        values.ContainsKey(key) ? (T)values[key] : default;

    private void Save() =>
        File.WriteAllText(filename, JsonConvert.SerializeObject(values));
}
