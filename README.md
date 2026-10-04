# CIEE/PR - Desafio Técnico

Aplicação para cadastro de candidatos com **extração automática de dados a partir de currículos em PDF**. O backend é uma API REST em **.NET 10** (Clean Architecture e conceitos de DDD) e o frontend é um formulário em **React + Vite**.

---

## Tecnologias

| Área | Tecnologia |
| :--- | :--- |
| **Backend** | C#, .NET 10, ASP.NET Core Web API |
| **Arquitetura** | Clean Architecture em 4 camadas (Domain, Application, Infrastructure, Api) |
| **Banco de dados** | SQL Server, Entity Framework Core 9 |
| **Leitura de PDF** | PdfPig 0.1.8 (`ContentOrderTextExtractor`) + Regex |
| **Proteção** | Rate Limiting global (Fixed Window) |
| **Documentação da API** | Swagger / OpenAPI |
| **Frontend** | React + Vite |
| **Testes** | xUnit |

---

## Estrutura do repositório

```text
.
├── src/
│   ├── Ciee.Api              # Controllers, CORS, Rate Limiting, DI e Swagger
│   ├── Ciee.Application      # Serviços de aplicação, DTOs e regras de fluxo
│   ├── Ciee.Domain           # Entidades, Value Objects e interfaces de repositório
│   ├── Ciee.Infrastructure   # EF Core, repositórios, migrations e PdfPigService
│   ├── database/             # Scripts auxiliares do banco
│   └── frontend/             # Aplicação React + Vite
│       └── src/
│           ├── components/   # CandidatoForm (formulário e importação de PDF)
│           └── services/     # api.js (cliente HTTP da API)
├── Ciee.UnitTests/           # Testes unitários com xUnit e fixtures de PDF
├── database_script.sql       # Script SQL completo (gerado a partir das migrations)
├── Ciee.slnx
└── README.md
```

No backend, a dependência aponta sempre para dentro: `Api → Application → Domain`, e `Infrastructure` implementa as interfaces definidas em `Domain` e `Application`.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (versão LTS) e npm
- SQL Server local (Express ou LocalDB)
- Ferramenta do EF Core: `dotnet tool install --global dotnet-ef`

---

## Como executar

Rode todos os comandos a partir da **raiz do repositório**, salvo indicação contrária.

### 1. Configurar a string de conexão

No projeto `src/Ciee.Api`, edite o `appsettings.json` e aponte para a sua instância do SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR\\SQLEXPRESS;Database=CieeDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Criar a estrutura do banco

**Opção A: migrations do EF Core (recomendada).** Cria o banco `CieeDb` e todas as tabelas e índices:

```bash
dotnet ef database update -p src/Ciee.Infrastructure -s src/Ciee.Api
```

**Opção B: script SQL.** Crie um banco vazio chamado `CieeDb` no SQL Server e execute o arquivo `database_script.sql` (o script não inclui o `CREATE DATABASE`).

### 3. Executar a API

```bash
dotnet run --project src/Ciee.Api/Ciee.Api.csproj
```

Em ambiente de desenvolvimento o Swagger fica disponível em `/swagger`. A URL e a porta em que a API sobe aparecem no terminal (e ficam definidas em `src/Ciee.Api/Properties/launchSettings.json`).

### 4. Executar o frontend

Em outro terminal:

```bash
cd src/frontend
npm install
npm run dev
```

O frontend sobe em `http://localhost:5173`. O endereço da API usado pelo front fica em `src/frontend/src/services/api.js`; se a sua API subir em outra porta, ajuste a `baseURL` nesse arquivo.

> A API libera CORS para qualquer origem em desenvolvimento, então o front se comunica com ela sem configuração adicional.

### Resumo (dois terminais)

```bash
# Terminal 1: API
dotnet run --project src/Ciee.Api/Ciee.Api.csproj

# Terminal 2: Frontend
cd src/frontend && npm run dev
```

---

## Executando os testes unitários

Os testes cobrem o Value Object `Email`, o serviço de leitura de PDF (incluindo arquivos corrompidos) e as regras do cadastro, sem depender de banco de dados real. Para rodar exibindo os detalhes no terminal:

```bash
dotnet test --logger "console;verbosity=detailed"
```

O frontend não possui testes automatizados.

---

## Frontend

Formulário de cadastro de candidato com:

