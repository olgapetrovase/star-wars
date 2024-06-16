
using StartWars.DTO;
using StartWars.MockApiDataAccess;
using System.Text.Json;

try
{
    await new StartWarsPlanetsApp(
        new PlanetsFromApiReader(
            new ApiDataReader(),
            new MockStarWarsApiDataReader()),
        new PlanetsStatisticsAnalyzer())
        .Run();
}
catch (Exception ex)
{
    Console.WriteLine("An error occured. " +
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
    public PlanetsFromApiReader(
        IApiDataReader apiDataReader, 
        IApiDataReader mockApiDataReader)
    {
        _apiDataReader = apiDataReader;
        _mockApiDataReader = mockApiDataReader;
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
            Console.WriteLine("API request was unsuccessful. " +
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

        //foreach (var planetDto in root.results)
        //{
        //    Planet planet = (Planet)planetDto;
        //    planets.Add(planet);
        //}
        //return planets;
    }
}

public interface IPlanetsStatisticsAnalyzer
{
    void Analyze(IEnumerable<Planet> planets);
}

public class PlanetsStatisticsAnalyzer : IPlanetsStatisticsAnalyzer
{
    public void Analyze(IEnumerable<Planet> planets)
    {
        var propertyNamesToSelectorMapping =
        new Dictionary<string, Func<Planet, int?>>
        {
            ["population"] = planet => planet.Population,
            ["diameter"] = planet => planet.Diameter,
            ["surface water"] = planet => planet.SurfaceWater,
        };

        Console.WriteLine();
        Console.WriteLine("The statistics of which property would you " +
            "like to see?");
        Console.WriteLine(string.Join(
            Environment.NewLine, propertyNamesToSelectorMapping.Keys));

        var userChoice = Console.ReadLine();

        if (userChoice is null ||
            !propertyNamesToSelectorMapping.ContainsKey(userChoice))
        {
            Console.WriteLine("Invalid choice.");
        }
        else
        {
            ShowStatistic(
                planets,
                userChoice,
                propertyNamesToSelectorMapping[userChoice]);
        }
    }

    private static void ShowStatistic(IEnumerable<Planet> planets,
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

    private static void ShowStatistic(
        string descriptor,
        Planet selectedPlanet,
        Func<Planet, int?> propertySelector,
        string propertyName)
    {
        Console.WriteLine($"{descriptor} {propertyName} is: " +
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

public interface IUserInteractor
{
    void ShowMessage(string message);
    string? ReadFromUser();
}

public class ConsoleUserInteractor : IUserInteractor
{
    public string? ReadFromUser()
    {
        return Console.ReadLine();
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }
}
