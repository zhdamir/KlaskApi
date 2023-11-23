using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class Turnier
{
    [Key]
    public long Id { get; set; }
    public string? TurnierTitel { get; set; }
    public DateTime StartDatum { get; set; }
    public DateTime EndDatum { get; set; }
    public long AnzahlGruppen { get; set; }


}