- **Importação de currículo em PDF**: o usuário envia o arquivo (até 5 MB) e os campos são preenchidos automaticamente, podendo ser editados antes de salvar.
- **Validação por campo**: mensagens de erro abaixo de cada campo, máscara de telefone e tratamento das respostas de erro da API.
- **Aviso de e-mail já cadastrado**: quando a API responde `409`, o front destaca o campo de e-mail.

Se o PDF não puder ser lido, o usuário continua podendo preencher tudo manualmente.

---

## Endpoints

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/candidatos` | Cadastra um candidato |
| `GET` | `/api/candidatos` | Lista todos os candidatos |
| `GET` | `/api/candidatos/{id}` | Busca um candidato pelo ID (GUID) |
| `POST` | `/api/candidatos/extrair-pdf` | Recebe um currículo em PDF (até 5 MB) e devolve os dados extraídos |

### Cadastro de candidato (`POST /api/candidatos`)

| Campo | Obrigatório | Regras |
| :--- | :---: | :--- |
| `nomeCompleto` | Sim | Até 150 caracteres |
| `email` | Sim | E-mail válido, até 150 caracteres, **único** (comparado sem diferenciar maiúsculas) |
| `telefone` | Não | Se informado, 10 ou 11 dígitos (DDD + número), até 20 caracteres |
| `areaInteresse` | Não | Até 100 caracteres |
| `resumoProfissional` | Não | Até 2000 caracteres |

A unicidade do e-mail é garantida em duas camadas: checagem no serviço (mensagem clara) e índice único no banco (protege contra cadastros simultâneos).

### Exemplo: extração de dados de um PDF

```bash
curl -X POST 'https://localhost:5001/api/Candidatos/extrair-pdf' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'arquivo=@curriculo.pdf;type=application/pdf'
```

Resposta:

```json
{
  "nomeCompleto": "João Vinicius da Silva Lima",
  "email": "joao.vslwwe@gmail.com",
  "telefone": "(41) 99967-8612",
  "areaInteresse": "Desenvolvimento Back-End (.NET)",
  "resumoProfissional": "Desenvolvedor Back-End focado no ecossistema .NET (C#) ..."
}
```

O retorno serve para pré-preencher o formulário do front-end; o cadastro final é feito depois via `POST /api/candidatos`. Se o PDF for corrompido ou ilegível, a API não falha: devolve `200` com os campos não extraídos vazios para que o usuário preencha manualmente na interface.

### Respostas de erro

| Status | Quando ocorre |
| :--- | :--- |
| `400` | Dados inválidos (campo obrigatório ausente, e-mail inválido, tamanho excedido), arquivo ausente, não-PDF ou acima de 5 MB |
| `404` | Candidato não encontrado |
| `409` | Já existe um candidato cadastrado com o e-mail informado |
| `429` | Limite de requisições excedido |
| `500` | Erro interno (os detalhes técnicos só são exibidos em ambiente de desenvolvimento) |

---

## Rate Limiting

Limite global por IP: **30 requisições a cada 10 segundos** (janela fixa). Ao exceder, a API responde `429` com:

```json
{ "erro": "Muitas requisições. Tente novamente mais tarde." }
```

---

## Como funciona a extração do PDF

O `PdfPigService` lê o texto de cada página com `ContentOrderTextExtractor`, que respeita a ordem visual e preserva as quebras de linha, e aplica heurísticas:

- **Nome**: primeira linha entre as 5 iniciais que parece um nome (só letras, 2 a 7 palavras, sem dígitos, e-mail ou link). Preposições ficam em minúsculas.
- **E-mail e telefone**: Regex, buscando primeiro no cabeçalho (10 primeiras linhas) e depois no documento todo. O telefone aceita DDD com ou sem parênteses.
- **Resumo**: texto da seção "Resumo", "Objetivo" ou "Perfil" até o próximo título em caixa alta (máximo de 500 caracteres).
- **Área de interesse**: inferida por palavras-chave do texto.

### Limitações conhecidas

- PDFs escaneados (imagem) não têm texto extraível; seria necessário OCR.
- Currículos com duas colunas ou layouts incomuns podem trocar a ordem das linhas e afetar nome e resumo.
- As heurísticas foram ajustadas para currículos em português com cabeçalho convencional (nome no topo).

---

## Documentação do desenvolvimento

Decisões técnicas, uso de IA, dificuldades e melhorias futuras estão detalhados no arquivo [`DESENVOLVIMENTO.md`](DESENVOLVIMENTO.md).