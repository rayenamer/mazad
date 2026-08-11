# NOTES

## Stack choisie

**Option A — C# / .NET 8 (ASP.NET Core Web API) + React 18 / TypeScript (Vite).**

## Lancer le projet

```bash
./start.sh
```

Tests backend, puis Lance API puis frontend. `Ctrl+C` arrête tout.

---

## Architecture


```
Requête → [Middleware] → Controllers → Services → Repositories → Domain
              ▲              HTTP        Règles      Stockage      Données
              │                             │
              └────── exception ────────────┘
                   traduite en 400/404/409
```

| Dossier | Rôle | Ne fait jamais |
| --- | --- | --- |
| `Domain` | entités, DTOs, exceptions métier | ne dépend de rien |
| `Repositories` | interface + stockage en mémoire | aucune validation |
| `Services` | toutes les règles d'enchère, mapping vers les DTOs | aucun HTTP |
| `Controllers` | lire la requête, appeler le service, renvoyer un statut | aucune logique |
| `Infrastructure` | middleware qui traduit les exceptions en codes HTTP | — |

Il enveloppe tout le pipeline dans un `try/catch` et traduit :
`ValidationException` → 400, `NotFoundException` → 404, `ConflictException` → 409.

Sans lui, ASP.NET ne connaît pas mes types d'exception et renvoie **500** : une
enchère trop basse sortait en erreur serveur alors que c'est un refus métier prévu.

Il est enregistré en premier, car un middleware n'attrape que ce qui se passe en
dessous de lui. Conséquence : aucun `try/catch` dans les contrôleurs, et un seul
endroit décide du format d'erreur.
---

## Choix techniques

### Rien de dérivable n'est stocké

`PrixActuel`, `MontantMinimum` et `Statut` sont des propriétés **calculées** sur
`Annonce`, jamais des champs. Il est donc impossible qu'un prix affiché diverge de
l'historique des enchères. Ces valeurs sont envoyées au front dans les DTOs : le
client n'a pas à rejouer la règle du pas, et changer la règle côté C# ne demande
aucune modification côté React.

### Les erreurs métier sont des exceptions, traduites en un seul endroit

Trois exceptions dans `Domain` : `ValidationException` (400), `NotFoundException`
(404), `ConflictException` (409). Un middleware les attrape et les traduit en
réponse HTTP.

Conséquence : les contrôleurs n'ont **aucun try/catch** et restent lisibles, et un
seul endroit décide du format d'erreur. Le middleware est enregistré en premier
dans le pipeline, sinon il ne verrait pas ce qui se passe en dessous de lui.

**409 plutôt que 400** sur « vente terminée » et « enchère trop basse » : la requête
est parfaitement bien formée, c'est l'état de la ressource qui la rend impossible.
Le client peut ainsi proposer « recharger et réessayer » sur un 409 et « corrigez
votre saisie » sur un 400.

### Ordre des refus

Dans `PlacerEnchere`, l'ordre est volontaire : saisie, puis clôture, puis montant.
Annoncer « montant trop bas » sur une vente déjà close est exact mais inutile.


### Stockage en mémoire

`data/annonces.json` est lu une seule fois au démarrage. Le dépôt est enregistré en
**singleton** : en `Scoped`, chaque requête repartirait du fichier et l'enchère
déposée aurait déjà disparu à la suivante.

Le fichier n'est jamais réécrit : les enchères vivent en RAM et disparaissent au
redémarrage, ce que le sujet autorise.

---

## Hypothèses assumées

tout :D

---

## Tests

Tests d'**intégration** via `WebApplicationFactory` : ils démarrent l'API en mémoire
et envoient de vraies requêtes HTTP. Ils couvrent les cinq règles de refus du sujet
plus les cas passants, en vérifiant les codes 200 / 201 / 400 / 404 / 409.

---

## Bonus traités

- **Filtre par statut et recherche texte** sur le titre et la description.
- **Temps restant** avant clôture, calculé côté client à partir de `dateFin`. Le
  calculer côté serveur le figerait à l'instant de la réponse.
- **Rafraîchissement automatique** avec le changement de filtre

---

## Ce que je n'ai pas eu le temps de faire

- **Tri par prix ou date de fin.** Le tri par défaut privilégie les ventes ouvertes
  puis la clôture la plus proche. J'aurais ajouté un paramètre `?tri=` avec quelques
  comparateurs.
- **Persistance.** `IAnnonceRepository` est fait pour ça : une implémentation
  EF Core + SQLite se substitue sans toucher au service.
- **Temps réel.** Le polling interroge le serveur même quand rien ne change.
  SignalR avec un groupe par annonce pousserait la mise à jour au moment exact de
  l'enchère. Trois lignes contre une demi-journée : j'ai choisi les trois lignes.
- **Debounce sur la recherche.** Le front appelle l'API à chaque frappe. Sur cinq
  annonces c'est indolore, mais il faudrait attendre ~250 ms après la dernière
  touche.
### Caching
Rien n'est mis en cache aujourd'hui : chaque appel refait le travail complet.

---

## Questions ouvertes

### 1. Deux enchères exactement en même temps



### 2. Passer à des milliers d'annonces et d'utilisateurs


---

## Outils d'assistance

J'ai utilisé une IA sur ce test, comme le permet le sujet. Elle m'a servi à
accélérer l'écriture répétitive (DTOs, mappers, CSS) et à challenger mes choix,
notamment sur la gestion de la concurrence et sur la frontière entre validation de
transport et validation métier. Les décisions documentées ci-dessus sont les
miennes et je peux défendre chaque ligne du code.