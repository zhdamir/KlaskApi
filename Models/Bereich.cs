
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class Bereich
{
    [Key]
    public long BereichId { get; set; }
    public String? BereichName { get; set; }
}