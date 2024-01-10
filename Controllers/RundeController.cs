using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;
using KlaskApi.Services;

namespace KlaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RundeController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="RundeController"/>-Klasse.
        /// </summary>
        /// <param name="context">Der Datenbankkontext zum Zugriff und Verwalten von datenbankbezogenen Informationen im Zusammenhang mit Turnieren.</param>
        public RundeController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft die Historie der Vorrundendetails für ein bestimmtes Turnier ab.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die Vorrundendetails abgerufen werden sollen.</param>
        /// <returns>Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Vorrundendetails oder einer Fehlermeldung zurückgibt.</returns>
        [HttpGet("vorrundenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> VorrundenDetailsHistorie(long turnierId)
        {
            try
            {
                // Turnier abrufen
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No  turnier found.");
                }


                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                          .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VorrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft die Ergebnisse der Vorrundenhistorie für ein bestimmtes Turnier ab.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die Vorrundenhistorie abgerufen werden soll.</param>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Vorrundenergebnissen oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("vorrundenResultsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> VorrundenResults(long turnierId)
        {
            try
            {
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das aktive Turnier abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                      .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId)
                    .Join(_context.Turniere, j => j.Runde.TurnierId, t => t.Id, (j, t) => new
                    {
                        TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                        Vorname = j.Teilnehmer.Vorname,
                        SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                        Punkte = j.SpieleTeilnehmer.Punkte,
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        RundeId = j.Runde.RundeId,
                        RundeBezeichnung = j.Runde.RundeBezeichnung,
                        TurnierTitel = t.TurnierTitel,
                        SpielId = j.Spiel.SpielId
                    })
                    .ToListAsync();

                // Ergebnisse der Vorrunden aggregieren
                var result = turnierDetails.Distinct().Select(td => new
                {
                    GruppeId = td.GruppeId,
                    Gruppenname = td.Gruppenname,
                    TeilnehmerId = td.TeilnehmerId,
                    Vorname = td.Vorname,
                    RundeBezeichnung = td.RundeBezeichnung,
                    AnzahlSpiele = GetAnzahlSpiele(td.TeilnehmerId, td.GruppeId, turnierId),
                    AnzahlSiege = GetAnzahlSiege(td.TeilnehmerId, td.GruppeId, turnierId),
                    SatzDifferenz = GetSatzDifferenz(td.TeilnehmerId, td.GruppeId, turnierId)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GruppenrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft die Details für Spiele um den Dritten Platz in der Historie eines bestimmten Turniers ab.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die Details zu den Spielen um den Dritten Platz abgerufen werden sollen.</param>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Details zu den Spielen um den Dritten Platz oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("spielUmDrittenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> SpielUmDrittenDetailsHistorie(long turnierId)
        {
            try
            {
                // Turnier abrufen
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);
                if (requestedTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }
                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                        .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("SpielUmDritten") && j.Runde.TurnierId == turnierId)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();
                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VorrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Ruft die Details für das Finale in der Historie eines bestimmten Turniers ab.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die Details zum Finale abgerufen werden sollen.</param>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Details zum Finale oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("finaleDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> FinaleDetailsHistorie(long turnierId)
        {
            try
            {
                // Turnier abrufen
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);
                if (requestedTurnier == null)
                {
                    return NotFound("No turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                        .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("Finale") && j.Runde.TurnierId == turnierId)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();
                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FinaleDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft die Details für das aktuelle Finale eines Turniers ab.
        /// </summary>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Details zum aktuellen Finale oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("finaleDetails")]
        public async Task<ActionResult<object>> FinaleDetails()
        {
            try
            {
                // Aktives Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);
                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                        .Where(j => j.TurnierTeilnehmer.TurnierId == activeTurnier.Id)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("Finale") && j.Runde.TurnierId == activeTurnier.Id)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();
                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VorrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft die Details für das aktuelle Spiel um den Dritten Platz eines Turniers ab.
        /// </summary>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Details zum aktuellen Spiel um den Dritten Platz oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("spielUmDrittenDetails")]
        public async Task<ActionResult<object>> SpielUmDrittenDetails()
        {
            try
            {
                // Aktives Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);
                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }
                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                        .Where(j => j.TurnierTeilnehmer.TurnierId == activeTurnier.Id)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("SpielUmDritten") && j.Runde.TurnierId == activeTurnier.Id)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();
                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VorrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        /// <summary>
        /// Ruft die Ergebnisse der Vorrunden für das aktive Turnier ab.
        /// </summary>
        /// <returns>
        /// Eine asynchrone Aufgabe, die eine Aktionsergebnis-Instanz mit den abgerufenen Vorrundenergebnissen oder einer Fehlermeldung zurückgibt.
        /// </returns>
        [HttpGet("vorrundenResults")]
        public async Task<ActionResult<object>> VorrundenResults()
        {
            try
            {
                // Aktives Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das aktive Turnier abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Where(j => j.TurnierTeilnehmer.TurnierId == activeTurnier.Id)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == activeTurnier.Id)
                    .Join(_context.Turniere, j => j.Runde.TurnierId, t => t.Id, (j, t) => new
                    {
                        TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                        Vorname = j.Teilnehmer.Vorname,
                        SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                        Punkte = j.SpieleTeilnehmer.Punkte,
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        RundeId = j.Runde.RundeId,
                        RundeBezeichnung = j.Runde.RundeBezeichnung,
                        TurnierTitel = t.TurnierTitel,
                        SpielId = j.Spiel.SpielId
                    })
                    .ToListAsync();

                // Ergebnisse aggregieren
                var result = turnierDetails.Distinct().Select(td => new
                {
                    GruppeId = td.GruppeId,
                    Gruppenname = td.Gruppenname,
                    TeilnehmerId = td.TeilnehmerId,
                    Vorname = td.Vorname,
                    RundeBezeichnung = td.RundeBezeichnung,
                    AnzahlSpiele = GetAnzahlSpiele(td.TeilnehmerId, td.GruppeId, activeTurnier.Id),
                    AnzahlSiege = GetAnzahlSiege(td.TeilnehmerId, td.GruppeId, activeTurnier.Id),
                    SatzDifferenz = GetSatzDifferenz(td.TeilnehmerId, td.GruppeId, activeTurnier.Id)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GruppenrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Berechnet die Anzahl der Spiele, an denen der Teilnehmer teilgenommen hat, in der angegebenen Gruppe und im angegebenen Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>
        /// Die Anzahl der Spiele, an denen der Teilnehmer teilgenommen hat.
        /// </returns>
        private long GetAnzahlSpiele(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {

                // SpieleTeilnehmer für den angegebenen Teilnehmer, Gruppe und Turnier abrufen
                var anzahlSpiele = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                    .Count();

                return anzahlSpiele;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAnzahlSpiele: {ex.Message}");
                // Handle exceptions appropriately, e.g., log or throw
                throw;
            }
        }

        /// <summary>
        /// Berechnet die Anzahl der Siege des Teilnehmers in der angegebenen Gruppe und im angegebenen Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>
        /// Die Anzahl der Siege des Teilnehmers.
        /// </returns>
        private int GetAnzahlSiege(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {
                // Siege des Teilnehmers in der Gruppenvorunde abrufen
                var anzahlSiege = _context.SpieleTeilnehmer
               .Where(st => st.TeilnehmerId == teilnehmerId)
               .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
               .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
               .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
               .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
               .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
               .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
               .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                .Where(j => j.Opponent.Punkte != null && j.SpieleTeilnehmer.Punkte > j.Opponent.Punkte)
               .Count();

                return anzahlSiege;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAnzahlSiege: {ex.Message}");
                // Handle exceptions appropriately, e.g., log or throw
                throw;
            }
        }

        /// <summary>
        /// Berechnet die Satzdifferenz des Teilnehmers in der angegebenen Gruppe und im angegebenen Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>
        /// Die Satzdifferenz des Teilnehmers.
        /// </returns>
        private long GetSatzDifferenz(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {
                // Satzdifferenz des Teilnehmers in der Vorunde abrufen
                var satzDifferenz = _context.SpieleTeilnehmer
                .Where(st => st.TeilnehmerId == teilnehmerId)
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                .Where(j => j.Opponent.Punkte != null)
                 .Sum(j => (j.SpieleTeilnehmer.Punkte - j.Opponent.Punkte).GetValueOrDefault());

                return satzDifferenz;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSatzDifferenz: {ex.Message}");
                // Handle exceptions appropriately, e.g., log or throw
                throw;
            }
        }

        /// <summary>
        /// Gibt Details zu den Vorrunden des aktiven Turniers zurück.
        /// </summary>
        /// <returns>Gibt eine Liste mit detaillierten Informationen zu den Vorrunden zurück.</returns>
        [HttpGet("vorrundenDetails")]
        public async Task<ActionResult<object>> VorrundenDetails()
        {
            try
            {
                // Aktives Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }


                // Gruppen, Teilnehmer und Spiele für das aktive Turnier und die zweitgrößte Runde abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele,
                        st => st.SpielId,
                        s => s.SpielId,
                        (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer,
                        j => j.SpieleTeilnehmer.TeilnehmerId,
                        tn => tn.TeilnehmerId,
                        (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer,
                        j => j.Teilnehmer.TeilnehmerId,
                        tt => tt.TeilnehmerId,
                        (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                        .Where(j => j.TurnierTeilnehmer.TurnierId == activeTurnier.Id)
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                     .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == activeTurnier.Id)
                    .Join(_context.Turniere,
                        j => j.Runde.TurnierId,
                        t => t.Id,
                        (j, t) => new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                            SpielTeilnehmerId = j.SpieleTeilnehmer.SpielTeilnehmerId,
                            Punkte = j.SpieleTeilnehmer.Punkte,
                            GruppeId = j.Gruppe.GruppeId,
                            Gruppenname = j.Gruppe.Gruppenname,
                            RundeId = j.Runde.RundeId,
                            RundeBezeichnung = j.Runde.RundeBezeichnung,
                            TurnierTitel = t.TurnierTitel,
                            SpielId = j.Spiel.SpielId
                        })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VorrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Startet das Finale und das Spiel um den Dritten für das angegebene Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das das Finale und das Spiel um den Dritten gestartet werden sollen.</param>
        /// <returns>Ein ActionResult, das den Erfolg oder Fehler des Vorgangs widerspiegelt.</returns>
        [HttpPost("startFinale")]
        public async Task<IActionResult> StartFinale([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {
                // Log or return a more specific error response for an invalid turnierId
                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                // FinaleService und SpielUmDrittenService initialisieren und starten
                var finaleService = new StartFinaleService(_context);
                var createdFinale = await finaleService.FinaleTeilnehmer(turnierId);

                var spielUmDritten = new StartSpielUmDrittenService(_context);
                var createdSpielUmDritten = await spielUmDritten.SpielUmDrittenTeilnehmer(turnierId);

                if (!createdFinale)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Finale failed. Check your data.");
                }

                if (!createdSpielUmDritten)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Spiel um Dritten failed. Check your data.");
                }


                // Return success response or createdGroups if needed
                return Ok(createdFinale && createdSpielUmDritten);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Startet die Vorrunde für das angegebene Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die Vorrunde gestartet werden soll.</param>
        /// <returns>Ein ActionResult, das den Erfolg oder Fehler des Vorgangs widerspiegelt.</returns>
        [HttpPost("startVorrunde")]
        public async Task<IActionResult> StartVorrunde([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {
                // Log or return a more specific error response for an invalid turnierId
                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                var vorrundeService = new StartVorrundeService(_context);
                var createdVorrunde = await vorrundeService.VorrundeTeilnehmer(turnierId);

                if (!createdVorrunde)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Vorrunde  failed. Check your data.");
                }

                // Return success response or createdGroups if needed
                return Ok(createdVorrunde);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        /// <summary>
        /// Ruft alle Runden ab.
        /// </summary>
        /// <returns>Eine ActionResult, die eine Liste aller Runden oder NotFound zurückgibt.</returns>
        ///GET: api/Runde
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Runde>>> GetRunden()
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            return await _context.Runden.ToListAsync();
        }


        /// <summary>
        /// Ruft eine bestimmte Runde anhand ihrer ID ab.
        /// </summary>
        /// <param name="id">Die ID der abzurufenden Runde.</param>
        /// <returns>Eine ActionResult, die die abgerufene Runde oder NotFound zurückgibt.</returns>
        // GET: api/Runde/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Runde>> GetRunde(long id)
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }

            // Runde anhand der ID suchen
            var runde = await _context.Runden.FindAsync(id);

            if (runde == null)
            {
                return NotFound();
            }

            return runde;
        }

        /// <summary>
        /// Aktualisiert eine bestimmte Runde anhand ihrer ID.
        /// </summary>
        /// <param name="id">Die ID der zu aktualisierenden Runde.</param>
        /// <param name="runde">Die aktualisierten Informationen für die Runde.</param>
        /// <returns>Eine IActionResult, die NoContent, NotFound oder BadRequest zurückgeben kann.</returns>
        // PUT: api/Runde/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRunde(long id, Runde runde)
        {
            if (id != runde.RundeId)
            {
                return BadRequest();
            }

            _context.Entry(runde).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RundeExists(id))
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
        /// Fügt eine neue Runde hinzu.
        /// </summary>
        /// <param name="runde">Die Informationen für die neue Runde.</param>
        /// <returns>Eine ActionResult, die CreatedAtAction, Problem oder BadRequest zurückgeben kann.</returns>
        // POST: api/Runde
        [HttpPost]
        public async Task<ActionResult<Runde>> PostRunde(Runde runde)
        {
            if (_context.Runden == null)
            {
                return Problem("Entity set 'TurnierContext.Runden'  is null.");
            }
            //neue Runde hinzufügen
            _context.Runden.Add(runde);

            // Speichere die Änderungen in der Datenbank
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRunde", new { id = runde.RundeId }, runde);
        }

        /// <summary>
        /// Löscht eine Runde basierend auf der angegebenen ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Runde.</param>
        /// <returns>Eine ActionResult, die NoContent oder NotFound zurückgeben kann.</returns>
        // DELETE: api/Runde/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRunde(long id)
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            var runde = await _context.Runden.FindAsync(id);
            if (runde == null)
            {
                return NotFound();
            }

            //Runde aus dem Entity-Set entfernen
            _context.Runden.Remove(runde);

            //Änderungen in der Datenbank speichern
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob eine Runde mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die ID der zu überprüfenden Runde.</param>
        /// <returns>True, wenn die Runde existiert; andernfalls False.</returns>
        private bool RundeExists(long id)
        {
            return (_context.Runden?.Any(e => e.RundeId == id)).GetValueOrDefault();
        }
    }
}
