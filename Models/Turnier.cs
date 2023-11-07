namespace KlaskApi.Models;

public class Turnier
{
    public long Id { get; set; }
    public string? TurnierTitel { get; set; }
    public DateTime StartDatum { get; set; }
    public DateTime EndDatum { get; set; }
    public long AnazhlGruppen { get; set; }
    public List<Gruppe> Gruppen { get; set; } = new List<Gruppe>();
    public List<Runde> Runden { get; set; } = new List<Runde>();

}