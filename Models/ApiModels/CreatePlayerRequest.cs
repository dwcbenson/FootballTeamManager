using System.ComponentModel.DataAnnotations;

namespace FootballTeamManager.Models.ApiModels
{
    public class CreatePlayerRequest
    {
        [Required(ErrorMessage = "Player name is required.")]
        [StringLength(100, ErrorMessage = "Player name cannot exceed 100 characters.")]
        [Display(Name = "Player Name")]
        public string PlayerName { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        [StringLength(50)]
        public string Position { get; set; }

        [Required(ErrorMessage = "Jersey number is required.")]
        [Range(1, 99, ErrorMessage = "Jersey number must be between 1 and 99.")]
        [Display(Name = "Jersey Number")]
        public int JerseyNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Goals scored must be 0 or more.")]
        [Display(Name = "Goals Scored")]
        public int GoalsScored { get; set; } = 0;
    }
}
