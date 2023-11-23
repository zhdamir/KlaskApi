
using System.ComponentModel.DataAnnotations;

namespace KlaskApi.Models;




public class Runde
{
    [Key]
    public long RundeId { get; set; }
    public string? RundeBezeichnung { get; set; }
    public long TurnierId { get; set; }
}

