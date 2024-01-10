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
    /// Controller zur Verwaltung von Gruppen-Entitäten über API-Endpunkte.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GruppeController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Konstruktor zur Initialisierung des Controllers mit einem Datenbankkontext.
        /// </summary>
        /// <param name="context">Der Datenbankkontext für Gruppen-Entitäten.</param>
        public GruppeController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft eine Liste aller Gruppen ab.
        /// </summary>
        /// <returns>Eine Liste von Gruppen-Entitäten.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gruppe>>> GetGruppen()
        {
            // Überprüfen, ob die Gruppen-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Gruppen == null)
            {
                return NotFound();
            }

            // Alle Gruppen abrufen und als Liste zurückgeben
            return await _context.Gruppen.ToListAsync();
        }

        /// <summary>
        /// Ruft eine bestimmte Gruppe anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID der abzurufenden Gruppe.</param>
        /// <returns>Die angeforderte Gruppe.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Gruppe>> GetGruppe(long id)
        {
            // Überprüfen, ob die Gruppen-Entität null ist, und bei Bedarf einen 404-Fehler zurückgeben
            if (_context.Gruppen == null)
            {
                return NotFound();
            }

            // Die Gruppe mit der angegebenen ID suchen und zurückgeben
            var gruppe = await _context.Gruppen.FindAsync(id);

            // Einen 404-Fehler zurückgeben, wenn die Gruppe nicht gefunden wurde
            if (gruppe == null)
            {
                return NotFound();
            }

            return gruppe;
        }

        /// <summary>
        /// Aktualisiert eine bestimmte Gruppe anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu aktualisierenden Gruppe.</param>
        /// <param name="gruppe">Die aktualisierte Gruppe.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; BadRequest, wenn die ID nicht zur Entität passt.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGruppe(long id, Gruppe gruppe)
        {
            // BadRequest zurückgeben, wenn die angegebene ID nicht mit der ID der Entität übereinstimmt
            if (id != gruppe.GruppeId)
            {
                return BadRequest();
            }

            // Die Gruppe als modifiziert markieren und Änderungen in der Datenbank speichern
            _context.Entry(gruppe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // NotFound zurückgeben, wenn die Gruppe nicht existiert
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

        /// <summary>
        /// Erstellt eine neue Gruppe.
        /// </summary>
        /// <param name="gruppe">Die zu erstellende Gruppe.</param>
        /// <returns>Die neu erstellte Gruppe.</returns>
        [HttpPost]
        public async Task<ActionResult<Gruppe>> PostGruppe(Gruppe gruppe)
        {
            // Fehler mit einer Meldung zurückgeben, wenn die Gruppen-Entität null ist
            if (_context.Gruppen == null)
            {
                return Problem("Entity set 'TurnierContext.Gruppen' ist null.");
            }

            // Die neue Gruppe hinzufügen und Änderungen in der Datenbank speichern
            _context.Gruppen.Add(gruppe);
            await _context.SaveChangesAsync();

            // Einen 201-Statuscode zusammen mit der erstellten Gruppe zurückgeben
            return CreatedAtAction("GetGruppe", new { id = gruppe.GruppeId }, gruppe);
        }

        /// <summary>
        /// Löscht eine bestimmte Gruppe anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Gruppe.</param>
        /// <returns>Kein Inhalt, wenn erfolgreich; NotFound, wenn die Gruppe nicht gefunden wird.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGruppe(long id)
        {
            // NotFound zurückgeben, wenn die Gruppen-Entität null ist
            if (_context.Gruppen == null)
            {
                return NotFound();
            }

            // Die Gruppe mit der angegebenen ID suchen
            var gruppe = await _context.Gruppen.FindAsync(id);

            // NotFound zurückgeben, wenn die Gruppe nicht gefunden wurde
            if (gruppe == null)
            {
                return NotFound();
            }

            // Die Gruppe entfernen und Änderungen in der Datenbank speichern
            _context.Gruppen.Remove(gruppe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob eine Gruppe mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die zu überprüfende ID der Gruppe.</param>
        /// <returns>True, wenn die Gruppe existiert; ansonsten false.</returns>
        private bool GruppeExists(long id)
        {
            return (_context.Gruppen?.Any(e => e.GruppeId == id)).GetValueOrDefault();
        }
    }
}
