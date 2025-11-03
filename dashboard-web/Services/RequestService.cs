using Models;

namespace Services;

public static class RequestService
{
    public static async Task<IEnumerable<Motorbike>> GetMotorbikesRequest(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient("DashboardAPI");
        var response = await httpClient.GetAsync("/motorbikes");
        if (response.IsSuccessStatusCode)
          return await httpClient.GetFromJsonAsync<IEnumerable<Motorbike>>("/motorbikes") ?? Enumerable.Empty<Motorbike>();
        return Enumerable.Empty<Motorbike>();
    }
}
