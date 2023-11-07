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
    public class BereichController : ControllerBase
    {
        private readonly TurnierContext _context;

        public BereichController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/Bereich
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bereich>>> GetBereich()
        {
          if (_context.Bereich == null)
          {
              return NotFound();
          }
            return await _context.Bereich.ToListAsync();
        }

        // GET: api/Bereich/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bereich>> GetBereich(long id)
        {
          if (_context.Bereich == null)
          {
              return NotFound();
          }
            var bereich = await _context.Bereich.FindAsync(id);

            if (bereich == null)
            {
                return NotFound();
            }

            return bereich;
        }

        // PUT: api/Bereich/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBereich(long id, Bereich bereich)
        {
            if (id != bereich.BereichId)
            {
                return BadRequest();
            }

            _context.Entry(bereich).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BereichExists(id))
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

        // POST: api/Bereich
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bereich>> PostBereich(Bereich bereich)
        {
          if (_context.Bereich == null)
          {
              return Problem("Entity set 'TurnierContext.Bereich'  is null.");
          }
            _context.Bereich.Add(bereich);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBereich", new { id = bereich.BereichId }, bereich);
        }

        // DELETE: api/Bereich/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBereich(long id)
        {
            if (_context.Bereich == null)
            {
                return NotFound();
            }
            var bereich = await _context.Bereich.FindAsync(id);
            if (bereich == null)
            {
                return NotFound();
            }

            _context.Bereich.Remove(bereich);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BereichExists(long id)
        {
            return (_context.Bereich?.Any(e => e.BereichId == id)).GetValueOrDefault();
        }
    }
}
