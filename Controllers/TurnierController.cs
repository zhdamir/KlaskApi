using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;
using KlaskApi.Services;

namespace KlaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnierController : ControllerBase
    {
        private readonly TurnierContext _context;

        public TurnierController(TurnierContext context)
        {
            _context = context;
        }

        [HttpGet("gruppenrundenDetails")]
        public async Task<ActionResult<object>> GruppenrundenDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }




                // Get groups, participants, and games for the active turnier
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                       .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde"))
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        [HttpGet("currentTurnierDetails")]
        public async Task<ActionResult<object>> CurrentTurnierDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Get groups and participants for the active turnier
                var turnierDetails = await _context.Gruppen
                    .Join(_context.TurniereTeilnehmer,
                        g => g.GruppeId,
                        tt => tt.GruppeId,
                        (g, tt) => new { Gruppe = g, TurnierTeilnehmer = tt })
                    .Join(_context.Teilnehmer,
                        j => j.TurnierTeilnehmer.TeilnehmerId,
                        t => t.TeilnehmerId,
                        (j, t) => new { j.Gruppe, j.TurnierTeilnehmer, Teilnehmer = t })
                    .Where(j => j.Gruppe.TurnierId == activeTurnier.Id)
                    .Select(j => new
                    {
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        Teilnehmer = new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            // Add other properties as needed
                        }
                    })
                    .GroupBy(j => new { j.GruppeId, j.Gruppenname })
                    .Select(g => new
                    {
                        GruppeId = g.Key.GruppeId,
                        Gruppenname = g.Key.Gruppenname,
                        Teilnehmer = g.Select(j => j.Teilnehmer).ToList()
                    })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        /*staring the Turnier*/
        [HttpPost("startTurnier")]
        //[Route("start")]
        public async Task<IActionResult> StartTurnier([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {
                // Log or return a more specific error response for an invalid turnierId
                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                var groupingService = new TeilnehmerGroupingService(_context);
                var createdGroups = await groupingService.GroupTeilnehmer(turnierId);

                if (createdGroups == null)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Grouping failed. Check your data.");
                }

                // Return success response or createdGroups if needed
                return Ok(createdGroups);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        // GET: api/Turnier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Turnier>>> GetTurniere()
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            return await _context.Turniere.ToListAsync();
        }

        // GET: api/Turnier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Turnier>> GetTurnier(long id)
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            var turnier = await _context.Turniere.FindAsync(id);

            if (turnier == null)
            {
                return NotFound();
            }

            return turnier;
        }

        // PUT: api/Turnier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurnier(long id, Turnier turnier)
        {
            if (id != turnier.Id)
            {
                return BadRequest();
            }

            // Convert DateTime values to UTC
            turnier.StartDatum = turnier.StartDatum.ToUniversalTime();
            turnier.EndDatum = turnier.EndDatum.ToUniversalTime();

            _context.Entry(turnier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnierExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Turnier
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Turnier>> PostTurnier(Turnier turnier)
        {
            if (_context.Turniere == null)
            {
                return Problem("Entity set 'TurnierContext.Turniere'  is null.");
            }

            // Convert DateTime values to UTC
            turnier.StartDatum = turnier.StartDatum.ToUniversalTime();
            turnier.EndDatum = turnier.EndDatum.ToUniversalTime();

            _context.Turniere.Add(turnier);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTurnier", new { id = turnier.Id }, turnier);
        }

        // DELETE: api/Turnier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurnier(long id)
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            var turnier = await _context.Turniere.FindAsync(id);
            if (turnier == null)
            {
                return NotFound();
            }

            _context.Turniere.Remove(turnier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TurnierExists(long id)
        {
            return (_context.Turniere?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
