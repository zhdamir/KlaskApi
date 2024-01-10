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
    public class TurnierController : ControllerBase
    {
        private readonly TurnierContext _context;

        /// <summary>
        /// KOnstruktor Initialisiert eine neue Instanz des TurnierControllers mit dem angegebenen Kontext.
        /// </summary>
        /// <param name="context">Der Kontext für die Turnierdatenbank.</param>
        public TurnierController(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gibt Details zu einem ausgewählten Turnier basierend auf der angegebenen Turnier-ID zurück.
        /// </summary>
        /// <param name="turnierId">Die ID des ausgewählten Turniers.</param>
        /// <returns>Details zum ausgewählten Turnier, einschließlich Gruppen und Teilnehmer.</returns>
        [HttpGet("selectedTurnierDetails/{turnierId}")]
        public async Task<ActionResult<object>> SelectedTurnierDetails(long turnierId)
        {
            try
            {
                // Das angeforderte Turnier abrufen
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("Turnier with Id " + turnierId + " not found.");
                }

                // Gruppen und Teilnehmer für das ausgewählte Turnier abrufen
                var turnierDetails = await _context.Gruppen
                    .Join(_context.TurniereTeilnehmer,
                        g => g.GruppeId,
                        tt => tt.GruppeId,
                        (g, tt) => new { Gruppe = g, TurnierTeilnehmer = tt })
                    .Join(_context.Teilnehmer,
                        j => j.TurnierTeilnehmer.TeilnehmerId,
                        t => t.TeilnehmerId,
                        (j, t) => new { j.Gruppe, j.TurnierTeilnehmer, Teilnehmer = t })
                    .Where(j => j.Gruppe.TurnierId == turnierId)
                    .Select(j => new
                    {
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        Teilnehmer = new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,

                        }
                    })
                    .GroupBy(j => new { j.GruppeId, j.Gruppenname })
                    .Select(g => new
                    {
                        GruppeId = g.Key.GruppeId,
                        Gruppenname = g.Key.Gruppenname,
                        Teilnehmer = g.Select(j => j.Teilnehmer).ToList()
                    })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gibt Historie-Details zu den Gruppenrunden eines ausgewählten Turniers basierend auf der angegebenen Turnier-ID zurück.
        /// </summary>
        /// <param name="turnierId">Die ID des ausgewählten Turniers.</param>
        /// <returns>Historie-Details zu den Gruppenrunden des ausgewählten Turniers, einschließlich Gruppen, Teilnehmer und Spielen.</returns>
        [HttpGet("gruppenrundenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> GruppenrundenDetailsHistorie(long turnierId)
        {
            try
            {
                // Das angeforderte Turnier abrufen
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No  turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das ausgewählte Turnier abrufen
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
                       .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == turnierId)
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
                Console.WriteLine($"Error in CurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("gruppenrundenResultsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> GruppenrundenResultsHistorie(long turnierId)
        {
            try
            {
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No  turnier found.");
                }

                // Get groups, participants, and games for the active turnier
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                      .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == turnierId)
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
        /// Gibt Historie-Ergebnisse zu den Gruppenrunden eines ausgewählten Turniers basierend auf der angegebenen Turnier-ID zurück.
        /// </summary>
        /// <param name="turnierId">Die ID des ausgewählten Turniers.</param>
        /// <returns>Historie-Ergebnisse zu den Gruppenrunden des ausgewählten Turniers, einschließlich Gruppen, Teilnehmer und Spielen.</returns>
        [HttpGet("gruppenrundenResults")]
        public async Task<ActionResult<object>> GruppenrundenResults()
        {
            try
            {
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das ausgewählte Turnier abrufen
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                      .Where(j => j.TurnierTeilnehmer.TurnierId == activeTurnier.Id)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == activeTurnier.Id)
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

                // Ergebnisse für jede Teilnehmer-Gruppenkombination abrufen
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
        /// Berechnet die Anzahl der Spiele für einen bestimmten Teilnehmer in einer bestimmten Gruppe und einem bestimmten Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Anzahl der Spiele für den angegebenen Teilnehmer, die Gruppe und das Turnier.</returns>
        private long GetAnzahlSpiele(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {

                // Spiele, Teilnehmer und Gruppen für das ausgewählte Turnier abrufen
                var anzahlSpiele = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                     .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                    .Count();

                return anzahlSpiele;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAnzahlSpiele: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Berechnet die Anzahl der Siege für einen bestimmten Teilnehmer in einer bestimmten Gruppe und einem bestimmten Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Anzahl der Siege für den angegebenen Teilnehmer, die Gruppe und das Turnier.</returns>
        private int GetAnzahlSiege(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {
                var anzahlSiege = _context.SpieleTeilnehmer
               .Where(st => st.TeilnehmerId == teilnehmerId)
               .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
               .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
               .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                 .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
               .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
               .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
               /*Nur Spiele aus der Gruppenvorrunde zählen*/
               .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                .Where(j => j.Opponent.Punkte != null && j.SpieleTeilnehmer.Punkte > j.Opponent.Punkte)
               .Count();

                return anzahlSiege;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAnzahlSiege: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Berechnet die Satzdifferenz für einen bestimmten Teilnehmer in einer bestimmten Gruppe und einem bestimmten Turnier.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers.</param>
        /// <param name="gruppeId">Die ID der Gruppe.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Satzdifferenz für den angegebenen Teilnehmer, die Gruppe und das Turnier.</returns>
        private long GetSatzDifferenz(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {
                var satzDifferenz = _context.SpieleTeilnehmer
                .Where(st => st.TeilnehmerId == teilnehmerId)
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                  .Where(j => j.TurnierTeilnehmer.TurnierId == turnierId)
                .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                .Where(j => j.Opponent.Punkte != null)
                 .Sum(j => (j.SpieleTeilnehmer.Punkte - j.Opponent.Punkte).GetValueOrDefault());

                return satzDifferenz;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSatzDifferenz: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Gibt die Details der Gruppenrunden für das aktive Turnier zurück.
        /// </summary>
        /// <returns>Die Details der Gruppenrunden für das aktive Turnier.</returns>
        [HttpGet("gruppenrundenDetails")]
        public async Task<ActionResult<object>> GruppenrundenDetails()
        {
            try
            {
                // Das aktive Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Gruppen, Teilnehmer und Spiele für das aktive Turnier abrufen
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
                       .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde")
                                    && j.Runde.TurnierId == activeTurnier.Id && j.Runde.TurnierId == activeTurnier.Id)
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
                Console.WriteLine($"Error in CurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Gibt die Details des aktuellen Turniers zurück.
        /// </summary>
        /// <returns>Die Details des aktuellen Turniers.</returns>
        [HttpGet("currentTurnierDetails")]
        public async Task<ActionResult<object>> CurrentTurnierDetails()
        {
            try
            {
                // Das aktive Turnier abrufen
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Gruppen und Teilnehmer für das aktive Turnier abrufen
                var turnierDetails = await _context.Gruppen
                    .Join(_context.TurniereTeilnehmer,
                        g => g.GruppeId,
                        tt => tt.GruppeId,
                        (g, tt) => new { Gruppe = g, TurnierTeilnehmer = tt })
                    .Join(_context.Teilnehmer,
                        j => j.TurnierTeilnehmer.TeilnehmerId,
                        t => t.TeilnehmerId,
                        (j, t) => new { j.Gruppe, j.TurnierTeilnehmer, Teilnehmer = t })
                    .Where(j => j.Gruppe.TurnierId == activeTurnier.Id)
                    .Select(j => new
                    {
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        Teilnehmer = new
                        {
                            TeilnehmerId = j.Teilnehmer.TeilnehmerId,
                            Vorname = j.Teilnehmer.Vorname,
                        }
                    })
                    .GroupBy(j => new { j.GruppeId, j.Gruppenname })
                    .Select(g => new
                    {
                        GruppeId = g.Key.GruppeId,
                        Gruppenname = g.Key.Gruppenname,
                        Teilnehmer = g.Select(j => j.Teilnehmer).ToList()
                    })
                    .ToListAsync();

                return turnierDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentTurnierDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        /// <summary>
        /// Startet ein Turnier durch Gruppierung der Teilnehmer.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, das gestartet werden soll.</param>
        /// <returns>HTTP-Statuscode und ggf. erstellte Gruppen.</returns>
        [HttpPost("startTurnier")]
        public async Task<IActionResult> StartTurnier([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {

                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                // TeilnehmerGroupingService Klasse initizialisieren
                var groupingService = new TeilnehmerGroupingService(_context);
                var createdGroups = await groupingService.GroupTeilnehmer(turnierId);

                if (createdGroups == null)
                {

                    return BadRequest("Grouping failed. Check your data.");
                }


                return Ok(createdGroups);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ruft alle Turniere ab.
        /// </summary>
        /// <returns>Eine Liste der abgerufenen Turniere oder NotFound, falls keine vorhanden sind.</returns>
        // GET: api/Turnier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Turnier>>> GetTurniere()
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            return await _context.Turniere.ToListAsync();
        }


        /// <summary>
        /// Ruft ein einzelnes Turnier anhand der angegebenen ID ab.
        /// </summary>
        /// <param name="id">Die ID des abzurufenden Turniers.</param>
        /// <returns>Das abgerufene Turnier oder NotFound, falls nicht gefunden.</returns>
        // GET: api/Turnier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Turnier>> GetTurnier(long id)
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            var turnier = await _context.Turniere.FindAsync(id);

            if (turnier == null)
            {
                return NotFound();
            }

            return turnier;
        }

        /// <summary>
        /// Aktualisiert ein vorhandenes Turnier anhand der angegebenen ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Turniers.</param>
        /// <param name="turnier">Die aktualisierten Informationen für das Turnier.</param>
        /// <returns>NoContent bei erfolgreicher Aktualisierung, BadRequest bei ungültigen Eingaben und NotFound, falls das Turnier nicht gefunden wurde.</returns>
        // PUT: api/Turnier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurnier(long id, Turnier turnier)
        {
            if (id != turnier.Id)
            {
                return BadRequest();
            }

            // Convert DateTime values to UTC
            turnier.StartDatum = turnier.StartDatum.ToUniversalTime();
            turnier.EndDatum = turnier.EndDatum.ToUniversalTime();

            _context.Entry(turnier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnierExists(id))
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
        /// Erstellt ein neues Turnier.
        /// </summary>
        /// <param name="turnier">Die Informationen für das neue Turnier.</param>
        /// <returns>CreatedAtAction mit den erstellten Turnierinformationen bei erfolgreicher Erstellung oder Problem mit einer entsprechenden Meldung bei Fehlern.</returns>
        // POST: api/Turnier
        [HttpPost]
        public async Task<ActionResult<Turnier>> PostTurnier(Turnier turnier)
        {
            if (_context.Turniere == null)
            {
                return Problem("Entity set 'TurnierContext.Turniere'  is null.");
            }

            // Convert DateTime values to UTC
            turnier.StartDatum = turnier.StartDatum.ToUniversalTime();
            turnier.EndDatum = turnier.EndDatum.ToUniversalTime();

            _context.Turniere.Add(turnier);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTurnier", new { id = turnier.Id }, turnier);
        }

        /// <summary>
        /// Löscht ein vorhandenes Turnier anhand der Turnier-ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Turniers.</param>
        /// <returns>NoContent bei erfolgreicher Löschung, NotFound falls das Turnier nicht gefunden wurde.</returns>
        // DELETE: api/Turnier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurnier(long id)
        {
            if (_context.Turniere == null)
            {
                return NotFound();
            }
            var turnier = await _context.Turniere.FindAsync(id);
            if (turnier == null)
            {
                return NotFound();
            }

            _context.Turniere.Remove(turnier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Überprüft, ob ein Turnier mit der angegebenen ID existiert.
        /// </summary>
        /// <param name="id">Die ID des zu überprüfenden Turniers.</param>
        /// <returns>True, wenn das Turnier existiert; andernfalls False.</returns>
        private bool TurnierExists(long id)
        {
            return (_context.Turniere?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
