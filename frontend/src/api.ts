import type { Annonce, AnnonceDetail } from './types'

// Le backend renvoie ce format quand une regle est refusee.
interface ErreurApi {
  message?: string
  detail?: string
}

async function lireErreur(reponse: Response): Promise<string> {
  try {
    const corps: ErreurApi = await reponse.json()

    return corps.message || corps.detail || 'Erreur ' + reponse.status
  } catch {
    return 'Erreur ' + reponse.status
  }
}

export async function listerAnnonces(statut: string, q: string): Promise<Annonce[]> {
  const params = new URLSearchParams()

  if (statut) {
    params.set('statut', statut)
  }

  if (q.trim()) {
    params.set('q', q.trim())
  }

  const url = params.toString() ? '/api/annonces?' + params.toString() : '/api/annonces'
  const reponse = await fetch(url)

  if (!reponse.ok) {
    throw new Error(await lireErreur(reponse))
  }

  return reponse.json()
}

export async function obtenirAnnonce(id: string): Promise<AnnonceDetail> {
  const reponse = await fetch('/api/annonces/' + id)

  if (!reponse.ok) {
    throw new Error(await lireErreur(reponse))
  }

  return reponse.json()
}

export async function placerEnchere(id: string, pseudo: string, montant: number): Promise<AnnonceDetail> {
  const reponse = await fetch('/api/annonces/' + id + '/encheres', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ pseudo, montant }),
  })

  if (!reponse.ok) {
    throw new Error(await lireErreur(reponse))
  }

  return reponse.json()
}