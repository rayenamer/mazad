namespace Backend.Api.Domain;

public class Enchere
{
    public string Pseudo { get; set; } = "";
    public decimal Montant { get; set; }
    public DateTimeOffset Date { get; set; }
}