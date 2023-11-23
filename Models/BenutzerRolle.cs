using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class BenutzerRolle
{
    [Key]
    public long RolleId { get; set; }
    public String? BezeichnungRolle { get; set; }
}