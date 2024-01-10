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
    /// Controller zur Verwaltung von TurnierTeilnehmer-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TurnierTeilnehmerController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für TurnierTeilnehmer-Entitäten.</param>
        public TurnierTeilnehmerController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft eine Liste aller TurnierTeilnehmer ab.
        /// </summary>
        /// <returns>Eine Liste von TurnierTeilnehmer-Entitäten.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnierTeilnehmer>>> GetTurniereTeilnehmer()
        {
            // Überprüfen, ob die TurnierTeilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.TurniereTeilnehmer == null)
            {
                return NotFound();
            }

            // Alle TurnierTeilnehmer abrufen und als Liste zurückgeben
            return await _context.TurniereTeilnehmer.ToListAsync();
        }

        /// <summary>
        /// Ruft einen bestimmten TurnierTeilnehmer anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden TurnierTeilnehmers.</param>
        /// <returns>Der angeforderte TurnierTeilnehmer.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TurnierTeilnehmer>> GetTurnierTeilnehmer(long id)
        {
            // Überprüfen, ob die TurnierTeilnehmer-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.TurniereTeilnehmer == null)
            {
                return NotFound();
            }

            // Den TurnierTeilnehmer mit der angegebenen ID suchen und zurückgeben
            var turnierTeilnehmer = await _context.TurniereTeilnehmer.FindAsync(id);

            // Einen 404-Fehler zurückgeben, wenn der TurnierTeilnehmer nicht gefunden wurde
            if (turnierTeilnehmer == null)
            {
                return NotFound();
            }

            return turnierTeilnehmer;
        }

        /// <summary>
        /// Aktualisiert einen bestimmten TurnierTeilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden TurnierTeilnehmers.</param>
        /// <param name="turnierTeilnehmer">Der aktualisierte TurnierTeilnehmer.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurnierTeilnehmer(long id, TurnierTeilnehmer turnierTeilnehmer)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != turnierTeilnehmer.TurnierTeilnehmerId)
            {
                return BadRequest();
            }

            // Den TurnierTeilnehmer als modifiziert markieren und Änderungen in der Datenbank speichern
            _context.Entry(turnierTeilnehmer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // NotFound zurückgeben, wenn der TurnierTeilnehmer nicht existiert
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

        /// <summary>
        /// Erstellt einen neuen TurnierTeilnehmer.
        /// </summary>
        /// <param name="turnierTeilnehmer">Der zu erstellende TurnierTeilnehmer.</param>
        /// <returns>Der neu erstellte TurnierTeilnehmer.</returns>
        [HttpPost]
        public async Task<ActionResult<TurnierTeilnehmer>> PostTurnierTeilnehmer(TurnierTeilnehmer turnierTeilnehmer)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die TurnierTeilnehmer-Entität null ist
            if (_context.TurniereTeilnehmer == null)
            {
                return Problem("Entity set 'TurnierContext.TurniereTeilnehmer' ist null.");
            }

            // Den neuen TurnierTeilnehmer hinzufügen und Änderungen in der Datenbank speichern
            _context.TurniereTeilnehmer.Add(turnierTeilnehmer);
            await _context.SaveChangesAsync();

            // Einen 201-Statuscode zusammen mit dem erstellten TurnierTeilnehmer zurückgeben
            return CreatedAtAction("GetTurnierTeilnehmer", new { id = turnierTeilnehmer.TurnierTeilnehmerId }, turnierTeilnehmer);
        }

        /// <summary>
        /// Löscht einen bestimmten TurnierTeilnehmer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden TurnierTeilnehmers.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn der TurnierTeilnehmer nicht gefunden wird.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurnierTeilnehmer(long id)
        {
            // NotFound zurückgeben, wenn die TurnierTeilnehmer-Entität null ist
            if (_context.TurniereTeilnehmer == null)
            {
                return NotFound();
            }

            // Den TurnierTeilnehmer mit der angegebenen ID suchen
            var turnierTeilnehmer = await _context.TurniereTeilnehmer.FindAsync(id);

            // NotFound zurückgeben, wenn der TurnierTeilnehmer nicht gefunden wurde
            if (turnierTeilnehmer == null)
            {
                return NotFound();
            }

            // Den TurnierTeilnehmer entfernen und Änderungen in der Datenbank speichern
            _context.TurniereTeilnehmer.Remove(turnierTeilnehmer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein TurnierTeilnehmer mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID des TurnierTeilnehmers.</param>
        /// <returns>True, wenn der TurnierTeilnehmer existiert; ansonsten false.</returns>
        private bool TurnierTeilnehmerExists(long id)
        {
            return (_context.TurniereTeilnehmer?.Any(e => e.TurnierTeilnehmerId == id)).GetValueOrDefault();
        }
    }
}
