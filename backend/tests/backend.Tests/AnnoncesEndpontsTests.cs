using System.Net;
using System.Net.Http.Json;
using Backend.Api.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests;

public class AnnoncesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private const string IdTracteur = "a1f4c2d0-0003-4b6e-9c11-000000000003";

    private const string IdTerminee = "a1f4c2d0-0004-4b6e-9c11-000000000004";

    public AnnoncesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GET_annonces_Renvoie200()
    {
        var client = _factory.CreateClient();
        var reponse = await client.GetAsync("/api/annonces");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }
    [Fact]
    public async Task GET_annonces_AvecStatutInconnu_Renvoie400()
    {
        var client = _factory.CreateClient();
        var reponse = await client.GetAsync("/api/annonces?statut=bientot");
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task GET_annonce_ParId_Renvoie200()
    {
        var client = _factory.CreateClient();
        var reponse = await client.GetAsync("/api/annonces/" + IdTracteur);
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task GET_annonce_IdInexistant_Renvoie404()
    {
        var client = _factory.CreateClient();
        var idInconnu = Guid.NewGuid();
        var reponse = await client.GetAsync("/api/annonces/" + idInconnu);
        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task POST_enchere_MontantSuffisant_Renvoie201()
    {
        var client = _factory.CreateClient();
        var dto = new CreerEnchereDto { Pseudo = "alice", Montant = 5000 };
        var reponse = await client.PostAsJsonAsync("/api/annonces/" + IdTracteur + "/encheres", dto);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
    }

    [Fact]
    public async Task POST_enchere_MontantTropBas_Renvoie409()
    {
        var client = _factory.CreateClient();
        var dto = new CreerEnchereDto { Pseudo = "alice", Montant = 10 };
        var reponse = await client.PostAsJsonAsync("/api/annonces/" + IdTracteur + "/encheres", dto);
        Assert.Equal(HttpStatusCode.Conflict, reponse.StatusCode);
    }

    [Fact]
    public async Task POST_enchere_VenteTerminee_Renvoie409()
    {
        var client = _factory.CreateClient();
        var dto = new CreerEnchereDto { Pseudo = "alice", Montant = 99999 };
        var reponse = await client.PostAsJsonAsync("/api/annonces/" + IdTerminee + "/encheres", dto);
        Assert.Equal(HttpStatusCode.Conflict, reponse.StatusCode);
    }

    [Fact]
    public async Task POST_enchere_PseudoVide_Renvoie400()
    {
        var client = _factory.CreateClient();
        var dto = new CreerEnchereDto { Pseudo = "", Montant = 99999 };
        var reponse = await client.PostAsJsonAsync("/api/annonces/" + IdTracteur + "/encheres", dto);
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task POST_enchere_AnnonceInexistante_Renvoie404()
    {
        var client = _factory.CreateClient();
        var dto = new CreerEnchereDto { Pseudo = "alice", Montant = 99999 };
        var idInconnu = Guid.NewGuid();
        var reponse = await client.PostAsJsonAsync("/api/annonces/" + idInconnu + "/encheres", dto);
        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }
}