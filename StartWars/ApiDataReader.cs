namespace StartWars;

public class ApiDataReader : IApiDataReader
{
    public async Task<string> Read(string baseAddress, string requestUrl)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(baseAddress);

        var response = await client.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}