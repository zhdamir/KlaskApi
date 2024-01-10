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

        /// <summary>
        /// Generiert und weist Teilnehmer der Vorrunde eines Turniers zu.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>True bei Erfolg, andernfalls False.</returns>
        public async Task<bool> VorrundeTeilnehmer(long turnierId)
        {
            try
            {
                //die besten Gruppe Teilnehmer für die Vorrunde holen (die besten 3)
                List<Teilnehmer> bestTeilnehmer = GetBestGroupTeilnehmer(turnierId);

                //Vorrunde erstellen
                var createdVorrunde = GenerateVorrunde(turnierId);
                if (createdVorrunde == null)
                {
                    Console.WriteLine($"Error creating Vorrunde for TurnierId: {turnierId}");

                }
                await _context.SaveChangesAsync();


                //Spiele für Vorrunde erstellen
                var rundeId = createdVorrunde.RundeId;
                var createdVorrundeSpiele = GenerateVorrundeSpiele(rundeId, bestTeilnehmer, turnierId);
                if (createdVorrundeSpiele == null)
                {
                    Console.WriteLine($"Error creating Vorrunde Spiele for RundeId: {rundeId}");

                }
                await _context.SaveChangesAsync();

                // Zuweisen der Teilnehmer zu den Spielen in der Vorrunde
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

        /// <summary>
        /// Methode zum Erstellen der Gruppenvorrunde.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers für die Gruppenvorrunde.</param>
        /// <returns>Das erstellte Runde-Objekt für die Gruppenvorrunde.</returns>
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

        /// <summary>
        /// Methode zum Generieren von Spielen für die Gruppenvorrunde.
        /// </summary>
        /// <param name="rundeId">Die ID der Runde, zu der die Spiele gehören.</param>
        /// <param name="teilnehmerList">Die Liste der Teilnehmer für die Gruppenvorrunde.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Liste der erstellten Spiele für die Gruppenvorrunde.</returns>
        private List<Spiel> GenerateVorrundeSpiele(long rundeId, List<Teilnehmer> teilnehmerList, long turnierId)
        {
            var spieleList = new List<Spiel>();

            // Logik zum Generieren von Spielen basierend auf der Teilnehmerliste aus der Gruppenrunden-Tabelle in jeder Gruppe
            for (int i = 0; i < teilnehmerList.Count - 1; i++)
            {
                for (int j = i + 1; j < teilnehmerList.Count; j++)
                {
                    // GruppeId für jeden Teilnehmer aus TurniereTeilnehmer abrufen
                    var gruppeIdTeilnehmer1 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[i].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    var gruppeIdTeilnehmer2 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[j].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    // Überprüfen, ob Teilnehmer i und j zur gleichen Gruppe gehören
                    if (gruppeIdTeilnehmer1 != gruppeIdTeilnehmer2)
                    {
                        // Ein Spiel für jedes Paar von Teilnehmern in verschiedenen Gruppen erstellen
                        var spiel = new Spiel
                        {
                            RundeId = rundeId,
                        };
                        spieleList.Add(spiel);
                        _context.Spiele.Add(spiel);
                    }
                }
            }

            return spieleList;
        }


        /// <summary>
        /// Methode zum Abrufen der besten Teilnehmergruppen für die Gruppenvorrunde eines Turniers.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Liste der besten Teilnehmer für jede Gruppe.</returns>
        private List<Teilnehmer> GetBestGroupTeilnehmer(long turnierId)
        {
            var bestTeilnehmerList = new List<Teilnehmer>();

            // Abfrage für die SpieleTeilnehmer in der Gruppenvorrunde mit Punkten und Rundenfilter
            var groupedTeilnehmerSatzDifferenz = _context.SpieleTeilnehmer
                .Join(_context.Spiele, st => st.SpielId, s => s.SpielId, (st, s) => new { SpieleTeilnehmer = st, Spiel = s })
                .Join(_context.Runden, j => j.Spiel.RundeId, r => r.RundeId, (j, r) => new { SpieleTeilnehmer = j.SpieleTeilnehmer, Spiel = j.Spiel, Runde = r })
                .Join(_context.TurniereTeilnehmer, j => j.SpieleTeilnehmer.TeilnehmerId, tt => tt.TeilnehmerId, (j, tt) => new { SpieleTeilnehmer = j.SpieleTeilnehmer, Spiel = j.Spiel, Runde = j.Runde, TurnierTeilnehmer = tt })
                .Where(j => j.Runde.TurnierId == turnierId
                            && j.SpieleTeilnehmer.Punkte != null
                            && j.Runde.RundeBezeichnung.Contains("Gruppenvorrunde")
                            && j.TurnierTeilnehmer.TurnierId == turnierId)
                .ToList();

            // Berechnung der Gesamtsatzdifferenz für jede Gruppe und Auswahl der besten 3 Teilnehmer pro Gruppe
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

            // Iteration durch die berechneten besten Teilnehmer und Hinzufügen zur Liste
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


        /// <summary>
        /// Methode zum Generieren von SpielTeilnehmer-Entitäten für die Gruppenvorrunde.
        /// </summary>
        /// <param name="rundeId">Die ID der Runde, zu der die Spiele gehören.</param>
        /// <param name="teilnehmerList">Die Liste der Teilnehmer für die Gruppenvorrunde.</param>
        /// <param name="spieleList">Die Liste der Spiele für die Gruppenvorrunde.</param>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die Liste der erstellten SpielTeilnehmer-Entitäten für die Gruppenvorrunde.</returns>
        private List<SpielTeilnehmer> GenerateSpielTeilnehmerVorrunde(long rundeId, List<Teilnehmer> teilnehmerList, List<Spiel> spieleList, long turnierId)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();

            // Generierung von eindeutigen SpielIds für die aktuelle Gruppe
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

            // Logik zum Generieren von SpielTeilnehmer-Entitäten basierend auf der Teilnehmerliste
            // jeder Teilnehmer in der Vorrunde spielt gegen alle anderen Teilnehmer 
            for (int i = 0; i < teilnehmerList.Count - 1; i++)
            {
                for (int j = i + 1; j < teilnehmerList.Count; j++)
                {
                    // GruppeId für jeden Teilnehmer aus TurniereTeilnehmer abrufen
                    var gruppeIdTeilnehmer1 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[i].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    var gruppeIdTeilnehmer2 = _context.TurniereTeilnehmer
                        .Where(tt => tt.TeilnehmerId == teilnehmerList[j].TeilnehmerId && tt.TurnierId == turnierId)
                        .Select(tt => tt.GruppeId)
                        .FirstOrDefault();

                    // Überprüfen, ob Teilnehmer i und j verschiedenen Gruppen angehören
                    if (gruppeIdTeilnehmer1 != gruppeIdTeilnehmer2)
                    {
                        // Nächste eindeutige SpielId für die aktuelle Gruppe auswählen
                        var spielId = uniqueSpielIds.FirstOrDefault();

                        if (spielId != 0) // Überprüfen, ob eine gültige SpielId vorhanden ist
                        {
                            // SpielTeilnehmer-Entitäten für das Paar erstellen
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

                            // SpielTeilnehmer-Entitäten zur Liste hinzufügen
                            spieleTeilnehmerList.Add(spielTeilnehmer1);
                            spieleTeilnehmerList.Add(spielTeilnehmer2);

                            // Verwendete SpielId aus der Liste entfernen
                            uniqueSpielIds.Remove(spielId);

                            // SpielTeilnehmer-Entitäten zum Kontext hinzufügen
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