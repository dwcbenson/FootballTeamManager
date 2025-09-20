namespace FootballTeamManager.Models.FootballData
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime UtcDate { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
    }
}
