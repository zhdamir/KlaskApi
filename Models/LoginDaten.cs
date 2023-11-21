
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KlaskApi.Models;

public class LoginDaten
{
    [Key]
    public long LoginId { get; set; }
    public String UserName { get; set; }
    public String Passwort { get; set; }
    public long TeilnehmerId { get; set; }
}