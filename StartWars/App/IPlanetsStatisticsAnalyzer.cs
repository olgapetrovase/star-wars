using StartWars.Model;

namespace StartWars.App;

public interface IPlanetsStatisticsAnalyzer
{
    void Analyze(IEnumerable<Planet> planets);
}