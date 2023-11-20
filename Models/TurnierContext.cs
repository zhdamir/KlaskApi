using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace KlaskApi.Models;

public class TurnierContext : DbContext
{
    public TurnierContext(DbContextOptions<TurnierContext> options)
        : base(options)
    {
    }

    public DbSet<Turnier> Turniere { get; set; }
    public DbSet<Gruppe> Gruppen { get; set; }
    public DbSet<Teilnehmer> Teilnehmer { get; set; }
    public DbSet<Runde> Runden { get; set; }

    public DbSet<Bereich> Bereich { get; set; }
    public DbSet<Spiel> Spiele { get; set; }
    public DbSet<SpielTeilnehmer> SpieleTeilnehmer { get; set; }
    public DbSet<BenutzerRolle> BenutzerRollen { get; set; }
    public DbSet<TurnierTeilnehmer> TurniereTeilnehmer { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);Identity related=> to be implemented later
        modelBuilder.Entity<Turnier>().ToTable("Turniere");
        modelBuilder.Entity<Gruppe>().ToTable("Gruppen");
        modelBuilder.Entity<Bereich>().ToTable("Bereich");
        modelBuilder.Entity<Teilnehmer>().ToTable("Teilnehmer");
        modelBuilder.Entity<Runde>().ToTable("Runden");
        modelBuilder.Entity<Spiel>().ToTable("Spiele");
        modelBuilder.Entity<SpielTeilnehmer>().ToTable("SpieleTeilnehmer");
        modelBuilder.Entity<BenutzerRolle>().ToTable("BenutzerRolle");
        modelBuilder.Entity<TurnierTeilnehmer>().ToTable("TurnierTeilnehmer");

        /*Turnier Beziwhungen*/
        // Hier definierst du die Beziehung zwischen Turnieren und Gruppen
        modelBuilder.Entity<Gruppe>()
            .HasOne(g => g.Turnier)   // Jede Gruppe gehört zu einem Turnier
            .WithMany()  // Ein Turnier kann viele Gruppen haben
            .HasForeignKey(g => g.TurnierId); // Fremdschlüssel in Gruppe, um das Turnier zuzuordnen

        modelBuilder.Entity<TurnierTeilnehmer>()
            .HasOne(tt => tt.Turnier)
            .WithMany()
            .HasForeignKey(tt => tt.TurnierId);

        modelBuilder.Entity<TurnierTeilnehmer>()
        .HasOne(tn => tn.Teilnehmer)
        .WithMany()
        .HasForeignKey(tn => tn.TurnierId);

        /*modelBuilder.Entity<Gruppe>()
        .HasMany(g => g.TurnierTeilnehmerListe)
        .WithOne(t => t.Gruppe)
        .HasForeignKey(t => t.GruppeId);*/

        //The above replaced with this:
        modelBuilder.Entity<TurnierTeilnehmer>()
        .HasOne(g => g.Gruppe)
        .WithMany()
        .HasForeignKey(g => g.GruppeId);

        /*modelBuilder.Entity<Teilnehmer>()
            .HasOne(tn => tn.Bereich)
            .WithMany()
            .HasForeignKey(tn => tn.BereichId);*/

        //The above is replaced with this: ONE TO MANY WITH NO NAVIGATIONS
        modelBuilder.Entity<Bereich>()
        .HasMany<Teilnehmer>()
        .WithOne()
        .HasForeignKey(tn => tn.BereichId)
        .IsRequired();

        /*modelBuilder.Entity<Teilnehmer>()
        .HasOne(tn => tn.Rolle)
        .WithMany()
        .HasForeignKey(tn => tn.RolleId);*/

        //The above is replaced with this: ONE TO MANY WITH NO NAVIGATIONS
        modelBuilder.Entity<BenutzerRolle>()
        .HasMany<Teilnehmer>()
        .WithOne()
        .HasForeignKey(tn => tn.RolleId)
        .IsRequired();

        modelBuilder.Entity<Runde>()
            .HasOne(r => r.Turnier)
            .WithMany()
            .HasForeignKey(r => r.TurnierId);

        modelBuilder.Entity<Spiel>()
            .HasOne(s => s.Runde)
            .WithMany()
            .HasForeignKey(s => s.RundeId);

        modelBuilder.Entity<SpielTeilnehmer>()
            .HasOne(st => st.Spiel)
            .WithMany()
            .HasForeignKey(st => st.SpielId);

        modelBuilder.Entity<SpielTeilnehmer>()
            .HasOne(st => st.Teilnehmer)
            .WithMany()
            .HasForeignKey(st => st.TeilnehmerId);
    }
}