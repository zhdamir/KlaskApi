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

        /// <summary>
        /// Initialisiert eine neue Instanz des StartFinaleService mit dem angegebenen TurnierContext.
        /// </summary>
        /// <param name="context">Der TurnierContext, der für den Zugriff auf die Datenbank verwendet wird.</param>
        public StartSpielUmDrittenService(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generiert und ordnet Teilnehmer für das Finale basierend auf die Leistungen aus der Vorrunde zu.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das das Finale generiert werden soll.</param>
        /// <returns>Ein boolscher Wert, der angibt, ob die Operation erfolgreich war oder nicht.</returns>
        public async Task<bool> SpielUmDrittenTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                // Holt die besten Performer aus der Vorrunde
                var allTeilnehmer = GetVorrundeTeilnehmer(turnierId);

                if (allTeilnehmer == null)
                {
                    Console.WriteLine("Teilnehmer is null!!!!");
                    return false;
                }

                var hashMapTeilnehmer = CalculateSatzDifferenzForVorrunde(allTeilnehmer, turnierId);

                var bestTeilnehmer = GetBestTeilnehmerForSpielUmDritten(hashMapTeilnehmer);

                // Erstellt die Runde für das Finale
                var createdFinale = GenerateFinale(turnierId);

                if (createdFinale == null)
                {
                    Console.WriteLine($"Error creating Finale for TurnierId: {turnierId}");
                    return false;

                }
                // Speichert Änderungen in der Datenbank
                await _context.SaveChangesAsync();


                //Spiele für die finale Runde erstellen
                var rundeId = createdFinale.RundeId;
                var createdFinaleSpiele = GenerateFinaleSpiele(rundeId);
                if (createdFinaleSpiele == null)
                {
                    Console.WriteLine($"Error creating Finale Spiele for RundeId: {rundeId}");
                    return false;
                }

                //Änderungen in der Datenbank speichen
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

        /// <summary>
        /// Erstellt eine neue Runde für das Finale im angegebenen Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, zu dem das Finale gehört.</param>
        /// <returns>Die neu erstellte Runde für das Finale.</returns>
        private Runde GenerateFinale(long turnierId)
        {
            var Finale = new Runde
            {
                RundeBezeichnung = "SpielUmDritten",
                TurnierId = turnierId,
            };
            _context.Runden.Add(Finale);
            // Save changes to the database to ensure SpielId is assigned
            _context.SaveChanges();
            return Finale;
        }

        /// <summary>
        /// Erstellt eine Liste von Spielen für das Finale mit den angegebenen Runden-IDs.
        /// </summary>
        /// <param name="rundeId">Die ID der Runde, zu der die Spiele gehören sollen.</param>
        /// <returns>Eine Liste von neu erstellten Spielen für das Finale.</returns>
        private Spiel GenerateFinaleSpiele(long rundeId)
        {

            var spielUmErstenPlatz = new Spiel
            {
                RundeId = rundeId,
            };

            _context.Spiele.Add(spielUmErstenPlatz);
            // Save changes to the database to ensure SpielId is assigned
            _context.SaveChanges();


            return spielUmErstenPlatz;
        }

        /// <summary>
        /// Ermittelt die besten Teilnehmer der Vorrunde für das angegebene Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die besten Teilnehmer ermittelt werden sollen.</param>
        /// <returns>Eine Liste der besten Teilnehmer der Vorrunde.</returns>

        private List<Teilnehmer> GetVorrundeTeilnehmer(long turnierId)
        {
            try
            {
                var vorrundeTeilnehmerList = _context.SpieleTeilnehmer
                    .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                    .Join(_context.Teilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tn => tn.TeilnehmerId, (j, tn) => new { j.SpieleTeilnehmer, j.Spiel, Teilnehmer = tn })
                    .Join(_context.TurniereTeilnehmer, j => j.Teilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, TurnierTeilnehmer = tt })
                    .Join(_context.Gruppen, j => j.TurnierTeilnehmer.GruppeId, g => g.GruppeId, (j, g) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, Gruppe = g })
                    .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { j.SpieleTeilnehmer, j.Spiel, j.Teilnehmer, j.TurnierTeilnehmer, j.Gruppe, Runde = r })
                    .Where(j => j.Runde.RundeBezeichnung.Contains("Vorrunde") && j.Runde.TurnierId == turnierId)
                    .Select(j => j.Teilnehmer)
                    .Distinct()  // To ensure unique Teilnehmer in the result
                    .ToList();

                return vorrundeTeilnehmerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetVorrundeTeilnehmer: {ex.Message}");
                return null;
            }
        }

        private Dictionary<Teilnehmer, long> CalculateSatzDifferenzForVorrunde(List<Teilnehmer> vorrundeTeilnehmer, long turnierId)
        {
            try
            {
                var satzDifferenzMap = new Dictionary<Teilnehmer, long>();

                foreach (var teilnehmer in vorrundeTeilnehmer)
                {



                    // Find the GruppeId for the given Turnier and Teilnehmer
                    var gruppeId = _context.TurniereTeilnehmer
                        .Where(tt => tt.TurnierId == turnierId && tt.TeilnehmerId == teilnehmer.TeilnehmerId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    // long satzDifferenz = GetSatzDifferenz2(teilnehmer.TeilnehmerId, gruppeId, turnierId);
                    long satzDifferenz = gruppeId.HasValue
                    ? GetSatzDifferenz2(teilnehmer.TeilnehmerId, gruppeId.Value, turnierId)
                    : 0;

                    // Add the Teilnehmer and SatzDifferenz to the Dictionary
                    satzDifferenzMap.Add(teilnehmer, satzDifferenz);
                }

                return satzDifferenzMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateSatzDifferenzForVorrunde: {ex.Message}");
                return null;
            }
        }

        /* private List<Teilnehmer> GetBestTeilnehmerForSpielUmDritten(Dictionary<Teilnehmer, long> satzDifferenzMap)
         {
             try
             {
                 // Order the Teilnehmer based on Satz Diffs in ascending order
                 var orderedTeilnehmer = satzDifferenzMap.OrderBy(pair => pair.Value).ToList();

                 // Take the 3rd and 4th Teilnehmer (index 2 and 3 in a zero-based index)
                 var bestTeilnehmer = orderedTeilnehmer.Skip(2).Take(2).Select(pair => pair.Key).ToList();

                 return bestTeilnehmer;
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Error in GetBestTeilnehmerForSpielUmDritten: {ex.Message}");
                 return null;
             }
         }*/

        private List<Teilnehmer> GetBestTeilnehmerForSpielUmDritten(Dictionary<Teilnehmer, long> satzDifferenzMap)
        {
            try
            {
                // Order the Teilnehmer based on Satz Diffs in ascending order and then by Vorname
                var orderedTeilnehmer = satzDifferenzMap.OrderBy(pair => pair.Value)
                                                       .ThenBy(pair => pair.Key.Vorname)
                                                       .ToList();

                // Take the 3rd and 4th Teilnehmer (index 2 and 3 in a zero-based index)
                var bestTeilnehmer = orderedTeilnehmer.Skip(2).Take(2).Select(pair => pair.Key).ToList();

                return bestTeilnehmer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBestTeilnehmerForSpielUmDritten: {ex.Message}");
                return null;
            }
        }




        private long GetSatzDifferenz2(long teilnehmerId, long gruppeId, long turnierId)
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
        /// Ermittelt die Satzdifferenz für einen bestimmten Teilnehmer in der Vorrunde eines Turniers.
        /// </summary>
        /// <param name="teilnehmerId">Die ID des Teilnehmers, für den die Satzdifferenz ermittelt werden soll.</param>
        /// <param name="turnierId">Die ID des Turniers, zu dem die Vorrunde gehört.</param>
        /// <returns>Die Satzdifferenz des Teilnehmers.</returns>
        private long GetSatzDifferenz(long teilnehmerId, long turnierId)
        {
            try
            {
                //  die Satzdifferenz für den angegebenen Teilnehmer ermitteln
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
                throw;
            }
        }

        /// <summary>
        /// Generiert SpielTeilnehmer für die Finalrunde basierend auf den Teilnehmern und Spielen der Vorrunde.
        /// </summary>
        /// <param name="rundeId">Die ID der Finalrunde.</param>
        /// <param name="teilnehmerList">Die Liste der Teilnehmer, die sich für die Finalrunde qualifiziert haben.</param>
        /// <param name="spieleList">Die Liste der Spiele für die Finalrunde.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Liste der generierten SpielTeilnehmer für die Finalrunde.</returns>
        private List<SpielTeilnehmer> GenerateSpielTeilnehmerFinale(long rundeId, List<Teilnehmer> teilnehmerList, Spiel spieleList, long turnierId)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();

            try
            {
                if (spieleList == null || spieleList.SpielId == 0)
                {
                    Console.WriteLine("Error: Invalid or null SpielId in spieleList.");
                    return null;
                }

                // Order participants by Satzdifferenz in descending order
                // var sortedTeilnehmer = teilnehmerWithScores.OrderByDescending(entry => entry.SatzDifferenz).ToList();

                // Create SpielTeilnehmer entities for the Spiel
                foreach (var teilnehmerEntry in teilnehmerList)
                {
                    var spielTeilnehmer = new SpielTeilnehmer
                    {
                        SpielId = spieleList.SpielId,
                        TeilnehmerId = teilnehmerEntry.TeilnehmerId,
                        Punkte = null,
                    };

                    spieleTeilnehmerList.Add(spielTeilnehmer);
                    _context.SpieleTeilnehmer.Add(spielTeilnehmer);
                    // Save changes to the database to ensure SpielId is assigned
                    _context.SaveChanges();

                }

                _context.SaveChanges();
                return spieleTeilnehmerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateSpielTeilnehmerFinale: {ex.Message}");
                return null;
            }
        }

    }
}

