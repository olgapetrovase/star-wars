using StartWars.DTO;
using StartWars.Model;
using StartWars.UserInteraction;
using System.Text.Json;

namespace StartWars.DataAccess;

public class PlanetsFromApiReader : IPlanetsReader
{
    private readonly IApiDataReader _apiDataReader;
    private readonly IApiDataReader _mockApiDataReader;
    private readonly IUserInteractor _userInteractor;
    public PlanetsFromApiReader(
        IApiDataReader apiDataReader,
        IApiDataReader mockApiDataReader,
        IUserInteractor userInteractor)
    {
        _apiDataReader = apiDataReader;
        _mockApiDataReader = mockApiDataReader;
        _userInteractor = userInteractor;
    }

    public async Task<IEnumerable<Planet>> Read()
    {
        string? json = null;
        string baseAddress = "https://swapi.dev/";
        string requestUrl = "api/planets";
        try
        {
            json = await _apiDataReader.Read(baseAddress, requestUrl);
        }
        catch (HttpRequestException ex)
        {
            _userInteractor.ShowMessage("API request was unsuccessful. " +
                "Switching to mock data. " +
                "Exception message: " + ex.Message);
        }
        json ??= await _mockApiDataReader.Read(baseAddress, requestUrl);

        var root = JsonSerializer.Deserialize<Root>(json);

        return ToPlanets(root);
    }

    private static IEnumerable<Planet> ToPlanets(Root? root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }
        var planets = new List<Planet>();

        return root.results.Select(
            planetDto => (Planet)planetDto);
    }
}