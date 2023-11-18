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
    public class TurnierTeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        public TurnierTeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/TurnierTeilnehmer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnierTeilnehmer>>> GetTurniereTeilnehmer()
        {
          if (_context.TurniereTeilnehmer == null)
          {
              return NotFound();
          }
            return await _context.TurniereTeilnehmer.ToListAsync();
        }

        // GET: api/TurnierTeilnehmer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TurnierTeilnehmer>> GetTurnierTeilnehmer(long id)
        {
          if (_context.TurniereTeilnehmer == null)
          {
              return NotFound();
          }
            var turnierTeilnehmer = await _context.TurniereTeilnehmer.FindAsync(id);

            if (turnierTeilnehmer == null)
            {
                return NotFound();
            }

            return turnierTeilnehmer;
        }

        // PUT: api/TurnierTeilnehmer/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurnierTeilnehmer(long id, TurnierTeilnehmer turnierTeilnehmer)
        {
            if (id != turnierTeilnehmer.TurnierTeilnehmerId)
            {
                return BadRequest();
            }

            _context.Entry(turnierTeilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnierTeilnehmerExists(id))
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

        // POST: api/TurnierTeilnehmer
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TurnierTeilnehmer>> PostTurnierTeilnehmer(TurnierTeilnehmer turnierTeilnehmer)
        {
          if (_context.TurniereTeilnehmer == null)
          {
              return Problem("Entity set 'TurnierContext.TurniereTeilnehmer'  is null.");
          }
            _context.TurniereTeilnehmer.Add(turnierTeilnehmer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTurnierTeilnehmer", new { id = turnierTeilnehmer.TurnierTeilnehmerId }, turnierTeilnehmer);
        }

        // DELETE: api/TurnierTeilnehmer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurnierTeilnehmer(long id)
        {
            if (_context.TurniereTeilnehmer == null)
            {
                return NotFound();
            }
            var turnierTeilnehmer = await _context.TurniereTeilnehmer.FindAsync(id);
            if (turnierTeilnehmer == null)
            {
                return NotFound();
            }

            _context.TurniereTeilnehmer.Remove(turnierTeilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TurnierTeilnehmerExists(long id)
        {
            return (_context.TurniereTeilnehmer?.Any(e => e.TurnierTeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
