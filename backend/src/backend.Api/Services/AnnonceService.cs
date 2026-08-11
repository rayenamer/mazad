using Backend.Api.Domain;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AnnonceService
{
    private readonly IAnnonceRepository _repository;

    public AnnonceService(IAnnonceRepository repository)
    {
        _repository = repository;
    }

    public List<AnnonceDto> Lister(string? statut, string? q)
    {
        var annonces = new List<Annonce>();

        foreach (var annonce in _repository.GetAll())
        {
            if (!CorrespondAuStatut(annonce, statut))
            {
                continue;
            }

            if (!CorrespondALaRecherche(annonce, q))
            {
                continue;
            }

            annonces.Add(annonce);
        }


        var resultat = new List<AnnonceDto>();

        foreach (var annonce in annonces)
        {
            resultat.Add(AnnonceMapper.ToDto(annonce));
        }

        return resultat;
    }

    public AnnonceDetailDto Obtenir(Guid id)
    {
        var annonce = TrouverAnnonce(id);

        return AnnonceMapper.ToDetailDto(annonce);
    }

    public AnnonceDetailDto PlacerEnchere(Guid id, CreerEnchereDto? dto)
    {
        ValiderSaisie(dto);

        var pseudo = dto!.Pseudo!.Trim();
        var montant = dto.Montant!.Value;

        var annonce = TrouverAnnonce(id);

        if (annonce.EstTerminee)
        {
            throw new ConflictException("Cette vente est terminée.");
        }

        if (montant < annonce.MontantMinimum)
        {
            throw new ConflictException("L'enchère doit être d'au moins " + annonce.MontantMinimum + " €.");
        }

        var enchere = new Enchere
        {
            Pseudo = pseudo,
            Montant = montant,
            Date = DateTimeOffset.UtcNow
        };

        _repository.AjouterEnchere(id, enchere);

        return AnnonceMapper.ToDetailDto(annonce);
    }

    private Annonce TrouverAnnonce(Guid id)
    {
        var annonce = _repository.GetById(id);

        if (annonce == null)
        {
            throw new NotFoundException("Cette annonce n'existe pas.");
        }

        return annonce;
    }

    private static void ValiderSaisie(CreerEnchereDto? dto)
    {
        if (dto == null)
        {
            throw new ValidationException("Le corps de la requête est obligatoire.");
        }

        if (dto.Montant == null)
        {
            throw new ValidationException("Le montant est obligatoire.");
        }

        if (dto.Montant.Value <= 0)
        {
            throw new ValidationException("Le montant doit être strictement positif.");
        }

        if (dto.Pseudo == null || dto.Pseudo.Trim().Length < 3)
        {
            throw new ValidationException("Le pseudo doit faire au moins 3 caractères.");
        }
    }

    private static bool CorrespondAuStatut(Annonce annonce, string? statut)
    {
        if (string.IsNullOrWhiteSpace(statut))
        {
            return true;
        }

        if (statut != "en_cours" && statut != "terminee")
        {
            throw new ValidationException("Le statut doit valoir 'en_cours' ou 'terminee'.");
        }

        return annonce.Statut == statut;
    }

    private static bool CorrespondALaRecherche(Annonce annonce, string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return true;
        }

        var terme = q.Trim();

        if (annonce.Titre.Contains(terme, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return annonce.Description.Contains(terme, StringComparison.OrdinalIgnoreCase);
    }

}