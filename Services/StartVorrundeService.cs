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
*/


    }
}
