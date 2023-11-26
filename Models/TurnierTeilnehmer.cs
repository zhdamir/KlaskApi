
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class TurnierTeilnehmer
{
    [Key]
    public long TurnierTeilnehmerId { get; set; }
    public long TurnierId { get; set; }
    public long TeilnehmerId { get; set; }
    public long? GruppeId { get; set; }
}