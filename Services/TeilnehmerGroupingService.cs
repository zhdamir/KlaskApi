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
