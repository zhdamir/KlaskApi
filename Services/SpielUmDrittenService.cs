using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;


namespace KlaskApi.Services
{
    public class StartSpielUmDrittenService
    {
        private readonly TurnierContext _context;// to interact with database

        public StartSpielUmDrittenService(TurnierContext context)
        {
            _context = context;
        }

        public async Task<bool> SpielUmDrittenTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                List<Teilnehmer> bestTeilnehmer = GetSecondBestVorrundeTeilnehmer(turnierId);
                Console.WriteLine("Heeeeeey teilneeeeeeehmeeeer! " + bestTeilnehmer.ToString() + "The number of Teilnehmer is: " + bestTeilnehmer.Count + " persons");


                if (bestTeilnehmer == null)
                {
                    Console.WriteLine("Teilnehmer is null!!!!");
                    return false;
                }
                //Create Gruppenvorrunde
                var createdFinale = GenerateFinale(turnierId);

                if (createdFinale == null)
                {
                    Console.WriteLine($"Error creating Finale for TurnierId: {turnierId}");
                    return false;

                }
                await _context.SaveChangesAsync();


                //Create Spiele für Vorrunde
                var rundeId = createdFinale.RundeId;
                var createdFinaleSpiele = GenerateFinaleSpiele(rundeId);
                if (createdFinaleSpiele == null)
                {
                    Console.WriteLine($"Error creating Finale Spiele for RundeId: {rundeId}");
                    return false;
                }
                await _context.SaveChangesAsync();

