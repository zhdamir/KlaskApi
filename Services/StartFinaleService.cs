using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;


namespace KlaskApi.Services
{
    public class StartFinaleService
    {
        private readonly TurnierContext _context;// to interact with database

        public StartFinaleService(TurnierContext context)
        {
            _context = context;
        }

        public async Task<bool> FinaleTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                List<Teilnehmer> bestTeilnehmer = GetBestVorrundeTeilnehmer(turnierId);
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

                var createdSpielTeilnehmerFinale = GenerateSpielTeilnehmerFinale(rundeId, bestTeilnehmer, createdFinaleSpiele);
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
                RundeBezeichnung = "Finale",
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


        /*private List<Teilnehmer> GetBestVorrundeTeilnehmer(long turnierId)
        {
            var teilnehmerIds = _context.SpieleTeilnehmer
                .Join(_context.Spiele,
                    st => st.SpielId,
                    s => s.SpielId,
                    (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Runden,
                    j => j.Spiel.RundeId,
                    r => r.RundeId,
                    (j, r) => new { j.SpieleTeilnehmer, j.Spiel, Runde = r })
                .Join(_context.TurniereTeilnehmer,
                    j => j.SpieleTeilnehmer.TeilnehmerId,
                    tt => tt.TeilnehmerId,
                    (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Runde, TurnierTeilnehmer = tt })
                .Where(j => j.Runde.TurnierId == turnierId && j.Runde.RundeBezeichnung == "Vorrunde")
                .GroupBy(j => j.TurnierTeilnehmer.TeilnehmerId)
                .Select(group => new
                {
                    TeilnehmerId = group.Key,
                    TotalPunkte = group.Sum(j => j.SpieleTeilnehmer.Punkte)
                })
                .OrderByDescending(entry => entry.TotalPunkte)
                .Take(4) // Take top 4 Teilnehmer
                .Select(entry => entry.TeilnehmerId)
                .ToList();

            var bestTeilnehmerList = _context.Teilnehmer
                .Where(teilnehmer => teilnehmerIds.Contains(teilnehmer.TeilnehmerId))
                .ToList();

            return bestTeilnehmerList;
        }*/
        /*private List<Teilnehmer> GetBestVorrundeTeilnehmer(long turnierId)
        {
            var bestTeilnehmerList = new List<Teilnehmer>();

            var teilnehmerIds = _context.SpieleTeilnehmer
                .Join(_context.Spiele,
                    st => st.SpielId,
                    s => s.SpielId,
                    (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Runden,
                    j => j.Spiel.RundeId,
                    r => r.RundeId,
                    (j, r) => new { j.SpieleTeilnehmer, j.Spiel, Runde = r })
                .Join(_context.TurniereTeilnehmer,
                    j => j.SpieleTeilnehmer.TeilnehmerId,
                    tt => tt.TeilnehmerId,
                    (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Runde, TurnierTeilnehmer = tt })
                .Where(j => j.Runde.TurnierId == turnierId && j.Runde.RundeBezeichnung.Contains("Vorrunde"))
                .GroupBy(j => j.TurnierTeilnehmer.TeilnehmerId)
                .Select(group => new
                {
                    TeilnehmerId = group.Key,
                    TotalPunkte = group.Sum(j => j.SpieleTeilnehmer.Punkte)
                })
                .OrderByDescending(entry => entry.TotalPunkte)
                .Take(4) // Take top 4 Teilnehmer
                .Select(entry => entry.TeilnehmerId)
                .ToList();

            foreach (var teilnehmerId in teilnehmerIds)
            {
                var teilnehmer = _context.Teilnehmer
                    .Where(tn => tn.TeilnehmerId == teilnehmerId)
                    .FirstOrDefault();

                if (teilnehmer != null)
                {
                    bestTeilnehmerList.Add(teilnehmer);
                }
            }

            return bestTeilnehmerList;
        }*/

