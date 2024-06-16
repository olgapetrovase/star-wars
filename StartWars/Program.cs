using StartWars;
using StartWars.App;
using StartWars.DataAccess;
using StartWars.MockApiDataAccess;
using StartWars.UserInteraction;

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
