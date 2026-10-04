import './App.css';
import CandidatoForm from './components/CandidatoForm';

export default function App() {
  return (
    <>
      <header className="topo">
        <div className="topo-conteudo">
          <h1>Processo seletivo CIEE/PR</h1>
          <p>Cadastro de talentos</p>
        </div>
      </header>

      <main className="conteudo">
        <CandidatoForm />
      </main>
    </>
  );
}