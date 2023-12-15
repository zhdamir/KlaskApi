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

        public RundeController(TurnierContext context)
        {
            _context = context;
        }

        [HttpGet("vorrundenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> VorrundenDetailsHistorie(long turnierId)
        {
            try
            {
                // Get the active turnier
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);

                if (requestedTurnier == null)
                {
                    return NotFound("No  turnier found.");
                }


                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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

                // Get groups, participants, and games for the active turnier
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

        [HttpGet("spielUmDrittenDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> SpielUmDrittenDetailsHistorie(long turnierId)
        {
            try
            {
                // Get the active turnier
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);
                if (requestedTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }
                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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



        [HttpGet("finaleDetailsHistorie/{turnierId}")]
        public async Task<ActionResult<object>> FinaleDetailsHistorie(long turnierId)
        {
            try
            {
                // Get the active turnier
                var requestedTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.Id == turnierId);
                if (requestedTurnier == null)
                {
                    return NotFound("No turnier found.");
                }
                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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




        [HttpGet("finaleDetails")]
        public async Task<ActionResult<object>> FinaleDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);
                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }
                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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

        [HttpGet("spielUmDrittenDetails")]
        public async Task<ActionResult<object>> SpielUmDrittenDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);
                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }
                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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




        [HttpGet("vorrundenResults")]
        public async Task<ActionResult<object>> VorrundenResults()
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

        // Define helper methods to calculate AnzahlSpiele, AnzahlSiege, and SatzDifferenz
        private long GetAnzahlSpiele(long teilnehmerId, long gruppeId, long turnierId)
        {
            try
            {

                // Get groups, participants, and games for the active turnier
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
               .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId && j.TurnierTeilnehmer.GruppeId == gruppeId && j.SpieleTeilnehmer.Punkte != null)
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


        [HttpGet("vorrundenDetails")]
        public async Task<ActionResult<object>> VorrundenDetails()
        {
            try
            {
                // Get the active turnier
                var activeTurnier = await _context.Turniere.FirstOrDefaultAsync(t => t.IsActive);

                if (activeTurnier == null)
                {
                    return NotFound("No active turnier found.");
                }


                // Get groups, participants, and games for the active turnier and second largest RundeId
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
                     /*.Where(j => j.Runde.RundeId == 172) works as expected*/
                     //.Where(j => j.Runde.RundeId == secondLargestRundeId)
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

        /*staring the Finale*/
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


        /*staring the Vorrunde*/
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



        // GET: api/Runde
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Runde>>> GetRunden()
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            return await _context.Runden.ToListAsync();
        }

        // GET: api/Runde/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Runde>> GetRunde(long id)
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

            return runde;
        }

        // PUT: api/Runde/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Runde
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Runde>> PostRunde(Runde runde)
        {
            if (_context.Runden == null)
            {
                return Problem("Entity set 'TurnierContext.Runden'  is null.");
            }
            _context.Runden.Add(runde);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRunde", new { id = runde.RundeId }, runde);
        }

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

            _context.Runden.Remove(runde);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RundeExists(long id)
        {
            return (_context.Runden?.Any(e => e.RundeId == id)).GetValueOrDefault();
        }
    }
}
