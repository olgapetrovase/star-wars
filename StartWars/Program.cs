
using StartWars.DTO;
using StartWars.MockApiDataAccess;
using System.Text.Json;

try
{
    await new StartWarsPlanetsApp(
        new PlanetsFromApiReader(
            new ApiDataReader(),
            new MockStarWarsApiDataReader(),
            new ConsoleUserInteractor()),
        new PlanetsStatisticsAnalyzer(
            new PlanetsStatsUserInteractor(
                new ConsoleUserInteractor())))
        .Run();
}
catch (Exception ex)
{
    new ConsoleUserInteractor().ShowMessage("An error occured. " +
        "Exception message: " + ex.Message);
}

public class StartWarsPlanetsApp
{
    
    private readonly IPlanetsReader _planetsReader;
    private readonly IPlanetsStatisticsAnalyzer _planetsStatisticsAnalyzer;

    public StartWarsPlanetsApp(IPlanetsReader planetsReader, IPlanetsStatisticsAnalyzer planetsStatisticsAnalyzer)
    {
        _planetsReader = planetsReader;
        _planetsStatisticsAnalyzer = planetsStatisticsAnalyzer;
    }

    public async Task Run()
    {
        var planets = await _planetsReader.Read();

        foreach (var planet in planets)
        {
            Console.WriteLine(planet);
        }

        _planetsStatisticsAnalyzer.Analyze(planets);
    }  
}

public interface IPlanetsReader
{
    public Task<IEnumerable<Planet>> Read();
}

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

public interface IPlanetsStatisticsAnalyzer
{
    void Analyze(IEnumerable<Planet> planets);
}

public class PlanetsStatisticsAnalyzer : IPlanetsStatisticsAnalyzer
{
    private readonly IPlanetsStatsUserInteractor _planetsStatsUserInteractor;

    public PlanetsStatisticsAnalyzer(IPlanetsStatsUserInteractor planetsStatsUserInteractor)
    {
        _planetsStatsUserInteractor = planetsStatsUserInteractor;
    }
    public void Analyze(IEnumerable<Planet> planets)
    {
        var propertyNamesToSelectorMapping =
        new Dictionary<string, Func<Planet, int?>>
        {
            ["population"] = planet => planet.Population,
            ["diameter"] = planet => planet.Diameter,
            ["surface water"] = planet => planet.SurfaceWater,
        };

        var userChoice = _planetsStatsUserInteractor.ChooseStatisticsToBeShown(
            propertyNamesToSelectorMapping.Keys);

        if (userChoice is null ||
            !propertyNamesToSelectorMapping.ContainsKey(userChoice))
        {
            _planetsStatsUserInteractor.ShowMessage("Invalid choice.");
        }
        else
        {
            ShowStatistic(
                planets,
                userChoice,
                propertyNamesToSelectorMapping[userChoice]);
        }
    }

    private void ShowStatistic(IEnumerable<Planet> planets,
        string propertyName,
        Func<Planet, int?> propertySelector)
    {
        ShowStatistic(
            "Max",
            planets.MaxBy(propertySelector),
            propertySelector,
            propertyName);

        ShowStatistic(
            "Min",
            planets.MinBy(propertySelector),
            propertySelector,
            propertyName);
    }

    private void ShowStatistic(
        string descriptor,
        Planet selectedPlanet,
        Func<Planet, int?> propertySelector,
        string propertyName)
    {
        _planetsStatsUserInteractor.ShowMessage($"{descriptor} {propertyName} is: " +
            $"{propertySelector(selectedPlanet)}" +
            $"(planet: {selectedPlanet.Name})");
    }
}

public readonly record struct Planet
{
    public string Name { get; }
    public int Diameter { get; }
    public int? SurfaceWater { get; }
    public int? Population { get; }

    public Planet(string name, 
        int diameter, 
        int? surfaceWater, 
        int? population)
    {
        if (name is null)
        {
            throw new ArgumentNullException($"{nameof(name)}");
        }
        Name = name;
        Diameter = diameter;
        SurfaceWater = surfaceWater;
        Population = population;
    }

    public static explicit operator Planet(Result planetDto)
    {
        var name = planetDto.name;
        var diameter = int.Parse(planetDto.diameter);

        int? population = planetDto.population.ToIntOrNull();
        int? surfaceWater = planetDto.surface_water.ToIntOrNull();

        return new Planet(name, diameter, surfaceWater, population);
    }    
}

public static class StringExtensions
{    public static int? ToIntOrNull(this string input)
    {
        return int.TryParse(input, out int resultParsed) ? 
            resultParsed :
            null;
    }
}

public class PlanetsStatsUserInteractor : IPlanetsStatsUserInteractor
{
    private readonly IUserInteractor _userInteractor;
    public PlanetsStatsUserInteractor(IUserInteractor userInteractor)
    {
        _userInteractor = userInteractor;
    }
    public string? ChooseStatisticsToBeShown(
        IEnumerable<string> propertiesThatCanBeChosen)
    {
        _userInteractor.ShowMessage(Environment.NewLine);
        _userInteractor.ShowMessage(
            "The statistics of which property would you " +
            "like to see?");
        _userInteractor.ShowMessage(
            string.Join(
                Environment.NewLine,
                propertiesThatCanBeChosen));

        return _userInteractor.ReadFromUser();
    }

    public void Show(IEnumerable<Planet> planets)
    {
        foreach (var planet in planets)
        {
            Console.WriteLine(planet);
        }
    }

    public void ShowMessage(string message)
    {
        _userInteractor.ShowMessage(message);
    }
}
