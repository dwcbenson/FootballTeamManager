using FootballTeamManager.Data;
using FootballTeamManager.Models;
using FootballTeamManager.Models.ApiModels;
using Microsoft.EntityFrameworkCore;

namespace FootballTeamManager.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlayerApiModel>> GetAllPlayersAsync()
        {
            return await _context.Players
                .Select(p => new PlayerApiModel
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    Position = p.Position.ToString(),
                    JerseyNumber = p.JerseyNumber,
                    GoalsScored = p.GoalsScored
                })
                .ToListAsync();
        }

        public async Task<PlayerApiModel?> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(x => x.PlayerId == id);

            if (player == null) return null;

            return new PlayerApiModel
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                Position = player.Position.ToString(),
                JerseyNumber = player.JerseyNumber,
                GoalsScored = player.GoalsScored
            };
        }

        public async Task<PlayerApiModel> CreatePlayerAsync(CreatePlayerRequest request)
        {
            // Validate position enum
            if (!Enum.TryParse<Position>(request.Position, true, out var positionEnum))
            {
                throw new ArgumentException($"Invalid position: {request.Position}. Valid positions are: {string.Join(", ", Enum.GetNames<Position>())}");
            }

            // Check jersey number availability
            if (await IsJerseyNumberTakenAsync(request.JerseyNumber))
            {
                throw new InvalidOperationException($"Jersey number {request.JerseyNumber} is already taken.");
            }

            var player = new Player
            {
                PlayerName = request.PlayerName,
                Position = positionEnum,
                JerseyNumber = request.JerseyNumber,
                GoalsScored = request.GoalsScored,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return new PlayerApiModel
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                Position = player.Position.ToString(),
                JerseyNumber = player.JerseyNumber,
                GoalsScored = player.GoalsScored
            };
        }

        public async Task<bool> UpdatePlayerAsync(int id, PatchPlayerRequest request)
        {
            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(x => x.PlayerId == id);

            if (existingPlayer == null)
                throw new KeyNotFoundException($"Player with ID {id} not found.");

            bool isUpdated = false;

            // Update PlayerName if provided and different
            if (!string.IsNullOrEmpty(request.PlayerName) &&
                request.PlayerName != existingPlayer.PlayerName)
            {
                existingPlayer.PlayerName = request.PlayerName;
                isUpdated = true;
            }

            // Update Position if provided and valid
            if (!string.IsNullOrEmpty(request.Position))
            {
                if (!Enum.TryParse<Position>(request.Position, true, out var positionEnum))
                {
                    throw new ArgumentException($"Invalid position: {request.Position}. Valid positions are: {string.Join(", ", Enum.GetNames<Position>())}");
                }

                if (existingPlayer.Position != positionEnum)
                {
                    existingPlayer.Position = positionEnum;
                    isUpdated = true;
                }
            }

            // Update JerseyNumber if provided and different
            if (request.JerseyNumber.HasValue &&
                request.JerseyNumber.Value != existingPlayer.JerseyNumber)
            {
                if (await IsJerseyNumberTakenAsync(request.JerseyNumber.Value))
                {
                    throw new InvalidOperationException($"Jersey number {request.JerseyNumber.Value} is already taken.");
                }

                existingPlayer.JerseyNumber = request.JerseyNumber.Value;
                isUpdated = true;
            }

            // Update GoalsScored if provided and different
            if (request.GoalsScored.HasValue &&
                request.GoalsScored.Value != existingPlayer.GoalsScored)
            {
                existingPlayer.GoalsScored = request.GoalsScored.Value;
                isUpdated = true;
            }

            if (isUpdated)
            {
                existingPlayer.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return isUpdated;
        }

        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == id);

            if (player == null)
                return false;

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsJerseyNumberTakenAsync(int jerseyNumber)
        {
            return await _context.Players
            .Where(p => p.JerseyNumber == jerseyNumber)
            .AnyAsync();
        }
    }
}
