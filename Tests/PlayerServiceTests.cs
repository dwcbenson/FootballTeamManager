using FluentAssertions;
using FootballTeamManager.Data;
using FootballTeamManager.Models;
using FootballTeamManager.Models.ApiModels;
using FootballTeamManager.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FootballTeamManager.Tests
{
    public class PlayerServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PlayerService _playerService;

        public PlayerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _playerService = new PlayerService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllPlayersAsync_ReturnsAllPlayers()
        {
            var player = new Player
            {
                PlayerName = "Test Player",
                Position = Position.Forward,
                JerseyNumber = 10,
                GoalsScored = 5,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var result = await _playerService.GetAllPlayersAsync();

            result.Should().HaveCount(1);
            result.First().PlayerName.Should().Be("Test Player");
        }

        [Fact]
        public async Task GetPlayerByIdAsync_ReturnsCorrectPlayer()
        {
            var player = new Player
            {
                PlayerName = "Test Player",
                Position = Position.Midfielder,
                JerseyNumber = 8,
                GoalsScored = 3,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var result = await _playerService.GetPlayerByIdAsync(player.PlayerId);

            result.Should().NotBeNull();
            result.PlayerName.Should().Be("Test Player");
        }

        [Fact]
        public async Task CreatePlayerAsync_CreatesPlayer()
        {
            var request = new CreatePlayerRequest
            {
                PlayerName = "New Player",
                Position = "Forward",
                JerseyNumber = 9,
                GoalsScored = 0
            };

            var result = await _playerService.CreatePlayerAsync(request);

            result.PlayerName.Should().Be("New Player");
            result.PlayerId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreatePlayerAsync_DuplicateJerseyNumber_ThrowsException()
        {
            var existing = new Player
            {
                PlayerName = "Existing Player",
                Position = Position.Goalkeeper,
                JerseyNumber = 1,
                GoalsScored = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Players.Add(existing);
            await _context.SaveChangesAsync();

            var request = new CreatePlayerRequest
            {
                PlayerName = "New Player",
                Position = "Forward",
                JerseyNumber = 1,
                GoalsScored = 0
            };

            var act = async () => await _playerService.CreatePlayerAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task UpdatePlayerAsync_UpdatesPlayer()
        {
            var player = new Player
            {
                PlayerName = "Old Name",
                Position = Position.Defender,
                JerseyNumber = 5,
                GoalsScored = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var request = new PatchPlayerRequest
            {
                PlayerName = "New Name",
                GoalsScored = 3,
                JerseyNumber = 10,
                Position = Position.Forward.ToString()
            };

            var result = await _playerService.UpdatePlayerAsync(player.PlayerId, request);

            result.Should().BeTrue();

            var updated = await _context.Players.FindAsync(player.PlayerId);
            updated.PlayerName.Should().Be("New Name");
            updated.GoalsScored.Should().Be(3);
            updated.JerseyNumber.Should().Be(10);
            updated.Position.Should().Be(Position.Forward);
        }

        [Fact]
        public async Task DeletePlayerAsync_DeletesPlayer()
        {
            var player = new Player
            {
                PlayerName = "Test Player",
                Position = Position.Goalkeeper,
                JerseyNumber = 1,
                GoalsScored = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var result = await _playerService.DeletePlayerAsync(player.PlayerId);

            result.Should().BeTrue();

            var deleted = await _context.Players.FindAsync(player.PlayerId);
            deleted.Should().BeNull();
        }
    }
}