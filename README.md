# TodoList SDD

Aplicação web de tarefas inspirada no Microsoft To Do e construída como estudo prático de **Spec-Driven Development (SDD)**. A primeira fatia permite criar, consultar, editar, concluir, reabrir e acompanhar o estado de tarefas com persistência em PostgreSQL.

## Fluxo SDD

```text
Ideia → Especificação → Plano técnico → Tarefas e testes → Implementação → Validação
```

A documentação aprovada é a fonte de verdade. Consulte [docs/sdd-workflow.md](docs/sdd-workflow.md), [docs/constitution.md](docs/constitution.md) e a [SPEC-001](docs/specs/001-essential-task/spec.md) antes de alterar comportamento.

## Estrutura

```text
.
├── .codex/skills/       # workflows SDD reutilizáveis
├── .github/workflows/   # CI reproduzível
├── backend/              # API ASP.NET Core 10 e testes xUnit
├── docs/                 # especificações, decisões e rastreabilidade
├── frontend/             # SPA React 19 e testes Vitest/Playwright
├── compose.yaml          # PostgreSQL 18 para desenvolvimento/E2E
└── AGENTS.md             # regras permanentes para agentes
```

Detalhes operacionais ficam nos READMEs de [frontend](frontend/README.md) e [backend](backend/README.md).

## Pré-requisitos

- .NET SDK `10.0.300` (fixado em `global.json`);
- Node.js `20.20.2` e npm `10.8.2`;
- Docker com Compose;
- navegador Chromium instalado pelo Playwright para E2E.

## Início rápido

```bash
cp .env.example .env
docker-compose -f compose.yaml up -d --wait
dotnet tool restore
dotnet restore backend/TodoList.slnx --locked-mode
npm ci --prefix frontend
dotnet ef database update --project backend/src/TodoList.Api/TodoList.Api.csproj
dotnet run --project backend/src/TodoList.Api/TodoList.Api.csproj --urls http://localhost:5080
```

Em outro terminal:

```bash
npm run dev --prefix frontend
```

Abra `http://localhost:5173`. Use `docker compose` no lugar de `docker-compose` quando sua instalação oferecer apenas o plugin moderno.

## Verificação

```bash
npm run check:api --prefix frontend
dotnet format backend/TodoList.slnx --verify-no-changes --no-restore
dotnet test backend/TodoList.slnx --no-restore --disable-build-servers -m:1
npm run test:run --prefix frontend
npm run build --prefix frontend
npm run test:e2e --prefix frontend
```

> A API não possui autenticação nesta spec. Não a publique em acesso aberto sem uma proteção externa ou uma futura especificação de autenticação.

## Skills SDD

- `$sdd-idea` — estrutura a necessidade;
- `$sdd-specify` — cria a especificação funcional;
- `$sdd-plan` — define o plano técnico;
- `$sdd-tasks` — deriva tarefas e testes;
- `$sdd-implement` — executa somente tarefas aprovadas;
- `$sdd-validate` — valida a entrega com evidências.

Cada etapa termina em `Ready for review` e exige aprovação humana explícita antes da próxima.
