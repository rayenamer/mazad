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
- **Le probléme de concurrence** deux encheres exactes pour la meme annonce , ma solution ne traite pas ce cas

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

**Non, mon implémentation ne gère pas ce cas.**

#### Le problème

Placer une enchère se fait en trois temps :

```
1. lire le prix actuel        →  1850
2. valider les règles         →  1900 > 1850, OK
3. écrire la nouvelle enchère →  1900
```
Rien n'empêche une deuxième requête de s'intercaler entre l'étape 1 et l'étape 3.
Deux personnes qui misent 1900 € lisent toutes les deux 1850, valident toutes les
deux, et enregistrent toutes les deux. Deux gagnants au même prix, alors que ma
règle exige un montant strictement supérieur au prix actuel.

C'est le *lost update* : une séquence lire-modifier-écrire qui n'est pas
**atomique**. Atomique veut dire indivisible ; soit l'opération se fait entièrement,
soit pas du tout, mais jamais à moitié, et rien ne peut s'exécuter au milieu.
Ici, l'opération est divisible en trois, donc vulnérable.

#### La solution : rendre l'écriture atomique en une seule requête

L'idée est de ne plus lire puis écrire, mais de **faire porter la condition par
l'écriture elle-même** :

```sql
INSERT INTO encheres (annonce_id, pseudo, montant, date)
SELECT @annonceId, @pseudo, @montant, NOW()
WHERE @montant >= (
    SELECT COALESCE(MAX(montant), a.prix_depart) + a.pas_enchere
    FROM annonces a
    LEFT JOIN encheres e ON e.annonce_id = a.id
    WHERE a.id = @annonceId
);
```
La base évalue la condition et fait l'insertion dans **la même instruction**. Elle
garantit qu'aucune autre transaction ne s'intercale entre les deux — c'est
exactement ce qu'on ne pouvait pas garantir en C#.

Le résultat se lit au nombre de lignes affectées :

- **1 ligne** → l'enchère est acceptée.
- **0 ligne** → quelqu'un est passé avant, le montant n'est plus suffisant. On
  renvoie un 409 avec le nouveau prix, le client réessaie.

#### d'autres solutions:



### 2. Des milliers d'annonces et d'utilisateurs

Je vois deux niveaux : ce qui coince dans mon code, et ce qui manque autour.

#### Dans mon code

**Le stockage en mémoire, en premier.** Tout vit dans la RAM d'un seul processus.
Rien ne survit à un redémarrage, et surtout : impossible de lancer une deuxième
instance, les deux auraient chacune leurs données. C'est le blocage qui empêche
tous les autres. `IAnnonceRepository` existe pour ça — il n'y a qu'une seconde
implémentation à écrire, le service et les contrôleurs n'y touchent pas.

**Le filtrage en mémoire.** `GetAll()` charge toutes les annonces puis filtre en
C#. Sur cinq, invisible ; sur dix mille, l'API charge dix mille objets pour en
renvoyer vingt. Il faut pousser filtres et tri dans la requête SQL, et ajouter une
pagination (`?page=&taille=`) — aujourd'hui l'API renvoie tout, sans limite.

**Le verrou applicatif.** Dès qu'il y a deux instances, elle ne protège plus rien. Il faut la
déplacer dans la base 
 c'est la
condition pour pouvoir dupliquer l'API.

**Le polling du frontend.** Chaque onglet ouvert appelle l'API  que quelque chose ait changé ou non. À mille utilisateurs connectés, ça
fait 200 requêtes par seconde pour rien. SignalR pousserait la mise à jour au
moment de l'enchère.

#### Autour du code

**Plusieurs instances derrière un load balancer.** Une seule API est un point de
défaillance unique et un plafond de charge. C'est possible uniquement une fois les
deux premiers points réglés : mon API doit être *stateless*, c'est-à-dire ne rien
garder en mémoire entre deux requêtes.

**Du cache sur la lecture.** Les annonces sont lues bien plus souvent qu'écrites.
Mais un cache en mémoire par instance donnerait des prix différents selon le
serveur qui répond , il faut un cache partagé type Redis. Et je ne cacherais pas
le prix d'une vente en cours : servir une valeur périmée ferait refuser des
enchères pourtant valides.

**Authentification et rate limiting.** Aujourd'hui n'importe qui peut enchérir
sous le pseudo d'un autre, et rien n'empêche d'inonder l'API. Avec du volume, ce
n'est plus un détail.

Dans l'ordre, je ferais : base de données → pagination et index → concurrence en
base → plusieurs instances derrière un load balancer → cache → temps réel. Chaque
étape débloque la suivante.

---

## Outils d'assistance

J'ai utilisé une IA sur ce test, comme le permet le sujet. Elle m'a servi à
accélérer l'écriture répétitive (DTOs, mappers, CSS) et à challenger mes choix,
notamment sur la gestion de la concurrence et sur la frontière entre validation de
transport et validation métier. Les décisions documentées ci-dessus sont les
miennes et je peux défendre chaque ligne du code.