
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KlaskApi.Models;

public class Teilnehmer
{

    [Key]
    public long TeilnehmerId { get; set; }
    public String? Vorname { get; set; }
    public String? Nachname { get; set; }
    public String? Email { get; set; }
    public long BereichId { get; set; }
    public long RolleId { get; set; }


}