
using System.ComponentModel.DataAnnotations;

namespace KlaskApi.Models;


public class Gruppe
{
    [Key]
    public long GruppeId { get; set; }
    public string? Gruppenname { get; set; }

    public long TurnierId { get; set; }
}