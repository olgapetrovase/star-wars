using StartWars.Model;

namespace StartWars.DataAccess;

public interface IPlanetsReader
{
    public Task<IEnumerable<Planet>> Read();
}