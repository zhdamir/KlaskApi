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
    public class SpielController : ControllerBase
    {
        private readonly TurnierContext _context;

        public SpielController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/Spiel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Spiel>>> GetSpiele()
        {
          if (_context.Spiele == null)
          {
              return NotFound();
          }
            return await _context.Spiele.ToListAsync();
        }

        // GET: api/Spiel/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Spiel>> GetSpiel(long id)
        {
          if (_context.Spiele == null)
          {
              return NotFound();
          }
            var spiel = await _context.Spiele.FindAsync(id);

            if (spiel == null)
            {
                return NotFound();
            }

            return spiel;
        }

        // PUT: api/Spiel/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpiel(long id, Spiel spiel)
        {
            if (id != spiel.SpielId)
            {
                return BadRequest();
            }

            _context.Entry(spiel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpielExists(id))
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

        // POST: api/Spiel
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Spiel>> PostSpiel(Spiel spiel)
        {
          if (_context.Spiele == null)
          {
              return Problem("Entity set 'TurnierContext.Spiele'  is null.");
          }
            _context.Spiele.Add(spiel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpiel", new { id = spiel.SpielId }, spiel);
        }

        // DELETE: api/Spiel/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpiel(long id)
        {
            if (_context.Spiele == null)
            {
                return NotFound();
            }
            var spiel = await _context.Spiele.FindAsync(id);
            if (spiel == null)
            {
                return NotFound();
            }

            _context.Spiele.Remove(spiel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpielExists(long id)
        {
            return (_context.Spiele?.Any(e => e.SpielId == id)).GetValueOrDefault();
        }
    }
}
