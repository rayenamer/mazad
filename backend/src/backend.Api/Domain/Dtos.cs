namespace Backend.Api.Domain;

public class AnnonceDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal PrixDepart { get; set; }
    public decimal PasEnchere { get; set; }
    public DateTimeOffset DateFin { get; set; }
    public decimal PrixActuel { get; set; }
    public decimal MontantMinimum { get; set; }
    public int NombreEncheres { get; set; }
    public string Statut { get; set; } = "";
}

public class EnchereDto
{
    public string Pseudo { get; set; } = "";
    public decimal Montant { get; set; }
    public DateTimeOffset Date { get; set; }
}

public class AnnonceDetailDto : AnnonceDto
{
    public List<EnchereDto> Encheres { get; set; } = new();
}

public class CreerEnchereDto
{
    public string? Pseudo { get; set; }
    public decimal? Montant { get; set; }
}