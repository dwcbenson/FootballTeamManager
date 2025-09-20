using FootballTeamManager.Models;
using FootballTeamManager.Models.ApiModels;
using FootballTeamManager.Models.FootballData;
using FootballTeamManager.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FootballTeamManager.Controllers
{
    public class PlayersController : Controller
    {
        private readonly IFootballDataService _footballDataService;
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService, IFootballDataService footballDataService)
        {
            _playerService = playerService;
            _footballDataService = footballDataService;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region API Endpoints

        [HttpGet]
        [Route("api/players")]
        public async Task<ActionResult<IEnumerable<PlayerApiModel>>> GetAllPlayers()
        {
            try
            {
                var players = await _playerService.GetAllPlayersAsync();

                if (!players.Any())
                    return NoContent();

                return Ok(players);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving players.",
                    details = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/players/{id}")]
        public async Task<ActionResult<PlayerApiModel>> GetPlayer(int id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);

                if (player == null)
                    return NotFound(new { message = $"Player with ID {id} not found." });

                return Ok(player);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the player.",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("api/players")]
        public async Task<ActionResult<PlayerApiModel>> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var player = await _playerService.CreatePlayerAsync(request);
                return CreatedAtAction(nameof(GetPlayer), new { id = player.PlayerId }, player);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the player.",
                    details = ex.Message
                });
            }
        }

        [HttpPatch]
        [Route("api/players/{id}")]
        public async Task<IActionResult> PatchPlayerApi(int id, [FromBody] PatchPlayerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var wasUpdated = await _playerService.UpdatePlayerAsync(id, request);

                if (wasUpdated)
                    return Ok(new { message = "Player updated successfully." });
                else
                    return NoContent(); // No changes were made
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the player.",
                    details = ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("api/players/{id}")]
        public async Task<IActionResult> DeletePlayerApi(int id)
        {
            try
            {
                var wasDeleted = await _playerService.DeletePlayerAsync(id);

                if (!wasDeleted)
                    return NotFound(new { message = $"Player with ID {id} not found." });

                return Ok(new { message = "Player deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the player.",
                    details = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/arsenal/recent-results")]
        public async Task<ActionResult<IEnumerable<Match>>> GetArsenalRecentResults()
        {
            try
            {
                var results = await _footballDataService.GetArsenalRecentResultsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving Arsenal's recent results.", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/arsenal/upcoming-fixtures")]
        public async Task<ActionResult<IEnumerable<Match>>> GetArsenalUpcomingFixtures()
        {
            try
            {
                var fixtures = await _footballDataService.GetArsenalUpcomingFixturesAsync();
                return Ok(fixtures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving Arsenal's upcoming fixtures.", details = ex.Message });
            }
        }
        #endregion

        #region MVC Actions
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);

                if (player == null)
                    return NotFound();

                var patchRequest = new PatchPlayerRequest
                {
                    PlayerName = player.PlayerName,
                    Position = player.Position,
                    JerseyNumber = player.JerseyNumber,
                    GoalsScored = player.GoalsScored
                };

                ViewBag.PlayerId = id;
                return View(patchRequest);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        #endregion
    }
}
