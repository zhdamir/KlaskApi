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
    /// <summary>
    /// Controller zur Verwaltung von Bereich-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BereichController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für Bereich-Entitäten.</param>
        public BereichController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft eine Liste aller Bereiche ab.
        /// </summary>
        /// <returns>Eine Liste von Bereich-Entitäten.</returns>
        // GET: api/Bereich
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bereich>>> GetBereich()
        {
            // Überprüfen, ob die Bereich-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Bereich == null)
            {
                return NotFound();
            }
            // Alle Bereiche abrufen und als Liste zurückgeben
            return await _context.Bereich.ToListAsync();
        }

        /// <summary>
        /// Ruft einen bestimmten Bereich anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden Bereichs.</param>
        /// <returns>Der angeforderte Bereich.</returns>
        // GET: api/Bereich/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bereich>> GetBereich(long id)
        {
            // Überprüfen, ob die Bereich-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Bereich == null)
            {
                return NotFound();
            }
            // Den Bereich mit der angegebenen ID suchen und zurückgeben
            var bereich = await _context.Bereich.FindAsync(id);

            if (bereich == null)
            {
                return NotFound();
            }

            return bereich;
        }

        /// <summary>
        /// Aktualisiert einen bestimmten Bereich anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Bereichs.</param>
        /// <param name="bereich">Der aktualisierte Bereich.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        // PUT: api/Bereich/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBereich(long id, Bereich bereich)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != bereich.BereichId)
            {
                return BadRequest();
            }

            // Den Bereich als modifiziert markieren und Änderungen in der Datenbank speichern
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

        /// <summary>
        /// Erstellt einen neuen Bereich.
        /// </summary>
        /// <param name="bereich">Der zu erstellende Bereich.</param>
        /// <returns>Der neu erstellte Bereich.</returns>
        // POST: api/Bereich
        [HttpPost]
        public async Task<ActionResult<Bereich>> PostBereich(Bereich bereich)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die Bereich-Entität null ist
            if (_context.Bereich == null)
            {
                return Problem("Entity set 'TurnierContext.Bereich'  is null.");
            }

            // Den neuen Bereich hinzufügen und Änderungen in der Datenbank speichern
            _context.Bereich.Add(bereich);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBereich", new { id = bereich.BereichId }, bereich);
        }

        /// <summary>
        /// Löscht einen bestimmten Bereich anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Bereichs.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn der Bereich nicht gefunden wird.</returns>
        // DELETE: api/Bereich/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBereich(long id)
        {
            if (_context.Bereich == null)
            {
                return NotFound();
            }
            // Den Bereich mit der angegebenen ID suchen
            var bereich = await _context.Bereich.FindAsync(id);
            if (bereich == null)
            {
                return NotFound();
            }

            // Den Bereich entfernen und Änderungen in der Datenbank speichern
            _context.Bereich.Remove(bereich);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein Bereich mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID des Bereichs.</param>
        /// <returns>True, wenn der Bereich existiert; ansonsten false.</returns>
        private bool BereichExists(long id)
        {
            return (_context.Bereich?.Any(e => e.BereichId == id)).GetValueOrDefault();
        }
    }
}
