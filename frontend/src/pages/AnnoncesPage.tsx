import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listerAnnonces } from '../api'
import type { Annonce } from '../types'

export default function AnnoncesPage() {
  const [annonces, setAnnonces] = useState<Annonce[]>([])
  const [statut, setStatut] = useState('')
  const [recherche, setRecherche] = useState('')
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState('')

  useEffect(() => {
    setChargement(true)

    listerAnnonces(statut, recherche)
      .then((resultat) => {
        setAnnonces(resultat)
        setErreur('')
      })
      .catch((e: Error) => {
        setErreur(e.message)
      })
      .finally(() => {
        setChargement(false)
      })
    // live :D 
  }, [statut, recherche])

  return (
    <div>
      <h1>Annonces</h1>

      <div>
        <input
          type="search"
          placeholder="Rechercher..."
          value={recherche}
          onChange={(e) => setRecherche(e.target.value)}
        />

        <select value={statut} onChange={(e) => setStatut(e.target.value)}>
          <option value="">Toutes</option>
          <option value="en_cours">En cours</option>
          <option value="terminee">Terminées</option>
        </select>
      </div>

      {erreur && <p style={{ color: 'red' }}>{erreur}</p>}

      {chargement && <p>Chargement...</p>}

      {!chargement && !erreur && annonces.length === 0 && (
        <p>Aucune annonce ne correspond.</p>
      )}

      <ul>
        {annonces.map((annonce) => (
          <li key={annonce.id}>
            <h2>
              <Link to={'/annonces/' + annonce.id}>{annonce.titre}</Link>
            </h2>

            <p>{annonce.description}</p>

            <p>
              Prix actuel : <strong>{annonce.prixActuel} €</strong>
              {' — '}
              {annonce.nombreEncheres} enchère(s)
              {' — '}
              {annonce.statut === 'en_cours' ? 'En cours' : 'Terminée'}
            </p>
          </li>
        ))}
      </ul>
    </div>
  )
}