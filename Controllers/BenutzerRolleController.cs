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
    public class BenutzerRolleController : ControllerBase
    {
        private readonly TurnierContext _context;

        // Konstruktor, der den Kontext für die Datenbankverbindung initialisiert
        public BenutzerRolleController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/BenutzerRolle
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BenutzerRolle>>> GetBenutzerRollen()
        {
            // Überprüft, ob die BenutzerRollen-Entität vorhanden ist, andernfalls gibt es einen 404-Fehler zurück
            if (_context.BenutzerRollen == null)
            {
                return NotFound();
            }
            // Ruft alle BenutzerRollen ab und gibt sie als Liste zurück
            return await _context.BenutzerRollen.ToListAsync();
        }

        // GET: api/BenutzerRolle/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BenutzerRolle>> GetBenutzerRolle(long id)
        {
            if (_context.BenutzerRollen == null)
            {
                return NotFound();
            }
            var benutzerRolle = await _context.BenutzerRollen.FindAsync(id);

            if (benutzerRolle == null)
            {
                return NotFound();
            }

            return benutzerRolle;
        }

        // PUT: api/BenutzerRolle/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBenutzerRolle(long id, BenutzerRolle benutzerRolle)
        {
            if (id != benutzerRolle.RolleId)
            {
                return BadRequest();
            }

            _context.Entry(benutzerRolle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BenutzerRolleExists(id))
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

        // POST: api/BenutzerRolle
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BenutzerRolle>> PostBenutzerRolle(BenutzerRolle benutzerRolle)
        {
            if (_context.BenutzerRollen == null)
            {
                return Problem("Entity set 'TurnierContext.BenutzerRollen'  is null.");
            }
            _context.BenutzerRollen.Add(benutzerRolle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBenutzerRolle", new { id = benutzerRolle.RolleId }, benutzerRolle);
        }

        // DELETE: api/BenutzerRolle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBenutzerRolle(long id)
        {
            if (_context.BenutzerRollen == null)
            {
                return NotFound();
            }
            var benutzerRolle = await _context.BenutzerRollen.FindAsync(id);
            if (benutzerRolle == null)
            {
                return NotFound();
            }

            _context.BenutzerRollen.Remove(benutzerRolle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BenutzerRolleExists(long id)
        {
            return (_context.BenutzerRollen?.Any(e => e.RolleId == id)).GetValueOrDefault();
        }
    }
}