        private List<Teilnehmer> GetBestVorrundeTeilnehmer(long turnierId)
        {
            try
            {
                var bestTeilnehmerList = _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde"))
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
                            .Where(innerJ => innerJ.InnerRunde.RundeBezeichnung.Contains("Vorrunde") && innerJ.SpieleTeilnehmer.Punkte != null)
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
                    .Take(4)
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



        private long GetSatzDifferenz(long teilnehmerId)
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
                .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.SpieleTeilnehmer.Punkte != null)
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

        private List<SpielTeilnehmer> GenerateSpielTeilnehmerFinale(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList)
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
                SatzDifferenz = GetSatzDifferenz(teilnehmer.TeilnehmerId)
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



        /* private List<SpielTeilnehmer> GenerateSpielTeilnehmerFinale(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList)
         {
             var spieleTeilnehmerList = new List<SpielTeilnehmer>();

             // Generate unique SpielIds for the current group
             var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

             // Logic to generate Spiele based on teilnehmerList
             // Assuming each pair of Teilnehmer plays in the Finale
             for (int i = 0; i < teilnehmerList.Count - 1; i += 2)
             {
                 // Pick the next unique SpielId for the current group
                 var spielId = uniqueSpielIds.FirstOrDefault();

                 if (spielId != 0) // Check if there's a valid SpielId
                 {
                     // Create SpielTeilnehmer entities for the pair
                     var spielTeilnehmer1 = new SpielTeilnehmer
                     {
                         SpielId = spielId,
                         TeilnehmerId = teilnehmerList[i].TeilnehmerId,
                         Punkte = null,
                     };

                     var spielTeilnehmer2 = new SpielTeilnehmer
                     {
                         SpielId = spielId,
                         TeilnehmerId = teilnehmerList[i + 1].TeilnehmerId,
                         Punkte = null,
                     };

                     // Add the SpielTeilnehmer entities to the list
                     spieleTeilnehmerList.Add(spielTeilnehmer1);
                     spieleTeilnehmerList.Add(spielTeilnehmer2);

                     // Remove the used SpielId from the list
                     uniqueSpielIds.Remove(spielId);

                     // Add SpielTeilnehmer entities to the context
                     _context.SpieleTeilnehmer.Add(spielTeilnehmer1);
                     _context.SpieleTeilnehmer.Add(spielTeilnehmer2);
                 }
             }

             return spieleTeilnehmerList;
         }*/



        /*private List<SpielTeilnehmer> GenerateSpielTeilnehmerFinale(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();

            // Generate unique SpielIds for the current group
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

            Console.WriteLine($"Unique SpielIds: {string.Join(", ", uniqueSpielIds)}");

            // Calculate Sätze Differenz for each Teilnehmer
            var teilnehmerWithScores = teilnehmerList.Select(teilnehmer => new
            {
                Teilnehmer = teilnehmer,
                SatzDifferenz = GetSatzDifferenz(teilnehmer.TeilnehmerId)
            });

            // Sort Teilnehmer by Sätze Differenz in descending order
            var sortedTeilnehmer = teilnehmerWithScores.OrderByDescending(entry => entry.SatzDifferenz).ToList();

            // Select the top two Teilnehmer for "um ersten Platz"
            var umErstenPlatzTeilnehmer = sortedTeilnehmer.Take(2);

            // Select the remaining two Teilnehmer for "um dritten Platz"
            var umDrittenPlatzTeilnehmer = sortedTeilnehmer.Skip(2).Take(2);

            // Pick the next unique SpielId for the best two Teilnehmer
            var firstSpielId = uniqueSpielIds.FirstOrDefault();

            // Pick the next unique SpielId for the next two Teilnehmer
            var secondSpielId = uniqueSpielIds.Skip(1).FirstOrDefault();

            foreach (var teilnehmerEntry in umErstenPlatzTeilnehmer.Concat(umDrittenPlatzTeilnehmer))
            {
                // Pick the appropriate SpielId for the current group
                var spielId = teilnehmerEntry == umErstenPlatzTeilnehmer.First() ? firstSpielId : secondSpielId;

                if (spielId != 0) // Check if there's a valid SpielId
                {
                    // Create SpielTeilnehmer entity for the Teilnehmer
                    var spielTeilnehmer = new SpielTeilnehmer
                    {
                        SpielId = spielId,
                        TeilnehmerId = teilnehmerEntry.Teilnehmer.TeilnehmerId,
                        Punkte = null,
                    };

                    // Add the SpielTeilnehmer entity to the list
                    spieleTeilnehmerList.Add(spielTeilnehmer);

                    // Remove the used SpielId from the list
                    uniqueSpielIds.Remove(spielId);

                    // Add SpielTeilnehmer entity to the context
                    _context.SpieleTeilnehmer.Add(spielTeilnehmer);

                    Console.WriteLine($"Created SpielTeilnehmer: SpielId = {spielTeilnehmer.SpielId}, TeilnehmerId = {spielTeilnehmer.TeilnehmerId}");
                }
                else
                {
                    Console.WriteLine("Error: No valid SpielId found.");
                    // Add additional logging or throw an exception to signal an error
                    return null; // or handle this case accordingly
                }
            }

            return spieleTeilnehmerList;
        }*/


    }
}
