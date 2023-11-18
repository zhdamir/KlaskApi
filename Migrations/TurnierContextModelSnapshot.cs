﻿// <auto-generated />
using System;
using KlaskApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    [DbContext(typeof(TurnierContext))]
    partial class TurnierContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("KlaskApi.Models.BenutzerRolle", b =>
                {
                    b.Property<long>("RolleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("RolleId"));

                    b.Property<string>("BezeichnungRolle")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RolleId");

                    b.ToTable("BenutzerRolle", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Bereich", b =>
                {
                    b.Property<long>("BereichId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("BereichId"));

                    b.Property<string>("BereichName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("BereichId");

                    b.ToTable("Bereich", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Gruppe", b =>
                {
                    b.Property<long>("GruppeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("GruppeId"));

                    b.Property<string>("Gruppenname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("TurnierId")
                        .HasColumnType("bigint");

                    b.HasKey("GruppeId");

                    b.HasIndex("TurnierId");

                    b.ToTable("Gruppen", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Runde", b =>
                {
                    b.Property<long>("RundeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("RundeId"));

                    b.Property<string>("RundeBezeichnung")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("TurnierId")
                        .HasColumnType("bigint");

                    b.HasKey("RundeId");

                    b.HasIndex("TurnierId");

                    b.ToTable("Runden", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Spiel", b =>
                {
                    b.Property<long>("SpielId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("SpielId"));

                    b.Property<long>("RundeId")
                        .HasColumnType("bigint");

                    b.HasKey("SpielId");

                    b.HasIndex("RundeId");

                    b.ToTable("Spiele", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.SpielTeilnehmer", b =>
                {
                    b.Property<long>("SpielTeilnehmerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("SpielTeilnehmerId"));

                    b.Property<long>("Punkte")
                        .HasColumnType("bigint");

                    b.Property<long>("SpielId")
                        .HasColumnType("bigint");

                    b.Property<long>("TeilnehmerId")
                        .HasColumnType("bigint");

                    b.HasKey("SpielTeilnehmerId");

                    b.HasIndex("SpielId");

                    b.HasIndex("TeilnehmerId");

                    b.ToTable("SpieleTeilnehmer", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Teilnehmer", b =>
                {
                    b.Property<long>("TeilnehmerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TeilnehmerId"));

                    b.Property<long>("BereichId")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Nachname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("RolleId")
                        .HasColumnType("bigint");

                    b.Property<string>("Vorname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TeilnehmerId");

                    b.HasIndex("BereichId");

                    b.HasIndex("RolleId");

                    b.ToTable("Teilnehmer", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Turnier", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AnazhlGruppen")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("EndDatum")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("StartDatum")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TurnierTitel")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Turniere", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.TurnierTeilnehmer", b =>
                {
                    b.Property<long>("TurnierTeilnehmerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TurnierTeilnehmerId"));

                    b.Property<long>("GruppeId")
                        .HasColumnType("bigint");

                    b.Property<long>("TeilnehmerId")
                        .HasColumnType("bigint");

                    b.Property<long>("TurnierId")
                        .HasColumnType("bigint");

                    b.HasKey("TurnierTeilnehmerId");

                    b.HasIndex("GruppeId");

                    b.HasIndex("TurnierId");

                    b.ToTable("TurnierTeilnehmer", (string)null);
                });

            modelBuilder.Entity("KlaskApi.Models.Gruppe", b =>
                {
                    b.HasOne("KlaskApi.Models.Turnier", "Turnier")
                        .WithMany("Gruppen")
                        .HasForeignKey("TurnierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Turnier");
                });

            modelBuilder.Entity("KlaskApi.Models.Runde", b =>
                {
                    b.HasOne("KlaskApi.Models.Turnier", "Turnier")
                        .WithMany("Runden")
                        .HasForeignKey("TurnierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Turnier");
                });

            modelBuilder.Entity("KlaskApi.Models.Spiel", b =>
                {
                    b.HasOne("KlaskApi.Models.Runde", "Runde")
                        .WithMany("SpieleListe")
                        .HasForeignKey("RundeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Runde");
                });

            modelBuilder.Entity("KlaskApi.Models.SpielTeilnehmer", b =>
                {
                    b.HasOne("KlaskApi.Models.Spiel", "Spiel")
                        .WithMany("SpielTeilnehmerListe")
                        .HasForeignKey("SpielId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KlaskApi.Models.Teilnehmer", "Teilnehmer")
                        .WithMany("SpielTeilnehmerListe")
                        .HasForeignKey("TeilnehmerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Spiel");

                    b.Navigation("Teilnehmer");
                });

            modelBuilder.Entity("KlaskApi.Models.Teilnehmer", b =>
                {
                    b.HasOne("KlaskApi.Models.Bereich", "Bereich")
                        .WithMany("TeilnehmerListe")
                        .HasForeignKey("BereichId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KlaskApi.Models.BenutzerRolle", "Rolle")
                        .WithMany("TeilnehmerListe")
                        .HasForeignKey("RolleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bereich");

                    b.Navigation("Rolle");
                });

            modelBuilder.Entity("KlaskApi.Models.TurnierTeilnehmer", b =>
                {
                    b.HasOne("KlaskApi.Models.Gruppe", "Gruppe")
                        .WithMany("TurnierTeilnehmerListe")
                        .HasForeignKey("GruppeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KlaskApi.Models.Turnier", "Turnier")
                        .WithMany("TurnierTeilnehmerListe")
                        .HasForeignKey("TurnierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KlaskApi.Models.Teilnehmer", "Teilnehmer")
                        .WithMany("TurnierTeilnehmerListe")
                        .HasForeignKey("TurnierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gruppe");

                    b.Navigation("Teilnehmer");

                    b.Navigation("Turnier");
                });

            modelBuilder.Entity("KlaskApi.Models.BenutzerRolle", b =>
                {
                    b.Navigation("TeilnehmerListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Bereich", b =>
                {
                    b.Navigation("TeilnehmerListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Gruppe", b =>
                {
                    b.Navigation("TurnierTeilnehmerListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Runde", b =>
                {
                    b.Navigation("SpieleListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Spiel", b =>
                {
                    b.Navigation("SpielTeilnehmerListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Teilnehmer", b =>
                {
                    b.Navigation("SpielTeilnehmerListe");

                    b.Navigation("TurnierTeilnehmerListe");
                });

            modelBuilder.Entity("KlaskApi.Models.Turnier", b =>
                {
                    b.Navigation("Gruppen");

                    b.Navigation("Runden");

                    b.Navigation("TurnierTeilnehmerListe");
                });
#pragma warning restore 612, 618
        }
    }
}
