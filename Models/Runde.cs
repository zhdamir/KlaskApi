
using System.ComponentModel.DataAnnotations;

namespace KlaskApi.Models;




public class Runde
{
    [Key]
    public long RundeId { get; set; }
    public string? RundeBezeichnung { get; set; }

    [Required]
    public long TurnierId { get; set; }
    public Turnier Turnier { get; set; }
    public List<Spiel> SpieleListe { get; set; }
}

