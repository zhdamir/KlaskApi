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
        // Hier definierst du die Beziehung zwischen Turnieren und Gruppen
        modelBuilder.Entity<Gruppe>()
            .HasOne(g => g.Turnier)   // Jede Gruppe gehört zu einem Turnier
            .WithMany(t => t.Gruppen)  // Ein Turnier kann viele Gruppen haben
            .HasForeignKey(g => g.TurnierId); // Fremdschlüssel in Gruppe, um das Turnier zuzuordnen

        modelBuilder.Entity<Gruppe>()
            .HasMany(g => g.TeilnehmerListe)
            .WithOne(t => t.Gruppe)
            .HasForeignKey(t => t.GruppeId);

        modelBuilder.Entity<Teilnehmer>()
            .HasOne(tn => tn.Bereich)
            .WithMany(b => b.TeilnehmerListe)
            .HasForeignKey(tn => tn.BereichId);

        modelBuilder.Entity<Runde>()
            .HasOne(r => r.Turnier)
            .WithMany(t => t.Runden)
            .HasForeignKey(r => r.TurnierId);

        modelBuilder.Entity<Spiel>()
            .HasOne(s => s.Runde)
            .WithMany(r => r.SpieleListe)
            .HasForeignKey(s => s.RundeId);

        modelBuilder.Entity<SpielTeilnehmer>()
            .HasOne(st => st.Spiel)
            .WithMany(s => s.SpielTeilnehmerListe)
            .HasForeignKey(st => st.SpielId);

        modelBuilder.Entity<SpielTeilnehmer>()
            .HasOne(st => st.Teilnehmer)
            .WithMany(tn => tn.SpielTeilnehmerListe)
            .HasForeignKey(st => st.TeilnehmerId);

        modelBuilder.Entity<Teilnehmer>()
            .HasOne(tn => tn.Rolle)
            .WithMany(b => b.TeilnehmerListe)
            .HasForeignKey(tn => tn.RoleId);

    }
}