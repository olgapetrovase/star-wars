namespace StartWars;

public interface IApiDataReader
{
    Task<string> Read(string baseAddress, string requestUrl);
}