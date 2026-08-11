using System.Text.Json;
using Backend.Api.Domain;

namespace Backend.Api.Repositories;

public class InMemoryAnnonceRepository : IAnnonceRepository
{
    private readonly Dictionary<Guid, Annonce> _annonces;

    // Constructeur utilisé par les tests : on injecte directement les annonces.
    public InMemoryAnnonceRepository(IEnumerable<Annonce> annonces)
    {
        _annonces = annonces.ToDictionary(a => a.Id);
    }

    public static InMemoryAnnonceRepository DepuisFichier(string chemin)
    {
        var json = File.ReadAllText(chemin);
        
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var annonces = JsonSerializer.Deserialize<List<Annonce>>(json, options)
                       ?? throw new InvalidDataException($"Fichier vide ou invalide : {chemin}");

        return new InMemoryAnnonceRepository(annonces);
    }

    public IEnumerable<Annonce> GetAll() => _annonces.Values;

    public Annonce? GetById(Guid id) => _annonces.GetValueOrDefault(id);

    public void AjouterEnchere(Guid annonceId, Enchere enchere)
    {
        if (_annonces.TryGetValue(annonceId, out var annonce))
        {
            annonce.Encheres.Add(enchere);
        }
    }
}