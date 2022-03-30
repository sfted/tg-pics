namespace TgPics.Core.Models.Responses;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ListResponse<T>
{
    public ListResponse(List<T> items)
    {
        Count = items.Count;
        Items = items;
    }

    public ListResponse(int count, List<T> items)
    {
        Count = count;
        Items = items;
    }

    /// <summary>
    /// Общее количество доступных элементов (только при пагинации, 
    /// в противном случае Count = Items.Count!!!)<br/>
    /// Например, в БД лежит 20 сущностей, а в запросе мы
    /// попросили вернуть только первые 5, в этом случае
    /// ответ будет такой: Count = 20, а Items.Count = 5.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; }
}