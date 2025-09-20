using FootballTeamManager.Models.ApiModels;

namespace FootballTeamManager.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<PlayerApiModel>> GetAllPlayersAsync();
        Task<PlayerApiModel?> GetPlayerByIdAsync(int id);
        Task<PlayerApiModel> CreatePlayerAsync(CreatePlayerRequest request);
        Task<bool> UpdatePlayerAsync(int id, PatchPlayerRequest request);
        Task<bool> DeletePlayerAsync(int id);
        Task<bool> IsJerseyNumberTakenAsync(int jerseyNumber);
    }
}
