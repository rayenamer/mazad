namespace Backend.Api.Domain;

public class Annonce
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal PrixDepart { get; set; }
    public decimal PasEnchere { get; set; }
    public DateTimeOffset DateFin { get; set; }
    public List<Enchere> Encheres { get; set; } = new();

    public Enchere? MeilleureEnchere
    {
        get
        {
            if (Encheres.Count == 0)
                return null;

            return Encheres.MaxBy(e => e.Montant);
        }
    }

    public decimal PrixActuel
    {
        get
        {
            if (MeilleureEnchere != null)
                return MeilleureEnchere.Montant;

            return PrixDepart;
        }
    }

    public decimal MontantMinimum
    {
        get { return PrixActuel + PasEnchere; }
    }

    public bool EstTerminee
    {
        get
        {
            return DateTimeOffset.UtcNow >= DateFin;
        }
    }

    public string Statut
    {
        get
        {
            if (EstTerminee)
                return "terminee";

            return "en_cours";
        }
    }
}