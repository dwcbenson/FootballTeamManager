using Azure.Core;
using FootballTeamManager.Data;
using FootballTeamManager.Models;
using FootballTeamManager.Models.ApiModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballTeamManager.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region ApiEndpoints

        // GET: /api/players
        [HttpGet]
        [Route("api/players")]
        public async Task<ActionResult<IEnumerable<PlayerApiModel>>> GetAllPlayers()
        {
            try
            {
                // Get all the players from the database
                var players = await _context.Players.Select(p => new PlayerApiModel
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    Position = p.Position.ToString(),
                    JerseyNumber = p.JerseyNumber,
                    GoalsScored = p.GoalsScored
                }).ToListAsync();

                // Check if there are players to return
                if (players.Count > 0)
                {
                    return Ok(players); // Return the full list
                }
                else
                {
                    // Unlikely to happen, but better than returning an empty list
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                // Something went wrong server-side
                return StatusCode(500, new { message = "An error occurred while retrieving players.", details = ex.Message });
            }
        }

        // GET: /api/players/5
        [HttpGet]
        [Route("api/players/{id}")]
        public async Task<ActionResult<PlayerApiModel>> GetPlayer(int id)
        {
            try
            {
                // Find the player by ID
                var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == id);

                if (player == null)
                {
                    return NotFound(new { message = $"Player with ID {id} not found." });
                }

                // Map the database player to API model
                var apiModel = new PlayerApiModel
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.PlayerName,
                    Position = player.Position.ToString(),
                    JerseyNumber = player.JerseyNumber,
                    GoalsScored = player.GoalsScored
                };

                return Ok(apiModel);
            }
            catch (Exception ex)
            {
                // Something went wrong while retrieving this specific player
                return StatusCode(500, new { message = "An error occurred while retrieving the player.", details = ex.Message });
            }
        }

        // POST: /api/players
        [HttpPost]
        [Route("api/players")]
        public async Task<ActionResult<Player>> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Model validation failed
                    return BadRequest(ModelState);
                }

                // Check if the jersey number is already taken
                if (await IsJerseyNumberTaken(request.JerseyNumber))
                {
                    return Conflict(new { message = "A player with this jersey number already exists." });
                }

                // Ensure the position is valid
                if (!Enum.TryParse<Position>(request.Position, true, out var positionEnum))
                {
                    return BadRequest(new { message = $"That is not a valid position. Please select one of the following: Goalkeeper, Defender, Midfielder, Forward" });
                }

                // Create the new player entity
                var player = new Player
                {
                    PlayerName = request.PlayerName,
                    Position = positionEnum,
                    JerseyNumber = request.JerseyNumber,
                    GoalsScored = request.GoalsScored,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Players.Add(player); // Add to the DB context
                await _context.SaveChangesAsync(); // Commit to the database

                // Prepare response model to return
                var responseModel = new PlayerApiModel
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.PlayerName,
                    Position = player.Position.ToString(),
                    JerseyNumber = player.JerseyNumber,
                    GoalsScored = player.GoalsScored
                };

                return CreatedAtAction(nameof(GetPlayer), new { id = player.PlayerId }, responseModel);
            }
            catch (Exception ex)
            {
                // Something went wrong server-side
                return StatusCode(500, new { message = "An error occurred while creating the player.", details = ex.Message });
            }
        }

        // PATCH: /api/players/1
        [HttpPatch]
        [Route("api/players/{id}")]
        public async Task<IActionResult> PatchPlayerApi(int id, [FromBody] PatchPlayerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Validation failed
                }

                // Retrieve the existing player from DB
                var existingPlayer = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == id);

                if (existingPlayer == null)
                {
                    return NotFound(new { message = $"Player with ID {id} not found." });
                }

                bool isUpdated = false;

                // Update PlayerName if it's different
                if (!string.IsNullOrEmpty(request.PlayerName) && request.PlayerName != existingPlayer.PlayerName)
                {
                    existingPlayer.PlayerName = request.PlayerName;
                    isUpdated = true;
                }

                // Update Position if it's different
                if (!string.IsNullOrEmpty(request.Position))
                {
                    if (!Enum.TryParse<Position>(request.Position, true, out var positionEnum))
                    {
                        return BadRequest(new { message = $"That is not a valid position. Please select one of the following: Goalkeeper, Defender, Midfielder, Forward" });
                    }

                    if (existingPlayer.Position != positionEnum)
                    {
                        existingPlayer.Position = positionEnum;
                        isUpdated = true;
                    }
                }

                // Update JerseyNumber if it's different
                if (request.JerseyNumber.HasValue && request.JerseyNumber.Value != existingPlayer.JerseyNumber)
                {
                    // Check if the jersey number is already taken
                    if (await IsJerseyNumberTaken(request.JerseyNumber.Value))
                    {
                        return Conflict(new { message = "A player with this jersey number already exists." });
                    }

                    existingPlayer.JerseyNumber = request.JerseyNumber.Value;
                    isUpdated = true;
                }

                // Update GoalsScored if it's different
                if (request.GoalsScored.HasValue && request.GoalsScored.Value != existingPlayer.GoalsScored)
                {
                    existingPlayer.GoalsScored = request.GoalsScored.Value;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Player values updated successfully." });
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                // Catch any DB errors
                return StatusCode(500, new { message = "An error occurred while updating the player.", details = ex.Message });
            }
        }


        // DELETE: /api/players/1
        [HttpDelete]
        [Route("api/players/{id}")]
        public async Task<IActionResult> DeletePlayerApi(int id)
        {
            try
            {
                // Find the player to delete
                var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == id);

                if (player == null)
                {
                    return NotFound(new { message = $"Player with ID {id} not found." });
                }

                _context.Players.Remove(player); // Remove from DB
                await _context.SaveChangesAsync(); // Commit deletion

                return Ok(new { message = "Player deleted successfully." });
            }
            catch (Exception ex)
            {
                // Server-side error
                return StatusCode(500, new { message = "An error occurred while deleting the player.", details = ex.Message });
            }
        }


        // Utility method to check for duplicate jersey numbers. There is a sql constraint but this is for redundancy
        private async Task<bool> IsJerseyNumberTaken(int jerseyNumber)
        {
            return await _context.Players
            .Where(p => p.JerseyNumber == jerseyNumber)
            .AnyAsync();
        }

        #endregion

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
            var player = await _context.Players
                .Where(p => p.PlayerId == id)
                .Select(p => new PatchPlayerRequest
                {
                    PlayerName = p.PlayerName,
                    Position = p.Position.ToString(),
                    JerseyNumber = p.JerseyNumber,
                    GoalsScored = p.GoalsScored
                })
                .FirstOrDefaultAsync();

            if (player == null)
                return NotFound();

            ViewBag.PlayerId = id;
            return View(player);
        }


    }
}
