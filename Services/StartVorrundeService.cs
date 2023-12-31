using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;

namespace KlaskApi.Services
{
    public class StartVorrundeService
    {
        private readonly TurnierContext _context;// to interact with database

        public StartVorrundeService(TurnierContext context)
        {
            _context = context;
        }

        public async Task<bool> VorrundeTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                List<Teilnehmer> bestTeilnehmer = GetBestGroupTeilnehmer(turnierId);
                //Create Gruppenvorrunde
                var createdVorrunde = GenerateVorrunde(turnierId);
                if (createdVorrunde == null)
                {
                    Console.WriteLine($"Error creating Vorrunde for TurnierId: {turnierId}");

                }
                await _context.SaveChangesAsync();


                //Create Spiele für Vorrunde
                var rundeId = createdVorrunde.RundeId;
                var createdVorrundeSpiele = GenerateVorrundeSpiele(rundeId, bestTeilnehmer, turnierId);
                if (createdVorrundeSpiele == null)
                {
                    Console.WriteLine($"Error creating Vorrunde Spiele for RundeId: {rundeId}");

                }
                await _context.SaveChangesAsync();

                var createdSpielTeilnehmerVorrunde = GenerateSpielTeilnehmerVorrunde(rundeId, bestTeilnehmer, createdVorrundeSpiele, turnierId);
                if (createdSpielTeilnehmerVorrunde == null)
                {
                    Console.WriteLine($"Error creating SpieleTeilnehmer for RundeId: {rundeId}");

                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Groups created and assigned successfully");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Creating Vorrunde Spiele: {ex.Message}");
                return false;
            }
        }

        //Method to create Gruppenvorrunde
        private Runde GenerateVorrunde(long turnierId)
        {
            var Vorrunde = new Runde
            {
                RundeBezeichnung = "Vorrunde",
                TurnierId = turnierId,
            };
            _context.Runden.Add(Vorrunde);
            return Vorrunde;
        }

        private List<Spiel> GenerateVorrundeSpiele(long rundeId, List<Teilnehmer> teilnehmerList, long turnierId)
        {
            var spieleList = new List<Spiel>();

            // Logic to generate Spiele based on the Teilnehmer List retrieved from Gruppenrunden Table in each group

            for (int i = 0; i < teilnehmerList.Count - 1; i++)
            {
                for (int j = i + 1; j < teilnehmerList.Count; j++)
                {
                    // Fetch the GruppeId from TurniereTeilnehmer for each Teilnehmer
                    var gruppeIdTeilnehmer1 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[i].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    var gruppeIdTeilnehmer2 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[j].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    // Check if Teilnehmer i and j belong to the same group
                    if (gruppeIdTeilnehmer1 != gruppeIdTeilnehmer2)
                    {
                        // Create a Spiel for each pair of Teilnehmer in different groups
                        var spiel = new Spiel
                        {
                            RundeId = rundeId,
                            // Add other properties as needed
                        };
                        spieleList.Add(spiel);
                        _context.Spiele.Add(spiel);
                    }
                }
            }

            return spieleList;
        }


        private List<Teilnehmer> GetBestGroupTeilnehmer(long turnierId)
        {
            var bestTeilnehmerList = new List<Teilnehmer>();

            var groupedTeilnehmerSatzDifferenz = _context.SpieleTeilnehmer
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { SpieleTeilnehmer = j.SpieleTeilnehmer, Spiel = j.Spiel, Runde = r })
                .Join(_context.TurniereTeilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { SpieleTeilnehmer = j.SpieleTeilnehmer, Spiel = j.Spiel, Runde = j.Runde, TurnierTeilnehmer = tt })
                .Where(j => j.Runde.TurnierId == turnierId
                            && j.SpieleTeilnehmer.Punkte != null
                            && j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde")
                            && j.TurnierTeilnehmer.TurnierId == turnierId)
                .ToList();

            var groupedTeilnehmerSatzDifferenzCalculation = groupedTeilnehmerSatzDifferenz
                .GroupBy(j => new { GruppeId = j.TurnierTeilnehmer.GruppeId, TeilnehmerId = j.SpieleTeilnehmer.TeilnehmerId })
                .Select(group => new
                {
                    GruppeId = group.Key.GruppeId,
                    TeilnehmerId = group.Key.TeilnehmerId,
                    TotalSatzDifferenz = group.Sum(j => (j.SpieleTeilnehmer.Punkte - groupedTeilnehmerSatzDifferenz
                        .Where(opp => opp.SpieleTeilnehmer.SpielId == j.SpieleTeilnehmer.SpielId && opp.SpieleTeilnehmer.TeilnehmerId != j.SpieleTeilnehmer.TeilnehmerId)
                        .Select(opp => opp.SpieleTeilnehmer.Punkte)
                        .FirstOrDefault()).GetValueOrDefault())
                })
                .GroupBy(entry => entry.GruppeId)
                .SelectMany(group => group.OrderByDescending(entry => entry.TotalSatzDifferenz).Take(3))
                .ToList();

            foreach (var entry in groupedTeilnehmerSatzDifferenzCalculation)
            {
                var teilnehmer = _context.Teilnehmer
                    .Join(_context.TurniereTeilnehmer, tn => tn.TeilnehmerId, tt => tt.TeilnehmerId, (tn, tt) => new { Teilnehmer = tn, TurnierTeilnehmer = tt })
                    .Where(joined => joined.TurnierTeilnehmer.GruppeId == entry.GruppeId && joined.TurnierTeilnehmer.TeilnehmerId == entry.TeilnehmerId)
                    .Select(joined => joined.Teilnehmer)
                    .FirstOrDefault();

                if (teilnehmer != null)
                {
                    bestTeilnehmerList.Add(teilnehmer);
                }
            }

            return bestTeilnehmerList;
        }


        private List<SpielTeilnehmer> GenerateSpielTeilnehmerVorrunde(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList, long turnierId)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();

            // Generate unique SpielIds for the current group
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

            // Logic to generate Spiele based on teilnehmerList
            // Assuming each Teilnehmer plays with all other Teilnehmer in the vorrunde
            for (int i = 0; i < teilnehmerList.Count - 1; i++)
            {
                for (int j = i + 1; j < teilnehmerList.Count; j++)
                {
                    // Fetch the GruppeId from TurniereTeilnehmer for each Teilnehmer
                    var gruppeIdTeilnehmer1 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[i].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    var gruppeIdTeilnehmer2 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[j].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    // Check if Teilnehmer i and j belong to different groups
                    if (gruppeIdTeilnehmer1 != gruppeIdTeilnehmer2)
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
                                TeilnehmerId = teilnehmerList[j].TeilnehmerId,
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
                }
            }

            return spieleTeilnehmerList;
        }



    }
}