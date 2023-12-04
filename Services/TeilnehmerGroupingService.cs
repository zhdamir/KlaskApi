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

        public TeilnehmerGroupingService(TurnierContext context)
        {
            _context = context;
        }
        public async Task<List<Gruppe>> GroupTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                var turnierTeilnehmerList = await _context.TurniereTeilnehmer
                    .Where(tt => tt.TurnierId == turnierId)
                    .ToListAsync();
                Console.WriteLine(turnierTeilnehmerList);
                Console.WriteLine(turnierId);

                if (turnierTeilnehmerList == null || turnierTeilnehmerList.Count == 0)
                {
                    Console.WriteLine($"No Teilnehmer found for TurnierId: {turnierId}");
                    return null;
                }

                var numberOfGroups = (int)_context.Turniere.FirstOrDefault(t => t.Id == turnierId)?.AnzahlGruppen;
                Console.WriteLine($"Number of Groups HEYY it works until here: {numberOfGroups}" + " Turnier Id is " + turnierId + " AAAND turnierTeilnehmerList is " + turnierTeilnehmerList.Count);

                if (numberOfGroups <= 0)
                {
                    Console.WriteLine($"Invalid AnzahlGruppen for TurnierId: {turnierId}");
                    return null;
                }

                var createdGroups = GenerateAndAssignGroups(numberOfGroups, turnierTeilnehmerList);

                if (createdGroups == null || createdGroups.Count != numberOfGroups)
                {
                    Console.WriteLine($"Error creating groups for TurnierId: {turnierId}");
                    return null;
                }

                //Create Gruppenvorrunde
                var createdGruppenRunde = GenerateGruppenRunde(turnierId);
                if (createdGruppenRunde == null)
                {
                    Console.WriteLine($"Error creating Gruppen Vorrunde for TurnierId: {turnierId}");
                    return null;
                }
                await _context.SaveChangesAsync();


                //Create Spiele für Gruppenrunde
                var rundeId = createdGruppenRunde.RundeId;
                var createdGruppenSpiele = GenerateGruppenRundeSpiele(rundeId, turnierTeilnehmerList);
                if (createdGruppenSpiele == null)
                {
                    Console.WriteLine($"Error creating Gruppen Spiele for RundeId: {rundeId}");
                    return null;
                }
                await _context.SaveChangesAsync();

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

        private List<SpielTeilnehmer> GenerateSpielTeilnehmer(long rundeId, List<TurnierTeilnehmer> teilnehmerList, List<Spiel> spieleList)
        {
            var spieleTeilnehmerList = new List<SpielTeilnehmer>();
            // Generate unique SpielIds for the current group
            var uniqueSpielIds = spieleList.Where(s => s.RundeId == rundeId).Select(s => s.SpielId).Distinct().ToList();
            // Logic to generate Spiele based on Teilnehmer in each group
            // Assuming each Teilnehmer in a group plays with all other Teilnehmer in the same group
            foreach (var gruppeId in teilnehmerList.Select(tt => tt.GruppeId).Distinct())
            {
                var gruppeTeilnehmer = teilnehmerList.Where(tt => tt.GruppeId == gruppeId).ToList();
                for (int i = 0; i < gruppeTeilnehmer.Count - 1; i++)
                {

                    for (int j = i + 1; j < gruppeTeilnehmer.Count; j++)
                    {

                        // Pick the next unique SpielId for the current group
                        var spielId = uniqueSpielIds.FirstOrDefault();

                        if (spielId != 0) // Check if there's a valid SpielId
                        {
                            // Create SpielTeilnehmer entities for the pair
                            var spielTeilnehmer1 = new SpielTeilnehmer
                            {
                                SpielId = spielId,
                                TeilnehmerId = gruppeTeilnehmer[i].TeilnehmerId,
                                Punkte = 0, // You can set initial points here
                            };

                            var spielTeilnehmer2 = new SpielTeilnehmer
                            {
                                SpielId = spielId,
                                TeilnehmerId = gruppeTeilnehmer[j].TeilnehmerId,
                                Punkte = 0, // You can set initial points here
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

        private List<Spiel> GenerateGruppenRundeSpiele(long rundeId, List<TurnierTeilnehmer> teilnehmerList)
        {

            var spieleList = new List<Spiel>();

            // Logic to generate Spiele based on Teilnehmer in each group
            // Assuming each Teilnehmer in a group plays with all other Teilnehmer in the same group
            foreach (var gruppeId in teilnehmerList.Select(tt => tt.GruppeId).Distinct())
            {
                var gruppeTeilnehmer = teilnehmerList.Where(tt => tt.GruppeId == gruppeId).ToList();

                for (int i = 0; i < gruppeTeilnehmer.Count - 1; i++)
                {
                    for (int j = i + 1; j < gruppeTeilnehmer.Count; j++)
                    {
                        // Create a Spiel for each pair of Teilnehmer in the group
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

        //Method to create Gruppenvorrunde
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

        //private List<SpielTeilnehmer> GenerateGruppenrundeGames()
        private List<Gruppe> GenerateAndAssignGroups(int numberOfGroups, List<TurnierTeilnehmer> teilnehmerList)
        {
            var groups = new List<Gruppe>();

            // Create and add groups to the Gruppen table
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

            // Save changes to persist the newly created groups
            _context.SaveChanges();

            // Shuffle the teilnehmerList randomly
            var shuffledTeilnehmer = teilnehmerList.OrderBy(x => Guid.NewGuid()).ToList();

            // Assign GruppeIds to the TurnierTeilnehmer instances
            for (int i = 0; i < shuffledTeilnehmer.Count; i++)
            {
                var gruppe = groups[i % numberOfGroups];
                shuffledTeilnehmer[i].GruppeId = gruppe.GruppeId;
            }

            // Save changes to persist the assigned GruppeIds
            _context.SaveChanges();

            return groups;
        }


        /*Group Teilnehmer into groups, based on the AnzahlGruppen prop of the realated Turnier
        -Retrieve TurnierTeilnehmer list
        -check for valid data
        -retrieve anzahlGruppen
        -genarate random groups
        -update TurnierTeilnehmer entries */
        /*public async Task<List<Gruppe>> GroupTeilnehmer(long turnierId)
        {
            Console.WriteLine(turnierId);
            try
            {
                var turnierTeilnehmerList = await _context.TurniereTeilnehmer
                    .Where(tt => tt.TurnierId == turnierId)
                    .ToListAsync();
                Console.WriteLine(turnierTeilnehmerList);
                Console.WriteLine(turnierId);

                if (turnierTeilnehmerList == null || turnierTeilnehmerList.Count == 0)
                {
                    Console.WriteLine($"No Teilnehmer found for TurnierId: {turnierId}");
                    return null;
                }

                var numberOfGroups = (int)_context.Turniere.FirstOrDefault(t => t.Id == turnierId)?.AnzahlGruppen;
                Console.WriteLine($"Number of Groups HEYY it works until here : {numberOfGroups}" + "Turnier Id is " + turnierId + " AAAND turnierTeilnehmerList is " + turnierTeilnehmerList.Count);

                if (numberOfGroups <= 0)
                {
                    Console.WriteLine($"Invalid AnzahlGruppen for TurnierId: {turnierId}");
                    return null;
                }

                var createdGroups = GenerateRandomGroups(numberOfGroups, turnierTeilnehmerList);

                if (createdGroups == null || createdGroups.Count != numberOfGroups)
                {
                    Console.WriteLine($"Error creating groups for TurnierId: {turnierId}");
                    return null;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Groups created successfully");
                return createdGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GroupTeilnehmer: {ex.Message}");
                return null;
            }
        }

        private static List<Gruppe> GenerateRandomGroups(int numberOfGroups, List<TurnierTeilnehmer> teilnehmerList)
        {
            var groups = new List<Gruppe>();

            // Shuffle the teilnehmerList randomly
            var shuffledTeilnehmer = teilnehmerList.OrderBy(x => Guid.NewGuid()).ToList();

            // Calculate the base number of Teilnehmer per group
            var baseTeilnehmerCount = shuffledTeilnehmer.Count / numberOfGroups;

            // Calculate the number of groups that will have one additional Teilnehmer
            var groupsWithExtraTeilnehmer = shuffledTeilnehmer.Count % numberOfGroups;

            // Initialize a variable to keep track of the current index in the shuffled list
            var currentIndex = 0;

            for (int i = 1; i <= numberOfGroups; i++)
            {
                var gruppe = new Gruppe
                {
                    Gruppenname = $"Gruppe {i}",
                    TurnierId = shuffledTeilnehmer.First().TurnierId,
                };

                groups.Add(gruppe);

                // Calculate the number of Teilnehmer for the current group
                var currentTeilnehmerCount = baseTeilnehmerCount + (i <= groupsWithExtraTeilnehmer ? 1 : 0);

                // Assign Teilnehmer to the current group
                for (int j = 0; j < currentTeilnehmerCount; j++)
                {
                    var turnierTeilnehmer = shuffledTeilnehmer[currentIndex++];
                    turnierTeilnehmer.GruppeId = gruppe.GruppeId;
                }
            }

            return groups;
        }
*/

        /*Metjhod to generate groups
        -return List of generated Groups
        -parameters: anzahlGruppen, list of TurnierTeilnehmer*/
        /*private static List<Gruppe> GenerateRandomGroups(int numberOfGroups, List<TurnierTeilnehmer> teilnehmerList)
        {
            var groups = new List<Gruppe>();

            // Shuffle the teilnehmerList randomly
            var shuffledTeilnehmer = teilnehmerList.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 1; i <= numberOfGroups; i++)
            {
                var gruppe = new Gruppe
                {
                    Gruppenname = $"Gruppe {i}",
                    TurnierId = shuffledTeilnehmer.First().TurnierId,
                };

                groups.Add(gruppe);

                // Assign an equal number of Teilnehmer to each group
                /*var teilnehmerCount = shuffledTeilnehmer.Count / numberOfGroups;

                for (int j = 0; j < teilnehmerCount; j++)
                {
                    var turnierTeilnehmer = shuffledTeilnehmer[j + i * teilnehmerCount];
                    turnierTeilnehmer.GruppeId = gruppe.GruppeId;
                }
            }

            return groups;
        }*/

    }
}
