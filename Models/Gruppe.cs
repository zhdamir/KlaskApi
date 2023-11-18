
using System.ComponentModel.DataAnnotations;

namespace KlaskApi.Models;


public class Gruppe
{
    [Key]
    public long GruppeId { get; set; }
    public string Gruppenname { get; set; }

    [Required]
    public long TurnierId { get; set; }
    public Turnier Turnier { get; set; }
    public List<TurnierTeilnehmer> TurnierTeilnehmerListe { get; set; }
}