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
    /// Controller zur Verwaltung von Spiel-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SpielController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für Spiel-Entitäten.</param>
        public SpielController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft eine Liste aller Spiele ab.
        /// </summary>
        /// <returns>Eine Liste von Spiel-Entitäten.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Spiel>>> GetSpiele()
        {
            // Überprüfen, ob die Spiele-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Spiele == null)
            {
                return NotFound();
            }

            // Alle Spiele abrufen und als Liste zurückgeben
            return await _context.Spiele.ToListAsync();
        }

        /// <summary>
        /// Ruft ein bestimmtes Spiel anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden Spiels.</param>
        /// <returns>Das angeforderte Spiel.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Spiel>> GetSpiel(long id)
        {
            // Überprüfen, ob die Spiele-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Spiele == null)
            {
                return NotFound();
            }

            // Das Spiel mit der angegebenen ID suchen und zurückgeben
            var spiel = await _context.Spiele.FindAsync(id);

            // Einen 404-Fehler zurückgeben, wenn das Spiel nicht gefunden wurde
            if (spiel == null)
            {
                return NotFound();
            }

            return spiel;
        }

        /// <summary>
        /// Aktualisiert ein bestimmtes Spiel anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Spiels.</param>
        /// <param name="spiel">Das aktualisierte Spiel.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpiel(long id, Spiel spiel)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != spiel.SpielId)
            {
                return BadRequest();
            }

            // Das Spiel als modifiziert markieren und Änderungen in der Datenbank speichern
            _context.Entry(spiel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // NotFound zurückgeben, wenn das Spiel nicht existiert
                if (!SpielExists(id))
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
        /// Erstellt ein neues Spiel.
        /// </summary>
        /// <param name="spiel">Das zu erstellende Spiel.</param>
        /// <returns>Das neu erstellte Spiel.</returns>
        [HttpPost]
        public async Task<ActionResult<Spiel>> PostSpiel(Spiel spiel)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die Spiele-Entität null ist
            if (_context.Spiele == null)
            {
                return Problem("Entity set 'TurnierContext.Spiele' ist null.");
            }

            // Das neue Spiel hinzufügen und Änderungen in der Datenbank speichern
            _context.Spiele.Add(spiel);
            await _context.SaveChangesAsync();

            // Einen 201-Statuscode zusammen mit dem erstellten Spiel zurückgeben
            return CreatedAtAction("GetSpiel", new { id = spiel.SpielId }, spiel);
        }

        /// <summary>
        /// Löscht ein bestimmtes Spiel anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Spiels.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn das Spiel nicht gefunden wird.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpiel(long id)
        {
            // NotFound zurückgeben, wenn die Spiele-Entität null ist
            if (_context.Spiele == null)
            {
                return NotFound();
            }

            // Das Spiel mit der angegebenen ID suchen
            var spiel = await _context.Spiele.FindAsync(id);

            // NotFound zurückgeben, wenn das Spiel nicht gefunden wurde
            if (spiel == null)
            {
                return NotFound();
            }

            // Das Spiel entfernen und Änderungen in der Datenbank speichern
            _context.Spiele.Remove(spiel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein Spiel mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID des Spiels.</param>
        /// <returns>True, wenn das Spiel existiert; ansonsten false.</returns>
        private bool SpielExists(long id)
        {
            return (_context.Spiele?.Any(e => e.SpielId == id)).GetValueOrDefault();
        }
    }
}
