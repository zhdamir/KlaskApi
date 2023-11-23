
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class Spiel
{
    [Key]
    public long SpielId { get; set; }

    public long RundeId { get; set; }

}