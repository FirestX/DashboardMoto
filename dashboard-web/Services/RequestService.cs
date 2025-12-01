using Models;
using System.Text.Json;

namespace Services;

public static class RequestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<IEnumerable<Motorbike>> GetMotorbikesRequest(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient("DashboardAPI");
        var response = await httpClient.GetAsync("/motorbikes");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Motorbike>>(json, JsonOptions) ?? Enumerable.Empty<Motorbike>();
        }
        return Enumerable.Empty<Motorbike>();
    }
}
