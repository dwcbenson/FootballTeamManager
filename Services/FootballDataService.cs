using FootballTeamManager.Models.FootballData;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FootballTeamManager.Services
{
    public class FootballDataService : IFootballDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const int ARSENAL_TEAM_ID = 57;

        public FootballDataService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", configuration["FootballDataApi:ApiKey"]);
        }

        public async Task<List<Match>> GetArsenalRecentResultsAsync(int count = 10)
        {
            var cacheKey = $"arsenal_results_{count}";

            if (_cache.TryGetValue(cacheKey, out List<Match> cached))
                return cached;

            var matches = await GetMatchesAsync("FINISHED", -90, 0, count, m => m.OrderByDescending(x => x.UtcDate));

            _cache.Set(cacheKey, matches, TimeSpan.FromMinutes(30));
            return matches;
        }

        public async Task<List<Match>> GetArsenalUpcomingFixturesAsync(int count = 5)
        {
            var cacheKey = $"arsenal_fixtures_{count}";

            if (_cache.TryGetValue(cacheKey, out List<Match> cached))
                return cached;

            var matches = await GetMatchesAsync("SCHEDULED", 0, 90, count, m => m.OrderBy(x => x.UtcDate));

            _cache.Set(cacheKey, matches, TimeSpan.FromHours(1));
            return matches;
        }

        private async Task<List<Match>> GetMatchesAsync(string status, int fromDays, int toDays, int count,
    Func<IEnumerable<Match>, IOrderedEnumerable<Match>> orderBy)
        {
            try
            {
                var today = DateTime.UtcNow;
                var fromDate = today.AddDays(fromDays).ToString("yyyy-MM-dd");
                var toDate = today.AddDays(toDays).ToString("yyyy-MM-dd");
                var url = $"https://api.football-data.org/v4/teams/{ARSENAL_TEAM_ID}/matches?dateFrom={fromDate}&dateTo={toDate}&status={status}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<Match>();

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<FootballDataResponse>(json, options);

                var apiMatches = apiResponse?.Matches ?? new List<ApiMatch>();

                var mapped = apiMatches.Select(m => new Match
                {
                    Id = m.Id,
                    UtcDate = m.UtcDate,
                    HomeTeamName = m.HomeTeam?.Name,
                    AwayTeamName = m.AwayTeam?.Name,
                    HomeScore = m.Score?.FullTime?.Home,
                    AwayScore = m.Score?.FullTime?.Away
                });

                return orderBy(mapped).Take(count).ToList();
            }
            catch
            {
                return new List<Match>();
            }
        }

    }
}