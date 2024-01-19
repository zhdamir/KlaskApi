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
                List<Teilnehmer> bestTeilnehmer = GetBestVorrundeTeilnehmer(turnierId);

                if (bestTeilnehmer == null)
                {
                    Console.WriteLine("Teilnehmer is null!!!!");
                    return false;
                }
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

            return spielUmErstenPlatz;
        }


        /// <summary>
        /// Ermittelt die besten Teilnehmer der Vorrunde für das angegebene Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers, für das die besten Teilnehmer ermittelt werden sollen.</param>
        /// <returns>Eine Liste der besten Teilnehmer der Vorrunde.</returns>
        private List<Teilnehmer> GetBestVorrundeTeilnehmer(long turnierId)
        {
            try
            {
                // Eine Liste der besten Teilnehmer basierend auf der Satzdifferenz ermitteln
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

                // Die besten Teilnehmer aus der Liste der Teilnehmer-IDs ermitteln
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

            // Eindeutige SpielIds generieren
            var uniqueSpielId = spieleList.SpielId;


            // Satzdifferenz für jeden Teilnehmer berechnen
            var teilnehmerWithScores = teilnehmerList.Select(teilnehmer => new
            {
                Teilnehmer = teilnehmer,
                SatzDifferenz = GetSatzDifferenz(teilnehmer.TeilnehmerId, turnierId)
            });

            // Teilnehmer nach Satzdifferenz absteigend sortieren
            var sortedTeilnehmer = teilnehmerWithScores.OrderByDescending(entry => entry.SatzDifferenz).ToList();

            // SpielTeilnehmer-Entitäten für das Spiel um den ersten Platz erstellen


            if (uniqueSpielId != 0)
            {


                foreach (var teilnehmerEntry in sortedTeilnehmer.Take(2))
                {
                    var spielTeilnehmer = new SpielTeilnehmer
                    {
                        SpielId = uniqueSpielId,
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



            return spieleTeilnehmerList;
        }
    }
}
