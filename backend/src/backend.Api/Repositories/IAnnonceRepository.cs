using Backend.Api.Domain;

namespace Backend.Api.Repositories;

public interface IAnnonceRepository
{
    IEnumerable<Annonce> GetAll();
    Annonce? GetById(Guid id);
    void AjouterEnchere(Guid annonceId, Enchere enchere);
}