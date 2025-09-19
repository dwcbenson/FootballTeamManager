using System.ComponentModel.DataAnnotations;

namespace FootballTeamManager.Models.ApiModels
{
    public class CreatePlayerRequest
    {
        [Required]
        [StringLength(100)]
        public string PlayerName { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        [Range(1, 99)]
        public int JerseyNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int GoalsScored { get; set; }
    }
}
