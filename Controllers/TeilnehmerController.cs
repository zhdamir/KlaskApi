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
    public class TeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        public TeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/Teilnehmer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teilnehmer>>> GetTeilnehmer()
        {
          if (_context.Teilnehmer == null)
          {
              return NotFound();
          }
            return await _context.Teilnehmer.ToListAsync();
        }

        // GET: api/Teilnehmer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Teilnehmer>> GetTeilnehmer(long id)
        {
          if (_context.Teilnehmer == null)
          {
              return NotFound();
          }
            var teilnehmer = await _context.Teilnehmer.FindAsync(id);

            if (teilnehmer == null)
            {
                return NotFound();
            }

            return teilnehmer;
        }

        // PUT: api/Teilnehmer/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeilnehmer(long id, Teilnehmer teilnehmer)
        {
            if (id != teilnehmer.TeilnehmerId)
            {
                return BadRequest();
            }

            _context.Entry(teilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeilnehmerExists(id))
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

        // POST: api/Teilnehmer
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Teilnehmer>> PostTeilnehmer(Teilnehmer teilnehmer)
        {
          if (_context.Teilnehmer == null)
          {
              return Problem("Entity set 'TurnierContext.Teilnehmer'  is null.");
          }
            _context.Teilnehmer.Add(teilnehmer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeilnehmer", new { id = teilnehmer.TeilnehmerId }, teilnehmer);
        }

        // DELETE: api/Teilnehmer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeilnehmer(long id)
        {
            if (_context.Teilnehmer == null)
            {
                return NotFound();
            }
            var teilnehmer = await _context.Teilnehmer.FindAsync(id);
            if (teilnehmer == null)
            {
                return NotFound();
            }

            _context.Teilnehmer.Remove(teilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeilnehmerExists(long id)
        {
            return (_context.Teilnehmer?.Any(e => e.TeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
