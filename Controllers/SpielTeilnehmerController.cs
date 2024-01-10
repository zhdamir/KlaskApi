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
    /// Controller zur Verwaltung von SpielTeilnehmer-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SpielTeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für SpielTeilnehmer-Entitäten.</param>
        public SpielTeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aktualisiert die Punkte eines SpielTeilnehmers anhand der ID.
        /// </summary>
        /// <param name="spielTeilnehmerId">Die ID des zu aktualisierenden SpielTeilnehmers.</param>
        /// <param name="neuePunkte">Die neuen Punkte für den SpielTeilnehmer.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPost("UpdatePunkte/{spielTeilnehmerId}")]
        public async Task<IActionResult> UpdatePunkte(long spielTeilnehmerId, [FromBody] int neuePunkte)
        {
            try
            {
                if (spielTeilnehmerId <= 0)
                {
                    return BadRequest("Ungültige spielTeilnehmerId. Die SpielTeilnehmerId muss größer als 0 sein.");
                }

                var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(spielTeilnehmerId);

                if (spielTeilnehmer == null)
                {
                    return NotFound("SpielTeilnehmer nicht gefunden.");
                }

                // Punkte aktualisieren
                spielTeilnehmer.Punkte = neuePunkte;

                // Änderungen in der Datenbank speichern
                await _context.SaveChangesAsync();

                // Erfolgsmeldung oder aktualisierten SpielTeilnehmer zurückgeben, falls erforderlich
                return Ok(spielTeilnehmer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Aktualisieren der Punkte: {ex.Message}");
                return StatusCode(500, $"Interner Serverfehler: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft eine Liste aller SpielTeilnehmer ab.
        /// </summary>
        /// <returns>Eine Liste von SpielTeilnehmer-Entitäten.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpielTeilnehmer>>> GetSpieleTeilnehmer()
        {
            // Überprüfen, ob die SpieleTeilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }

            // Alle SpielTeilnehmer abrufen und als Liste zurückgeben
            return await _context.SpieleTeilnehmer.ToListAsync();
        }

        /// <summary>
        /// Ruft einen bestimmten SpielTeilnehmer anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden SpielTeilnehmers.</param>
        /// <returns>Der angeforderte SpielTeilnehmer.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SpielTeilnehmer>> GetSpielTeilnehmer(long id)
        {
            // Überprüfen, ob die SpieleTeilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }

            // Den SpielTeilnehmer mit der angegebenen ID suchen und zurückgeben
            var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(id);

            // Einen 404-Fehler zurückgeben, wenn der SpielTeilnehmer nicht gefunden wurde
            if (spielTeilnehmer == null)
            {
                return NotFound();
            }

            return spielTeilnehmer;
        }

        /// <summary>
        /// Aktualisiert einen bestimmten SpielTeilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden SpielTeilnehmers.</param>
        /// <param name="spielTeilnehmer">Der aktualisierte SpielTeilnehmer.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpielTeilnehmer(long id, SpielTeilnehmer spielTeilnehmer)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != spielTeilnehmer.SpielTeilnehmerId)
            {
                return BadRequest();
            }

            // Den SpielTeilnehmer als modifiziert markieren und Änderungen in der Datenbank speichern
            _context.Entry(spielTeilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // NotFound zurückgeben, wenn der SpielTeilnehmer nicht existiert
                if (!SpielTeilnehmerExists(id))
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
        /// Erstellt einen neuen SpielTeilnehmer.
        /// </summary>
        /// <param name="spielTeilnehmer">Der zu erstellende SpielTeilnehmer.</param>
        /// <returns>Der neu erstellte SpielTeilnehmer.</returns>
        [HttpPost]
        public async Task<ActionResult<SpielTeilnehmer>> PostSpielTeilnehmer(SpielTeilnehmer spielTeilnehmer)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die SpieleTeilnehmer-Entität null ist
            if (_context.SpieleTeilnehmer == null)
            {
                return Problem("Entity set 'TurnierContext.SpieleTeilnehmer' ist null.");
            }

            // Den neuen SpielTeilnehmer hinzufügen und Änderungen in der Datenbank speichern
            _context.SpieleTeilnehmer.Add(spielTeilnehmer);
            await _context.SaveChangesAsync();

            // Einen 201-Statuscode zusammen mit dem erstellten SpielTeilnehmer zurückgeben
            return CreatedAtAction("GetSpielTeilnehmer", new { id = spielTeilnehmer.SpielTeilnehmerId }, spielTeilnehmer);
        }

        /// <summary>
        /// Löscht einen bestimmten SpielTeilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden SpielTeilnehmers.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn der SpielTeilnehmer nicht gefunden wird.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpielTeilnehmer(long id)
        {
            // NotFound zurückgeben, wenn die SpieleTeilnehmer-Entität null ist
            if (_context.SpieleTeilnehmer == null)
            {
                return NotFound();
            }

            // Den SpielTeilnehmer mit der angegebenen ID suchen
            var spielTeilnehmer = await _context.SpieleTeilnehmer.FindAsync(id);

            // NotFound zurückgeben, wenn der SpielTeilnehmer nicht gefunden wurde
            if (spielTeilnehmer == null)
            {
                return NotFound();
            }

            // Den SpielTeilnehmer entfernen und Änderungen in der Datenbank speichern
            _context.SpieleTeilnehmer.Remove(spielTeilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein SpielTeilnehmer mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID des SpielTeilnehmers.</param>
        /// <returns>True, wenn der SpielTeilnehmer existiert; ansonsten false.</returns>
        private bool SpielTeilnehmerExists(long id)
        {
            return (_context.SpieleTeilnehmer?.Any(e => e.SpielTeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
