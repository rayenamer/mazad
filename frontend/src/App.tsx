import { Route, Routes } from 'react-router-dom'
import AnnoncesPage from './pages/AnnoncesPage'
import AnnonceDetailPage from './pages/AnnonceDetailPage'

export default function App() {
  return (
    <div>
      <Routes>
        <Route path="/" element={<AnnoncesPage />} />
        <Route path="/annonces/:id" element={<AnnonceDetailPage />} />
      </Routes>
    </div>
  )
}