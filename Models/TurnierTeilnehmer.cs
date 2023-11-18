
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class TurnierTeilnehmer
{
    [Key]
    public long TurnierTeilnehmerId { get; set; }

    public long TurnierId { get; set; }
    [ForeignKey("TurnierId")]
    public Turnier Turnier { get; set; }

    public long TeilnehmerId { get; set; }
    [ForeignKey("TeilnehmerId")]
    public Teilnehmer Teilnehmer { get; set; }

    [ForeignKey("GruppeId")]
    public long GruppeId { get; set; }
    public Gruppe Gruppe { get; set; }
}