                var createdSpielTeilnehmerFinale = GenerateSpielTeilnehmerFinale(rundeId, bestTeilnehmer, createdFinaleSpiele, turnierId);
                if (createdSpielTeilnehmerFinale == null)
                {
                    Console.WriteLine($"Error creating SpieleTeilnehmer for RundeId: {rundeId}");
                    return false;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Finale Spiel created and assigned successfully");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Creating Finale Spiele: {ex.Message}");
                return false;
            }
        }

        //Method to create Gruppenvorrunde
        private Runde GenerateFinale(long turnierId)
        {
            var Finale = new Runde
            {
                RundeBezeichnung = "SpielUmDritten",
                TurnierId = turnierId,
            };
            _context.Runden.Add(Finale);
            return Finale;
        }

        private List<Spiel> GenerateFinaleSpiele(long rundeId)
        {
            var spieleList = new List<Spiel>();

            // Create Spiele for "Spiel um dritten Platz" and "Spiel um ersten Platz"
            var spielUmDrittenPlatz = new Spiel
            {
                RundeId = rundeId,
            };

            spieleList.Add(spielUmDrittenPlatz);
            _context.Spiele.Add(spielUmDrittenPlatz);

            var spielUmErstenPlatz = new Spiel
            {
                RundeId = rundeId,
            };
            spieleList.Add(spielUmErstenPlatz);
            _context.Spiele.Add(spielUmErstenPlatz);

            return spieleList;
        }



        private List<Teilnehmer> GetSecondBestVorrundeTeilnehmer(long turnierId)
        {
            try
            {
                var bestTeilnehmerList = _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId)
                    .Join(_context.Turniere, j => j.Runde.TurnierId, t => t.Id, (j, t) => new
                    {
                        Teilnehmer = j.Teilnehmer,
                        GruppeId = j.Gruppe.GruppeId,
                        Gruppenname = j.Gruppe.Gruppenname,
                        RundeBezeichnung = j.Runde.RundeBezeichnung,
                        SatzDifferenz = _context.SpieleTeilnehmer
                            .Where(st => st.TeilnehmerId == j.Teilnehmer.TeilnehmerId)
                            .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                            .Join(_context.Teilnehmer, innerJ => innerJ.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (innerJ, tn) => new { innerJ.SpieleTeilnehmer, innerJ.Spiel, Teilnehmer = tn })
                            .Join(_context.TurniereTeilnehmer, innerJ => innerJ.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (innerJ, tt) => new { innerJ.SpieleTeilnehmer, innerJ.Spiel, innerJ.Teilnehmer, TurnierTeilnehmer = tt })
                            .Join(_context.Gruppen, innerJ => innerJ.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (innerJ, g) => new { innerJ.SpieleTeilnehmer, innerJ.Spiel, innerJ.Teilnehmer, innerJ.TurnierTeilnehmer, Gruppe = g })
                            .Join(_context.Runden, innerJ => innerJ.Spiel.RundeId, innerR => innerR.RundeId, (innerJ, innerR) => new { innerJ.SpieleTeilnehmer, innerJ.Spiel, innerJ.Teilnehmer, innerJ.TurnierTeilnehmer, innerJ.Gruppe, InnerRunde = innerR })
                            .Where(innerJ => innerJ.InnerRunde.RundeBezeichnung.Contains("Vorrunde") && innerJ.InnerRunde.TurnierId == turnierId && innerJ.SpieleTeilnehmer.Punkte != null)
                            .Join(_context.SpieleTeilnehmer, innerJ => innerJ.Spiel.SpielId, opp => opp.SpielId, (innerJ, opp) => new { innerJ.SpieleTeilnehmer, Opponent = opp })
                            .Where(innerJ => innerJ.Opponent.Punkte != null)
                            .Sum(innerJ => (innerJ.SpieleTeilnehmer.Punkte - innerJ.Opponent.Punkte).GetValueOrDefault())
                    })
                    .GroupBy(j => j.Teilnehmer.TeilnehmerId)
                    .Select(group => new
                    {
                        TeilnehmerId = group.Key,
                        BestSatzDifferenz = group.Max(j => j.SatzDifferenz)
                    })
                    .OrderByDescending(entry => entry.BestSatzDifferenz)
                    .Skip(2)
                    .Take(2)
                    .Select(entry => entry.TeilnehmerId)
                    .ToList();
                var bestTeilnehmer = _context.Teilnehmer
                    .Where(tn => bestTeilnehmerList.Contains(tn.TeilnehmerId))
                    .ToList();
                return bestTeilnehmer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GruppenrundenDetails: {ex.Message}");
                return null;
            }
        }



        private long GetSatzDifferenz(long teilnehmerId, long turnierId)
        {
            try
            {
                var satzDifferenz = _context.SpieleTeilnehmer
                .Where(st => st.TeilnehmerId == teilnehmerId)
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId && j.SpieleTeilnehmer.Punkte != null)
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

        private List<SpielTeilnehmer> GenerateSpielTeilnehmerFinale(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList, long turnierId)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();

            // Generate unique SpielIds for the current group
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

            // Check if there are enough unique SpielIds
            if (uniqueSpielIds.Count < 2)
            {
                Console.WriteLine("Error: Not enough unique SpielIds for matchups.");
                return null;
            }

            // Calculate SatzDifferenz for each Teilnehmer
            var teilnehmerWithScores = teilnehmerList.Select(teilnehmer => new
            {
                Teilnehmer = teilnehmer,
                SatzDifferenz = GetSatzDifferenz(teilnehmer.TeilnehmerId, turnierId)
            });

            // Sort Teilnehmer by SatzDifferenz in descending order
            var sortedTeilnehmer = teilnehmerWithScores.OrderByDescending(entry => entry.SatzDifferenz).ToList();

            // Create SpielTeilnehmer entities for first place matchup
            var firstPlaceSpielId = uniqueSpielIds.FirstOrDefault();

            if (firstPlaceSpielId != 0)
            {
                uniqueSpielIds.Remove(firstPlaceSpielId);

                foreach (var teilnehmerEntry in sortedTeilnehmer.Take(2))
                {
                    var spielTeilnehmer = new SpielTeilnehmer
                    {
                        SpielId = firstPlaceSpielId,
                        TeilnehmerId = teilnehmerEntry.Teilnehmer.TeilnehmerId,
                        Punkte = null,
                    };

                    spieleTeilnehmerList.Add(spielTeilnehmer);
                    _context.SpieleTeilnehmer.Add(spielTeilnehmer);

                    Console.WriteLine($"Created SpielTeilnehmer for first place matchup: SpielId = {spielTeilnehmer.SpielId}, TeilnehmerId = {spielTeilnehmer.TeilnehmerId}");
                }
            }
            else
            {
                Console.WriteLine("Error: No valid SpielId found for first place matchup.");
                return null;
            }

            // Create SpielTeilnehmer entities for third place matchup
            var thirdPlaceSpielId = uniqueSpielIds.FirstOrDefault();

            if (thirdPlaceSpielId != 0)
            {
                uniqueSpielIds.Remove(thirdPlaceSpielId);

                foreach (var teilnehmerEntry in sortedTeilnehmer.Skip(2).Take(2))
                {
                    var spielTeilnehmer = new SpielTeilnehmer
                    {
                        SpielId = thirdPlaceSpielId,
                        TeilnehmerId = teilnehmerEntry.Teilnehmer.TeilnehmerId,
                        Punkte = null,
                    };

                    spieleTeilnehmerList.Add(spielTeilnehmer);
                    _context.SpieleTeilnehmer.Add(spielTeilnehmer);

                    Console.WriteLine($"Created SpielTeilnehmer for third place matchup: SpielId = {spielTeilnehmer.SpielId}, TeilnehmerId = {spielTeilnehmer.TeilnehmerId}");
                }
            }
            else
            {
                Console.WriteLine("Error: No valid SpielId found for third place matchup.");
                return null;
            }

            return spieleTeilnehmerList;
        }
    }
}
