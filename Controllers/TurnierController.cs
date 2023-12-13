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

        public TurnierController(TurnierContext context)
        {
            _context = context;
        }
        [HttpGet("selectedTurnierDetails/{turnierId}")]
        public async Task<ActionResult<object>> SelectedTurnierDetails(long turnierId)
        {
            try
            {
                // Get the active turnier
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("Turnier with Id " + turnierId + " not found.");
                }

                // Get groups and participants for the active turnier
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
                            // Add other properties as needed
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


        [HttpGet("gruppenrundenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> GruppenrundenDetailsHistorie(long turnierId)
        {
            try
            {
                // Get the active turnier
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No  turnier found.");
                }

                // Get groups, participants, and games for the active turnier
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
                    AnzahlSpiele = GetAnzahlSpiele(td.TeilnehmerId, td.GruppeId),
                    AnzahlSiege = GetAnzahlSiege(td.TeilnehmerId, td.GruppeId),
                    SatzDifferenz = GetSatzDifferenz(td.TeilnehmerId, td.GruppeId)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GruppenrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }




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

                // Get groups, participants, and games for the active turnier
                var turnierDetails = await _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
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

                var result = turnierDetails.Distinct().Select(td => new
                {
                    GruppeId = td.GruppeId,
                    Gruppenname = td.Gruppenname,
                    TeilnehmerId = td.TeilnehmerId,
                    Vorname = td.Vorname,
                    RundeBezeichnung = td.RundeBezeichnung,
                    AnzahlSpiele = GetAnzahlSpiele(td.TeilnehmerId, td.GruppeId),
                    AnzahlSiege = GetAnzahlSiege(td.TeilnehmerId, td.GruppeId),
                    SatzDifferenz = GetSatzDifferenz(td.TeilnehmerId, td.GruppeId)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GruppenrundenDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // Define helper methods to calculate AnzahlSpiele, AnzahlSiege, and SatzDifferenz
        private long GetAnzahlSpiele(long teilnehmerId, long gruppeId)
        {
            try
            {
                // Count the number of games played by the participant in the group
                /*var anzahlSpiele = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.TurniereTeilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, TurnierTeilnehmer = tt })
                    .Where(j => j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                    .Count();*/

                // Get groups, participants, and games for the active turnier
                var anzahlSpiele = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
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


        private int GetAnzahlSiege(long teilnehmerId, long gruppeId)
        {
            try
            {
                // Count the number of victories by the participant in the group
                /*var anzahlSiege = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, TurnierTeilnehmer = tt })
                    .Where(j => j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                    .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                    .Where(j => j.Opponent.Punkte != null && j.SpieleTeilnehmer.Punkte > j.Opponent.Punkte)
                    .Count();=> Error: counts Siege from all Runde (Gruppenvorrunde, Vorrunde)*/

                var anzahlSiege = _context.SpieleTeilnehmer
               .Where(st => st.TeilnehmerId == teilnehmerId)
               .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
               .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
               .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
               .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
               .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
               .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                /*Count only Spiele from the Gruppenvoorunde*/
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

        private long GetSatzDifferenz(long teilnehmerId, long gruppeId)
        {
            try
            {
                // Calculate the set difference by the participant in the group
                /*var satzDifferenz = _context.SpieleTeilnehmer
                    .Where(st => st.TeilnehmerId == teilnehmerId)
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.TurniereTeilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, TurnierTeilnehmer = tt })
                    .Where(j => j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
                    .Join(_context.SpieleTeilnehmer, j => j.Spiel.SpielId, opp => opp.SpielId, (j, opp) => new { j.SpieleTeilnehmer, Opponent = opp })
                    .Where(j => j.Opponent.Punkte != null)
                    .Sum(j => (j.SpieleTeilnehmer.Punkte - j.Opponent.Punkte).GetValueOrDefault());*/

                var satzDifferenz = _context.SpieleTeilnehmer
                .Where(st => st.TeilnehmerId == teilnehmerId)
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
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



        [HttpGet("gruppenrundenDetails")]
        public async Task<ActionResult<object>> GruppenrundenDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Get groups, participants, and games for the active turnier
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
                    .Join(_context.Gruppen,
                        j => j.TurnierTeilnehmer.GruppeId,
                        g => g.GruppeId,
                        (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden,
                        j => j.Spiel.RundeId,
                        r => r.RundeId,
                        (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                       .Where(j => j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde") && j.Runde.TurnierId == activeTurnier.Id)
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





        [HttpGet("currentTurnierDetails")]
        public async Task<ActionResult<object>> CurrentTurnierDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }

                // Get groups and participants for the active turnier
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
                            // Add other properties as needed
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



        /*staring the Turnier*/
        [HttpPost("startTurnier")]
        //[Route("start")]
        public async Task<IActionResult> StartTurnier([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {
                // Log or return a more specific error response for an invalid turnierId
                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                var groupingService = new TeilnehmerGroupingService(_context);
                var createdGroups = await groupingService.GroupTeilnehmer(turnierId);

                if (createdGroups == null)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Grouping failed. Check your data.");
                }

                // Return success response or createdGroups if needed
                return Ok(createdGroups);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


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

        // POST: api/Turnier
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        private bool TurnierExists(long id)
        {
            return (_context.Turniere?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
