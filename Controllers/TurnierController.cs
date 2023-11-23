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
    public class TurnierController : ControllerBase
    {
        private readonly TurnierContext _context;

        public TurnierController(TurnierContext context)
        {
            _context = context;
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
