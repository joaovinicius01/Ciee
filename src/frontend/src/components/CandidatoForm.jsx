import { useState } from 'react';
import api from '../services/api';
import './CandidatoForm.css';

const CAMPOS_VAZIOS = {
  nomeCompleto: '',
  email: '',
  telefone: '',
  areaInteresse: '',
  resumoProfissional: '',
};

const LIMITE_PDF_BYTES = 5 * 1024 * 1024;
const REGEX_EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

// Mesmos limites do CadastrarCandidatoRequest no backend
const LIMITES = {
  nomeCompleto: 150,
  email: 150,
  telefone: 20,
  areaInteresse: 100,
  resumoProfissional: 2000,
};

// Máscara (41) 99999-9999 enquanto o usuário digita
const formatarTelefone = (valor) => {
  const d = valor.replace(/\D/g, '').slice(0, 11);
  if (d.length === 0) return '';
  if (d.length <= 2) return `(${d}`;
  if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`;
  if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`;
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
};

// Para o telefone vindo do PDF: só aplica a máscara se tiver DDD + número completo
const normalizarTelefoneImportado = (valor) => {
  const digitos = (valor || '').replace(/\D/g, '');
  return digitos.length === 10 || digitos.length === 11 ? formatarTelefone(digitos) : valor || '';
};

const validar = (dados) => {
  const erros = {};

  if (!dados.nomeCompleto.trim()) {
    erros.nomeCompleto = 'Informe o nome completo.';
  }

  if (!dados.email.trim()) {
    erros.email = 'Informe o e-mail.';
  } else if (!REGEX_EMAIL.test(dados.email.trim())) {
    erros.email = 'Esse e-mail parece incompleto. Exemplo: nome@dominio.com';
  }

  // Telefone é opcional, mas se preenchido precisa ter DDD + número
  if (dados.telefone.trim()) {
    const digitosTelefone = dados.telefone.replace(/\D/g, '');
    if (digitosTelefone.length < 10 || digitosTelefone.length > 11) {
      erros.telefone = 'Informe o telefone com DDD. Exemplo: (41) 99999-9999';
    }
  }

  return erros;
};

// Converte { NomeCompleto: ["msg"] } do ASP.NET em { nomeCompleto: "msg" }
const mapearErrosDoServidor = (errors) => {
  const mapa = {};
  Object.entries(errors).forEach(([campo, mensagens]) => {
    const chave = campo.charAt(0).toLowerCase() + campo.slice(1);
    mapa[chave] = Array.isArray(mensagens) ? mensagens[0] : String(mensagens);
  });
  return mapa;
};

function Campo({ id, label, obrigatorio, erro, children }) {
  return (
    <div className="campo">
      <label htmlFor={id}>
        {label}
        {obrigatorio && (
          <span className="obrigatorio" aria-hidden="true">
            {' '}
            *
          </span>
        )}
      </label>
      {children}
      {erro && (
        <p className="campo-erro" id={`${id}-erro`} role="alert">
          {erro}
        </p>
      )}
    </div>
  );
}

