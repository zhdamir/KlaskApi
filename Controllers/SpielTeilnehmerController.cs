using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;

namespace KlaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpielTeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        public SpielTeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        // POST: api/Turnier/UpdatePunkte/5
        [HttpPost("UpdatePunkte/{spielTeilnehmerId}")]
        public async Task<IActionResult> UpdatePunkte(long spielTeilnehmerId, [FromBody] int neuePunkte)
        {
            try
            {
                if (spielTeilnehmerId <= 0)
                {
                    return BadRequest("Invalid spielTeilnehmerId. SpielTeilnehmerId must be greater than 0.");
                }

                var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(spielTeilnehmerId);

                if (spielTeilnehmer == null)
                {
                    return NotFound("SpielTeilnehmer not found.");
                }

                // Update the Punkte
                spielTeilnehmer.Punkte = neuePunkte;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Return success response or updated spielTeilnehmer if needed
                return Ok(spielTeilnehmer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePunkte: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/SpielTeilnehmer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpielTeilnehmer>>> GetSpieleTeilnehmer()
        {
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }
            return await _context.SpieleTeilnehmer.ToListAsync();
        }

        // GET: api/SpielTeilnehmer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpielTeilnehmer>> GetSpielTeilnehmer(long id)
        {
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }
            var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(id);

            if (spielTeilnehmer == null)
            {
                return NotFound();
            }

            return spielTeilnehmer;
        }

        // PUT: api/SpielTeilnehmer/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpielTeilnehmer(long id, SpielTeilnehmer spielTeilnehmer)
        {
            if (id != spielTeilnehmer.SpielTeilnehmerId)
            {
                return BadRequest();
            }

            _context.Entry(spielTeilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpielTeilnehmerExists(id))
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

        // POST: api/SpielTeilnehmer
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SpielTeilnehmer>> PostSpielTeilnehmer(SpielTeilnehmer spielTeilnehmer)
        {
            if (_context.SpieleTeilnehmer == null)
            {
                return Problem("Entity set 'TurnierContext.SpieleTeilnehmer'  is null.");
            }
            _context.SpieleTeilnehmer.Add(spielTeilnehmer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpielTeilnehmer", new { id = spielTeilnehmer.SpielTeilnehmerId }, spielTeilnehmer);
        }

        // DELETE: api/SpielTeilnehmer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpielTeilnehmer(long id)
        {
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }
            var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(id);
            if (spielTeilnehmer == null)
            {
                return NotFound();
            }

            _context.SpieleTeilnehmer.Remove(spielTeilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpielTeilnehmerExists(long id)
        {
            return (_context.SpieleTeilnehmer?.Any(e => e.SpielTeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
