using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KlaskApi.Models;

public class BenutzerRolle
{

    //Authentication test git
    [Key]
    public long RolleId { get; set; }
    public String BezeichnungRolle { get; set; }

    /*public List<Teilnehmer> TeilnehmerListe { get; set; }*/


}