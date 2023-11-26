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
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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
    public DbSet<LoginDaten> LoginDaten { get; set; }


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
        modelBuilder.Entity<LoginDaten>().ToTable("LoginDaten");


        modelBuilder.Entity<Bereich>()
        .HasMany<Teilnehmer>()
        .WithOne()
        .HasForeignKey(tn => tn.BereichId)
        .IsRequired();


        modelBuilder.Entity<BenutzerRolle>()
        .HasMany<Teilnehmer>()
        .WithOne()
        .HasForeignKey(tn => tn.RolleId)
        .IsRequired();

        modelBuilder.Entity<Turnier>()
        .HasMany<TurnierTeilnehmer>()
        .WithOne()
        .HasForeignKey(tt => tt.TurnierId)
        .IsRequired();

        modelBuilder.Entity<Teilnehmer>()
        .HasMany<TurnierTeilnehmer>()
        .WithOne()
        .HasForeignKey(tt => tt.TeilnehmerId)
        .IsRequired();

        modelBuilder.Entity<Turnier>()
        .HasMany<Gruppe>()
        .WithOne()
        .HasForeignKey(g => g.TurnierId)
        .IsRequired();

        modelBuilder.Entity<Gruppe>()
        .HasMany<TurnierTeilnehmer>()
        .WithOne()
        .HasForeignKey(tt => tt.GruppeId);


        modelBuilder.Entity<LoginDaten>()
        .HasOne<Teilnehmer>()
        .WithOne()
        .HasForeignKey<LoginDaten>(ld => ld.TeilnehmerId)
        .IsRequired();

        modelBuilder.Entity<Turnier>()
        .HasMany<Runde>()
        .WithOne()
        .HasForeignKey(r => r.TurnierId)
        .IsRequired();

        modelBuilder.Entity<Runde>()
        .HasMany<Spiel>()
        .WithOne()
        .HasForeignKey(s => s.RundeId)
        .IsRequired();

        modelBuilder.Entity<Spiel>()
        .HasMany<SpielTeilnehmer>()
        .WithOne()
        .HasForeignKey(st => st.SpielId)
        .IsRequired();

        modelBuilder.Entity<Teilnehmer>()
        .HasMany<SpielTeilnehmer>()
        .WithOne()
        .HasForeignKey(st => st.TeilnehmerId)
        .IsRequired();
    }

}