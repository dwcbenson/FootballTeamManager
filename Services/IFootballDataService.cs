using FootballTeamManager.Models.FootballData;

namespace FootballTeamManager.Services
{
    public interface IFootballDataService
    {
        Task<List<Match>> GetArsenalRecentResultsAsync(int count = 10);
        Task<List<Match>> GetArsenalUpcomingFixturesAsync(int count = 5);
    }
}