export default function CandidatoForm({ onCadastrado }) {
  const [formData, setFormData] = useState(CAMPOS_VAZIOS);
  const [erros, setErros] = useState({});
  const [mensagem, setMensagem] = useState(null);
  const [nomeArquivo, setNomeArquivo] = useState('');
  const [loadingPdf, setLoadingPdf] = useState(false);
  const [loadingSalvar, setLoadingSalvar] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    const novoValor = name === 'telefone' ? formatarTelefone(value) : value;

    setFormData((prev) => ({ ...prev, [name]: novoValor }));

    // Limpa o erro do campo assim que o usuário volta a digitar
    if (erros[name]) {
      setErros((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  // Ao sair do campo, só valida o formato se já tiver algo digitado.
  // "Campo obrigatório" aparece apenas ao tentar cadastrar.
  const handleBlur = (e) => {
    const { name, value } = e.target;
    if (!value.trim()) return;
    const erro = validar(formData)[name];
    setErros((prev) => ({ ...prev, [name]: erro }));
  };

  const propsCampo = (nome) => ({
    id: `campo-${nome}`,
    name: nome,
    value: formData[nome],
    onChange: handleChange,
    onBlur: handleBlur,
    maxLength: LIMITES[nome],
    className: erros[nome] ? 'invalido' : undefined,
    'aria-invalid': erros[nome] ? 'true' : undefined,
    'aria-describedby': erros[nome] ? `campo-${nome}-erro` : undefined,
  });

  const handlePdfUpload = async (e) => {
    const input = e.target;
    const file = input.files[0];
    if (!file) return;

    const ehPdf = file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');

    if (!ehPdf) {
      setMensagem({ tipo: 'erro', texto: 'Envie um arquivo em formato PDF.' });
      input.value = '';
      return;
    }

    if (file.size > LIMITE_PDF_BYTES) {
      setMensagem({ tipo: 'erro', texto: 'O arquivo tem mais de 5 MB. Escolha um PDF menor.' });
      input.value = '';
      return;
    }

    const data = new FormData();
    data.append('arquivo', file);

    try {
      setLoadingPdf(true);
      setNomeArquivo(file.name);
      setMensagem({ tipo: 'info', texto: 'Lendo o currículo...' });

      const response = await api.post('/candidatos/extrair-pdf', data, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      setFormData({
        nomeCompleto: response.data.nomeCompleto || '',
        email: response.data.email || '',
        telefone: normalizarTelefoneImportado(response.data.telefone),
        areaInteresse: response.data.areaInteresse || '',
        resumoProfissional: response.data.resumoProfissional || '',
      });
      setErros({});

      setMensagem({
        tipo: 'sucesso',
        texto: 'Dados importados do currículo. Revise os campos antes de cadastrar.',
      });
    } catch {
      setMensagem({
        tipo: 'erro',
        texto: 'Não foi possível ler esse PDF. Ele pode estar corrompido ou ilegível. Preencha os campos manualmente.',
      });
    } finally {
      setLoadingPdf(false);
      // Permite escolher o mesmo arquivo de novo depois de um erro
      input.value = '';
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const errosEncontrados = validar(formData);
    if (Object.keys(errosEncontrados).length > 0) {
      setErros(errosEncontrados);
      setMensagem({ tipo: 'erro', texto: 'Corrija os campos destacados para continuar.' });

      const primeiroComErro = Object.keys(errosEncontrados)[0];
      document.getElementById(`campo-${primeiroComErro}`)?.focus();
      return;
    }

    try {
      setLoadingSalvar(true);
      setMensagem(null);

      await api.post('/candidatos', {
        ...formData,
        nomeCompleto: formData.nomeCompleto.trim(),
        email: formData.email.trim(),
      });

      setFormData(CAMPOS_VAZIOS);
      setErros({});
      setNomeArquivo('');
      setMensagem({ tipo: 'sucesso', texto: 'Candidato cadastrado.' });

      if (onCadastrado) onCadastrado();
    } catch (error) {
      // 409 = e-mail já cadastrado
      if (error.response?.status === 409) {
        setErros({ email: 'Este e-mail já está cadastrado.' });
        setMensagem({ tipo: 'erro', texto: 'Já existe um candidato cadastrado com este e-mail.' });
        document.getElementById('campo-email')?.focus();
        return;
      }

      const resposta = error.response?.data;

      if (resposta?.errors && typeof resposta.errors === 'object' && !Array.isArray(resposta.errors)) {
        setErros(mapearErrosDoServidor(resposta.errors));
        setMensagem({ tipo: 'erro', texto: 'Corrija os campos destacados para continuar.' });
      } else {
        const texto =
          resposta?.mensagem || resposta?.erro || 'Não foi possível cadastrar o candidato. Tente novamente.';
        setMensagem({ tipo: 'erro', texto: typeof texto === 'string' ? texto : JSON.stringify(texto) });
      }
    } finally {
      setLoadingSalvar(false);
    }
  };

  return (
    <section className="cartao">
      <h2>Cadastro de candidato</h2>

      {mensagem && (
        <div
          className={`aviso aviso-${mensagem.tipo}`}
          role={mensagem.tipo === 'erro' ? 'alert' : 'status'}
        >
          {mensagem.texto}
        </div>
      )}

      <label className={`upload${loadingPdf ? ' desativado' : ''}`}>
        <input type="file" accept=".pdf,application/pdf" onChange={handlePdfUpload} disabled={loadingPdf} />
        <span className="upload-botao">Escolher PDF</span>
        <span className="upload-texto">
          <p className="upload-titulo">Preencher a partir de um currículo</p>
          <p className="upload-detalhe">
            {loadingPdf ? (
              <>
                <span className="spinner" aria-hidden="true" />
                Lendo {nomeArquivo}...
              </>
            ) : nomeArquivo ? (
              nomeArquivo
            ) : (
              'PDF de até 5 MB. Você poderá editar os dados depois.'
            )}
          </p>
        </span>
      </label>

      <form onSubmit={handleSubmit} noValidate>
        <Campo id="campo-nomeCompleto" label="Nome completo" obrigatorio erro={erros.nomeCompleto}>
          <input type="text" autoComplete="name" placeholder="Ex.: Maria da Silva" {...propsCampo('nomeCompleto')} />
        </Campo>

        <Campo id="campo-email" label="E-mail" obrigatorio erro={erros.email}>
          <input type="email" autoComplete="email" placeholder="nome@dominio.com" {...propsCampo('email')} />
        </Campo>

        <Campo id="campo-telefone" label="Telefone" erro={erros.telefone}>
          <input type="tel" autoComplete="tel" placeholder="(41) 99999-9999" {...propsCampo('telefone')} />
        </Campo>

        <Campo id="campo-areaInteresse" label="Área ou cargo de interesse" erro={erros.areaInteresse}>
          <input type="text" placeholder="Ex.: Desenvolvimento Back-End" {...propsCampo('areaInteresse')} />
        </Campo>

        <Campo id="campo-resumoProfissional" label="Resumo profissional" erro={erros.resumoProfissional}>
          <textarea rows="4" placeholder="Conte brevemente sua experiência." {...propsCampo('resumoProfissional')} />
        </Campo>

        <button type="submit" className="botao-principal" disabled={loadingSalvar || loadingPdf}>
          {loadingSalvar ? 'Salvando...' : 'Cadastrar candidato'}
        </button>
      </form>
    </section>
  );
}