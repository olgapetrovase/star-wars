using StartWars.DataAccess;

namespace StartWars.App;

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