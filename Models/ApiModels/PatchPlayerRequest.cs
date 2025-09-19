using System.ComponentModel.DataAnnotations;

namespace FootballTeamManager.Models.ApiModels
{
    public class PatchPlayerRequest
    {
        [StringLength(100)]
        public string? PlayerName { get; set; }

        public string? Position { get; set; }

        [Range(1, 99)]
        public int? JerseyNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int? GoalsScored { get; set; }
    }
}
