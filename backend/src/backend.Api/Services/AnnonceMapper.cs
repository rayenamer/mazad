using Backend.Api.Domain;

namespace Backend.Api.Services;

public static class AnnonceMapper
{
    public static AnnonceDto ToDto(Annonce a)
    {
        return new AnnonceDto
        {
            Id = a.Id,
            Titre = a.Titre,
            Description = a.Description,
            PrixDepart = a.PrixDepart,
            PasEnchere = a.PasEnchere,
            DateFin = a.DateFin,
            PrixActuel = a.PrixActuel,
            MontantMinimum = a.MontantMinimum,
            NombreEncheres = a.Encheres.Count,
            Statut = a.Statut
        };
    }

    public static AnnonceDetailDto ToDetailDto(Annonce a)
    {
        var encheres = new List<EnchereDto>();

        foreach (var enchere in a.Encheres.OrderByDescending(e => e.Montant))
        {
            encheres.Add(ToEnchereDto(enchere));
        }

        return new AnnonceDetailDto
        {
            Id = a.Id,
            Titre = a.Titre,
            Description = a.Description,
            PrixDepart = a.PrixDepart,
            PasEnchere = a.PasEnchere,
            DateFin = a.DateFin,
            PrixActuel = a.PrixActuel,
            MontantMinimum = a.MontantMinimum,
            NombreEncheres = a.Encheres.Count,
            Statut = a.Statut,
            Encheres = encheres
        };
    }

    public static EnchereDto ToEnchereDto(Enchere e)
    {
        return new EnchereDto
        {
            Pseudo = e.Pseudo,
            Montant = e.Montant,
            Date = e.Date
        };
    }
}