import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { obtenirAnnonce, placerEnchere } from '../api'
import type { AnnonceDetail } from '../types'

export default function AnnonceDetailPage() {
   
 function tempsRestant(dateFin: string): string {
      const restant = new Date(dateFin).getTime() - Date.now()

      if (restant <= 0) {
        return 'Terminée'
      }

      const minutes = Math.floor(restant / 1000 / 60)
      const heures = Math.floor(minutes / 60)
      const jours = Math.floor(heures / 24)

      if (jours > 0) {
        return jours + ' jours'
      }

      if (heures > 0) {
        return heures + ' heures'
      }

      return minutes + ' minutes'
  }
  const { id } = useParams()

  const [annonce, setAnnonce] = useState<AnnonceDetail | null>(null)
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState('')

  const [pseudo, setPseudo] = useState('')
  const [montant, setMontant] = useState('')
  const [erreurEnchere, setErreurEnchere] = useState('')
  const [succes, setSucces] = useState('')
  const [envoi, setEnvoi] = useState(false)

  useEffect(() => {
    if (!id) {
      return
    }

    setChargement(true)

    obtenirAnnonce(id)
      .then((resultat) => {
        setAnnonce(resultat)
        setErreur('')
        // tres beau honnetement
        setMontant(String(resultat.montantMinimum))
      })
      .catch((e: Error) => {
        setErreur(e.message)
      })
      .finally(() => {
        setChargement(false)
      })
  }, [id])
  useEffect(() => {
  if (!id) {
    return
  }

  const timer = setInterval(() => {
    obtenirAnnonce(id).then(setAnnonce).catch(() => {})
  }, 5000)

  return () => clearInterval(timer)
}, [id])

  async function envoyer() {
    if (!id || !annonce) {
      return
    }

    setErreurEnchere('')
    setSucces('')
    setEnvoi(true)

    try {
      const misAJour = await placerEnchere(id, pseudo, Number(montant))

      setAnnonce(misAJour)
      setMontant(String(misAJour.montantMinimum))
      setSucces('Enchère enregistrée.')
    } catch (e) {

      setErreurEnchere((e as Error).message)
    } finally {
      setEnvoi(false)
    }
  }

  if (chargement) {
    return <p>Chargement...</p>
  }

  if (erreur || !annonce) {
    return (
      <div>
        <p style={{ color: 'red' }}>{erreur || 'Annonce introuvable.'}</p>
        <Link to="/">Retour à la liste</Link>
      </div>
    )
  }

  const terminee = annonce.statut === 'terminee'

  return (
    <div>
      <Link to="/">← Toutes les annonces</Link>

      <h1>{annonce.titre}</h1>

      <p>{annonce.description}</p>

      <ul>
        <li>Prix de départ : {annonce.prixDepart} €</li>
        <li>Pas d'enchère : {annonce.pasEnchere} €</li>
        <li>
          Prix actuel : <strong>{annonce.prixActuel} €</strong>
        </li>
        <li>
            Clôture : {new Date(annonce.dateFin).toLocaleString('fr-FR')}
            {!terminee && <> — reste {tempsRestant(annonce.dateFin)}</>}
        </li>
        <li>Statut : {terminee ? 'Terminée' : 'En cours'}</li>
      </ul>

      <h2>Enchérir</h2>

      {terminee ? (
        <p>Cette vente est terminée.</p>
      ) : (
        <div>
          <div>
            <label htmlFor="pseudo">Pseudo</label>
            <input
              id="pseudo"
              value={pseudo}
              onChange={(e) => setPseudo(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="montant">Montant (€)</label>
            <input
              id="montant"
              type="number"
              value={montant}
              onChange={(e) => setMontant(e.target.value)}
            />
            <p>Minimum : {annonce.montantMinimum} €</p>
          </div>

          <button type="button" onClick={envoyer} disabled={envoi}>
            {envoi ? 'Envoi...' : 'Placer mon enchère'}
          </button>

          {erreurEnchere && <p style={{ color: 'red' }}>{erreurEnchere}</p>}
          {succes && <p style={{ color: 'green' }}>{succes}</p>}
        </div>
      )}

      <h2>Enchères ({annonce.nombreEncheres})</h2>

      {annonce.encheres.length === 0 ? (
        <p>Aucune enchère pour le moment.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Pseudo</th>
              <th>Montant</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            {annonce.encheres.map((enchere, index) => (
              <tr key={index}>
                <td>{enchere.pseudo}</td>
                <td>{enchere.montant} €</td>
                <td>{new Date(enchere.date).toLocaleString('fr-FR')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}