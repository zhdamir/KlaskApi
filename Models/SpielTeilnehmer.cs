
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class SpielTeilnehmer
{
    [Key]
    public long SpielTeilnehmerId { get; set; }

    public long SpielId { get; set; }
    [ForeignKey("SpielId")]
    public Spiel Spiel { get; set; }

    public long TeilnehmerId { get; set; }
    [ForeignKey("TeilnehmerId")]
    public Teilnehmer Teilnehmer { get; set; }

    public long Punkte { get; set; }
}