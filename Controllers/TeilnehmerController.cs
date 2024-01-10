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
    /// Controller zur Verwaltung von Teilnehmer-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für Teilnehmer-Entitäten.</param>
        public TeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft eine Liste aller Teilnehmer ab.
        /// </summary>
        /// <returns>Eine Liste von Teilnehmer-Entitäten.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teilnehmer>>> GetTeilnehmer()
        {
            // Überprüfen, ob die Teilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Teilnehmer == null)
            {
                return NotFound();
            }

            // Alle Teilnehmer abrufen und als Liste zurückgeben
            return await _context.Teilnehmer.ToListAsync();
        }

        /// <summary>
        /// Ruft einen bestimmten Teilnehmer anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden Teilnehmers.</param>
        /// <returns>Der angeforderte Teilnehmer.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Teilnehmer>> GetTeilnehmer(long id)
        {
            // Überprüfen, ob die Teilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Teilnehmer == null)
            {
                return NotFound();
            }

            // Den Teilnehmer mit der angegebenen ID suchen und zurückgeben
            var teilnehmer = await _context.Teilnehmer.FindAsync(id);

            // Einen 404-Fehler zurückgeben, wenn der Teilnehmer nicht gefunden wurde
            if (teilnehmer == null)
            {
                return NotFound();
            }

            return teilnehmer;
        }

        /// <summary>
        /// Aktualisiert einen bestimmten Teilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Teilnehmers.</param>
        /// <param name="teilnehmer">Der aktualisierte Teilnehmer.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeilnehmer(long id, Teilnehmer teilnehmer)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != teilnehmer.TeilnehmerId)
            {
                return BadRequest();
            }

            // Den Teilnehmer als modifiziert markieren und Änderungen in der Datenbank speichern
            _context.Entry(teilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // NotFound zurückgeben, wenn der Teilnehmer nicht existiert
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

        /// <summary>
        /// Erstellt einen neuen Teilnehmer.
        /// </summary>
        /// <param name="teilnehmer">Der zu erstellende Teilnehmer.</param>
        /// <returns>Der neu erstellte Teilnehmer.</returns>
        [HttpPost]
        public async Task<ActionResult<Teilnehmer>> PostTeilnehmer([FromBody] Teilnehmer teilnehmer)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die Teilnehmer-Entität null ist
            if (_context.Teilnehmer == null)
            {
                return Problem("Entity set 'TurnierContext.Teilnehmer' ist null.");
            }

            // Den neuen Teilnehmer hinzufügen und Änderungen in der Datenbank speichern
            _context.Teilnehmer.Add(teilnehmer);
            await _context.SaveChangesAsync();

            // Einen 201-Statuscode zusammen mit dem erstellten Teilnehmer zurückgeben
            return CreatedAtAction("GetTeilnehmer", new { id = teilnehmer.TeilnehmerId }, teilnehmer);
        }

        /// <summary>
        /// Löscht einen bestimmten Teilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Teilnehmers.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn der Teilnehmer nicht gefunden wird.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeilnehmer(long id)
        {
            // NotFound zurückgeben, wenn die Teilnehmer-Entität null ist
            if (_context.Teilnehmer == null)
            {
                return NotFound();
            }

            // Den Teilnehmer mit der angegebenen ID suchen
            var teilnehmer = await _context.Teilnehmer.FindAsync(id);

            // NotFound zurückgeben, wenn der Teilnehmer nicht gefunden wurde
            if (teilnehmer == null)
            {
                return NotFound();
            }

            // Den Teilnehmer entfernen und Änderungen in der Datenbank speichern
            _context.Teilnehmer.Remove(teilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein Teilnehmer mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID des Teilnehmers.</param>
        /// <returns>True, wenn der Teilnehmer existiert; ansonsten false.</returns>
        private bool TeilnehmerExists(long id)
        {
            return (_context.Teilnehmer?.Any(e => e.TeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
