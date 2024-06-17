using StartWars.DataAccess;
using StartWars.UserInteraction;

namespace StartWars.App;

public class StartWarsPlanetsApp
{

    private readonly IPlanetsReader _planetsReader;
    private readonly IPlanetsStatisticsAnalyzer _planetsStatisticsAnalyzer;
    private readonly IPlanetsStatsUserInteractor _planetsStatsUserInteractor;

    public StartWarsPlanetsApp(
        IPlanetsReader planetsReader, 
        IPlanetsStatisticsAnalyzer planetsStatisticsAnalyzer, 
        IPlanetsStatsUserInteractor planetsStatsUserInteractor)
    {
        _planetsReader = planetsReader;
        _planetsStatisticsAnalyzer = planetsStatisticsAnalyzer;
        _planetsStatsUserInteractor = planetsStatsUserInteractor;
    }

    public async Task Run()
    {
        var planets = await _planetsReader.Read();

        _planetsStatsUserInteractor.Show(planets);

        _planetsStatisticsAnalyzer.Analyze(planets);
    }
}