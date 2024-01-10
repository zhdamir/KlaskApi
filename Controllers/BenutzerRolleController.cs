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

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für BenutzerRolle-Entitäten.</param>
        public BenutzerRolleController(TurnierContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Ruft eine Liste aller BenutzerRollen ab.
        /// </summary>
        /// <returns>Eine Liste von BenutzerRolle-Entitäten.</returns>
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

        /// <summary>
        /// Ruft eine bestimmte BenutzerRolle anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID der abzurufenden BenutzerRolle.</param>
        /// <returns>Die angeforderte BenutzerRolle-Entität.</returns>
        // GET: api/BenutzerRolle/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BenutzerRolle>> GetBenutzerRolle(long id)
        {
            // Überprüft, ob die BenutzerRollen-Entität vorhanden ist, andernfalls gibt es einen 404-Fehler zurück
            if (_context.BenutzerRollen == null)
            {
                return NotFound();
            }
            // Sucht die BenutzerRolle mit der angegebenen ID
            var benutzerRolle = await _context.BenutzerRollen.FindAsync(id);

            // Überprüft, ob die BenutzerRolle gefunden wurde, andernfalls gibt es einen 404-Fehler zurück
            if (benutzerRolle == null)
            {
                return NotFound();
            }

            return benutzerRolle;
        }

        /// <summary>
        /// Aktualisiert eine bestimmte BenutzerRolle anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu aktualisierenden BenutzerRolle.</param>
        /// <param name="benutzerRolle">Die aktualisierte BenutzerRolle-Entität.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        // PUT: api/BenutzerRolle/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBenutzerRolle(long id, BenutzerRolle benutzerRolle)
        {
            // Überprüft, ob die angegebene ID mit der ID der BenutzerRolle übereinstimmt, andernfalls gibt es einen 400-Fehler zurück
            if (id != benutzerRolle.RolleId)
            {
                return BadRequest();
            }

            // Markiert die BenutzerRolle als modifiziert und speichert die Änderungen in der Datenbank
            _context.Entry(benutzerRolle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Überprüft, ob die BenutzerRolle nicht existiert, andernfalls gibt es einen 404-Fehler zurück
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

        /// <summary>
        /// Erstellt eine neue BenutzerRolle.
        /// </summary>
        /// <param name="benutzerRolle">Die zu erstellende BenutzerRolle-Entität.</param>
        /// <returns>Die neu erstellte BenutzerRolle-Entität.</returns>
        // POST: api/BenutzerRolle
        [HttpPost]
        public async Task<ActionResult<BenutzerRolle>> PostBenutzerRolle(BenutzerRolle benutzerRolle)
        {
            // Überprüft, ob die BenutzerRollen-Entität vorhanden ist, andernfalls gibt es einen Fehler mit einer Meldung zurück
            if (_context.BenutzerRollen == null)
            {
                return Problem("Entity set 'TurnierContext.BenutzerRollen'  is null.");
            }
            // Fügt die neue BenutzerRolle hinzu und speichert die Änderungen in der Datenbank
            _context.BenutzerRollen.Add(benutzerRolle);
            await _context.SaveChangesAsync();

            // Gibt einen 201-Statuscode zusammen mit der erstellten BenutzerRolle zurück
            return CreatedAtAction("GetBenutzerRolle", new { id = benutzerRolle.RolleId }, benutzerRolle);
        }


        /// <summary>
        /// Löscht eine bestimmte BenutzerRolle anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden BenutzerRolle.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn die BenutzerRolle nicht gefunden wird.</returns>
        // DELETE: api/BenutzerRolle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBenutzerRolle(long id)
        {

            // Überprüft, ob die BenutzerRollen-Entität vorhanden ist, andernfalls gibt es einen 404-Fehler zurück
            if (_context.BenutzerRollen == null)
            {
                return NotFound();
            }
            // Sucht die BenutzerRolle mit der angegebenen ID
            var benutzerRolle = await _context.BenutzerRollen.FindAsync(id);
            // Überprüft, ob die BenutzerRolle gefunden wurde, andernfalls gibt es einen 404-Fehler zurück
            if (benutzerRolle == null)
            {
                return NotFound();
            }
            // Entfernt die BenutzerRolle und speichert die Änderungen in der Datenbank
            _context.BenutzerRollen.Remove(benutzerRolle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob eine BenutzerRolle mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID der BenutzerRolle.</param>
        /// <returns>True, wenn die BenutzerRolle existiert; ansonsten false.</returns>
        private bool BenutzerRolleExists(long id)
        {
            return (_context.BenutzerRollen?.Any(e => e.RolleId == id)).GetValueOrDefault();
        }
    }
}
