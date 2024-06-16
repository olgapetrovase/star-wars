using StartWars.Model;
using StartWars.UserInteraction;

namespace StartWars.App;

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