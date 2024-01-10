using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;

namespace KlaskApi.Services
{
    public class TeilnehmerGroupingService
    {
        private readonly TurnierContext _context;// to interact with database

        /// <summary>
        /// Initialisiert eine neue Instanz des TeilnehmerGroupingService.
        /// </summary>
        /// <param name="context">Der Datenkontext für das Turnier.</param>
        public TeilnehmerGroupingService(TurnierContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gruppiert die Teilnehmer für ein bestimmtes Turnier.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Liste der erstellten Gruppen oder null bei Fehlern.</returns>
        public async Task<List<Gruppe>> GroupTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                // Abrufen der Liste der Turnier-Teilnehmer
                var turnierTeilnehmerList = await _context.TurniereTeilnehmer
                    .Where(tt => tt.TurnierId == turnierId)
                    .ToListAsync();
                Console.WriteLine(turnierTeilnehmerList);
                Console.WriteLine(turnierId);

                // Überprüfen, ob Teilnehmer vorhanden sind
                if (turnierTeilnehmerList == null || turnierTeilnehmerList.Count == 0)
                {
                    Console.WriteLine($"No Teilnehmer found for TurnierId: {turnierId}");
                    return null;
                }

                // Abrufen der Anzahl der Gruppen für das Turnier
                var numberOfGroups = (int)_context.Turniere.FirstOrDefault(t => t.Id == turnierId)?.AnzahlGruppen;
                Console.WriteLine($"Number of Groups HEYY it works until here: {numberOfGroups}" + " Turnier Id is " + turnierId + " AAAND turnierTeilnehmerList is " + turnierTeilnehmerList.Count);

                if (numberOfGroups <= 0)
                {
                    Console.WriteLine($"Invalid AnzahlGruppen for TurnierId: {turnierId}");
                    return null;
                }

                // Gruppen erstellen und zuweisen
                var createdGroups = GenerateAndAssignGroups(numberOfGroups, turnierTeilnehmerList);

                if (createdGroups == null || createdGroups.Count != numberOfGroups)
                {
                    Console.WriteLine($"Error creating groups for TurnierId: {turnierId}");
                    return null;
                }

                // Gruppenvorrunde erstellen
                var createdGruppenRunde = GenerateGruppenRunde(turnierId);
                if (createdGruppenRunde == null)
                {
                    Console.WriteLine($"Error creating Gruppen Vorrunde for TurnierId: {turnierId}");
                    return null;
                }
                await _context.SaveChangesAsync();


                // Spiele für Gruppenrunde erstellen
                var rundeId = createdGruppenRunde.RundeId;
                var createdGruppenSpiele = GenerateGruppenRundeSpiele(rundeId, turnierTeilnehmerList);
                if (createdGruppenSpiele == null)
                {
                    Console.WriteLine($"Error creating Gruppen Spiele for RundeId: {rundeId}");
                    return null;
                }
                await _context.SaveChangesAsync();

                // SpielTeilnehmer für Gruppenrunde erstellen
                var createdSpielTeilnehmer = GenerateSpielTeilnehmer(rundeId, turnierTeilnehmerList, createdGruppenSpiele);
                if (createdSpielTeilnehmer == null)
                {
                    Console.WriteLine($"Error creating SpieleTeilnehmer for RundeId: {rundeId}");
                    return null;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Groups created and assigned successfully");
                return createdGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GroupTeilnehmer: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Methode zum Generieren von SpielTeilnehmer-Entitäten basierend auf Teilnehmern in einer Gruppe.
        /// </summary>
        /// <param name="rundeId">Die ID der Runde, zu der die Spiele gehören.</param>
        /// <param name="teilnehmerList">Die Liste der Turnier-Teilnehmer in der Gruppe.</param>
        /// <param name="spieleList">Die Liste der Spiele für die Gruppenvorrunde.</param>
        /// <returns>Die Liste der erstellten SpielTeilnehmer-Entitäten.</returns>
        private List<SpielTeilnehmer> GenerateSpielTeilnehmer(long rundeId, List<TurnierTeilnehmer> teilnehmerList, List<Spiel> spieleList)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();
            // Eindeutige SpielIds für die aktuelle Gruppe generieren
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();

            // Logik zum Generieren von SpielTeilnehmer-Entitäten basierend auf Teilnehmern in jeder Gruppe
            // Es wird angenommen, dass jeder Teilnehmer in einer Gruppe gegen alle anderen Teilnehmer in derselben Gruppe spielt
            foreach (var gruppeId in teilnehmerList.Select(tt => tt.GruppeId).Distinct())
            {
                var gruppeTeilnehmer = teilnehmerList.Where(tt => tt.GruppeId == gruppeId).ToList();
                for (int i = 0; i < gruppeTeilnehmer.Count - 1; i++)
                {

                    for (int j = i + 1; j < gruppeTeilnehmer.Count; j++)
                    {

                        // Nächste eindeutige SpielId für die aktuelle Gruppe auswählen
                        var spielId = uniqueSpielIds.FirstOrDefault();

                        if (spielId != 0) // Überprüfen, ob eine gültige SpielId vorhanden ist
                        {
                            // SpielTeilnehmer-Entitäten für das Paar erstellen
                            var spielTeilnehmer1 = new SpielTeilnehmer
                            {
                                SpielId = spielId,
                                TeilnehmerId = gruppeTeilnehmer[i].TeilnehmerId,
                                Punkte = null,
                            };

                            var spielTeilnehmer2 = new SpielTeilnehmer
                            {
                                SpielId = spielId,
                                TeilnehmerId = gruppeTeilnehmer[j].TeilnehmerId,
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

        /// <summary>
        /// Methode zum Generieren von Spiel-Entitäten für eine Gruppenrunde basierend auf den Teilnehmern in einer Gruppe.
        /// </summary>
        /// <param name="rundeId">Die ID der Runde, zu der die Spiele gehören.</param>
        /// <param name="teilnehmerList">Die Liste der Turnier-Teilnehmer in der Gruppe.</param>
        /// <returns>Die Liste der erstellten Spiel-Entitäten für die Gruppenrunde.</returns>
        private List<Spiel> GenerateGruppenRundeSpiele(long rundeId, List<TurnierTeilnehmer> teilnehmerList)
        {

            var spieleList = new List<Spiel>();

            // Logik zum Generieren von Spiel-Entitäten basierend auf Teilnehmern in jeder Gruppe
            // jeder Teilnehmer in einer Gruppe spielt gegen alle anderen Teilnehmer in derselben Gruppe 
            foreach (var gruppeId in teilnehmerList.Select(tt => tt.GruppeId).Distinct())
            {
                var gruppeTeilnehmer = teilnehmerList.Where(tt => tt.GruppeId == gruppeId).ToList();

                for (int i = 0; i < gruppeTeilnehmer.Count - 1; i++)
                {
                    for (int j = i + 1; j < gruppeTeilnehmer.Count; j++)
                    {
                        // Spiel-Entität für jedes Paar von Teilnehmern in der Gruppe erstellen
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
        /// Methode zum Erstellen einer Runde für die Gruppenvorrunde.
        /// </summary>
        /// <param name="turnierId">Die ID des Turniers.</param>
        /// <returns>Die erstellte Runde für die Gruppenvorrunde.</returns>
        private Runde GenerateGruppenRunde(long turnierId)
        {
            var gruppenRunde = new Runde
            {
                RundeBezeichnung = "Gruppenvorrunde ",
                TurnierId = turnierId,
            };
            _context.Runden.Add(gruppenRunde);
            return gruppenRunde;
        }

        /// <summary>
        /// Methode zum Generieren und Zuweisen von Gruppen für Teilnehmer.
        /// </summary>
        /// <param name="numberOfGroups">Die Anzahl der zu generierenden Gruppen.</param>
        /// <param name="teilnehmerList">Die Liste der Turnier-Teilnehmer.</param>
        /// <returns>Die Liste der erstellten Gruppen.</returns>
        private List<Gruppe> GenerateAndAssignGroups(int numberOfGroups, List<TurnierTeilnehmer> teilnehmerList)
        {
            var groups = new List<Gruppe>();

            // Erstellen und Hinzufügen von Gruppen zur Tabelle Gruppen
            for (int i = 1; i <= numberOfGroups; i++)
            {
                var gruppe = new Gruppe
                {
                    Gruppenname = $"Gruppe {i}",
                    TurnierId = teilnehmerList.First().TurnierId,
                };

                groups.Add(gruppe);
                _context.Gruppen.Add(gruppe);
            }

            // Änderungen speichern, um die neu erstellten Gruppen zu persistieren
            _context.SaveChanges();

            // Die Teilnehmerliste zufällig mischen
            var shuffledTeilnehmer = teilnehmerList.OrderBy(x => Guid.NewGuid()).ToList();

            // GruppeIds den TurnierTeilnehmer-Instanzen zuweisen
            for (int i = 0; i < shuffledTeilnehmer.Count; i++)
            {
                var gruppe = groups[i % numberOfGroups];
                shuffledTeilnehmer[i].GruppeId = gruppe.GruppeId;
            }

            // Änderungen speichern, um die zugewiesenen GruppeIds zu persistieren
            _context.SaveChanges();

            return groups;
        }

    }
}
