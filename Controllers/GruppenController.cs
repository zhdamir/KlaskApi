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
    public class GruppenController : ControllerBase
    {
        private readonly TurnierContext _context;

        public GruppenController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/Gruppen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gruppe>>> GetGruppen()
        {
            if (_context.Gruppen == null)
            {
                return NotFound();
            }
            return await _context.Gruppen.ToListAsync();
        }

        // GET: api/Gruppen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Gruppe>> GetGruppe(long id)
        {
            if (_context.Gruppen == null)
            {
                return NotFound();
            }
            var gruppe = await _context.Gruppen.FindAsync(id);

            if (gruppe == null)
            {
                return NotFound();
            }

            return gruppe;
        }

        // PUT: api/Gruppen/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGruppe(long id, Gruppe gruppe)
        {
            if (id != gruppe.GruppeId)
            {
                return BadRequest();
            }

            _context.Entry(gruppe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GruppeExists(id))
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

        // POST: api/Gruppen
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Gruppe>> PostGruppe(Gruppe gruppe)
        {

            gruppe.Turnier.StartDatum = gruppe.Turnier.StartDatum.ToUniversalTime();
            gruppe.Turnier.EndDatum = gruppe.Turnier.EndDatum.ToUniversalTime();
            if (_context.Gruppen == null)
            {
                return Problem("Entity set 'TurnierContext.Gruppen'  is null.");
            }
            _context.Gruppen.Add(gruppe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGruppe", new { id = gruppe.GruppeId }, gruppe);
        }

        // DELETE: api/Gruppen/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGruppe(long id)
        {
            if (_context.Gruppen == null)
            {
                return NotFound();
            }
            var gruppe = await _context.Gruppen.FindAsync(id);
            if (gruppe == null)
            {
                return NotFound();
            }

            _context.Gruppen.Remove(gruppe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GruppeExists(long id)
        {
            return (_context.Gruppen?.Any(e => e.GruppeId == id)).GetValueOrDefault();
        }
    }
}
