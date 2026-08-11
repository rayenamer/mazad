export interface Annonce {
  id: string
  titre: string
  description: string
  prixDepart: number
  pasEnchere: number
  dateFin: string
  prixActuel: number
  montantMinimum: number
  nombreEncheres: number
  statut: string
}

export interface Enchere {
  pseudo: string
  montant: number
  date: string
}

export interface AnnonceDetail extends Annonce {
  encheres: Enchere[]
}