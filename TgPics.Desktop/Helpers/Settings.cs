namespace TgPics.Desktop.Helpers;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

// мне просто лень.
// нормальные настройки (которые LocalSettings) не работали,
// и мне пришлось выкручиваться...
public sealed class Settings
{
    private static Settings instance;
    private readonly string filename;
    private readonly Dictionary<string, object> values;

    private Settings()
    {
        filename = $"{ApplicationData.Current.LocalFolder.Path}/settings.json";

        if (File.Exists(filename))
            values = JsonConvert.DeserializeObject<Dictionary<string, object>>
                (File.ReadAllText(filename));
        else
            values = new();

        Debug.WriteLine(JsonConvert.SerializeObject(values));
    }

    public static Settings Instance
    {
        get
        {
            if (instance == null)
                instance = new Settings();

            return instance;
        }
    }

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