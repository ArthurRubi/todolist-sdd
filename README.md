# TodoList SDD

Aplicação web de tarefas inspirada no Microsoft To Do, criada como projeto de estudo de **Spec-Driven Development (SDD)**.

> Estado atual: fundação documental e operacional. Ainda não há código de produto; a primeira funcionalidade nascerá de uma especificação aprovada.

## Fluxo de desenvolvimento

```text
Ideia → Especificação → Plano técnico → Tarefas e testes → Implementação → Validação
```

Cada etapa produz um artefato versionado e rastreável. As regras completas estão em [docs/sdd-workflow.md](docs/sdd-workflow.md), e os princípios obrigatórios estão em [docs/constitution.md](docs/constitution.md).

## Estrutura do monorepo

```text
.
├── .codex/skills/       # Fluxos SDD reutilizáveis pelo Codex
├── .github/              # Convenções de contribuição no GitHub
├── backend/              # API ASP.NET Core (.NET 10)
├── docs/                 # Constituição, decisões, specs e templates
├── frontend/             # Aplicação React
├── AGENTS.md             # Instruções permanentes para agentes
└── README.md
```

Consulte o README do módulo antes de alterá-lo:

- [Documentação](docs/README.md)
- [Frontend](frontend/README.md)
- [Backend](backend/README.md)

## Stack planejada

- Frontend: React com TypeScript, inicialmente via Vite.
- Backend: ASP.NET Core Web API sobre .NET 10.
- Persistência: PostgreSQL com Entity Framework Core.
- Contrato: OpenAPI, tratado como fronteira explícita entre frontend e backend.
- Testes: Vitest/Testing Library, xUnit e testes de integração; E2E será definido no plano da funcionalidade.

Essas escolhas são uma linha de base, não uma autorização para gerar código. Mudanças relevantes exigem decisão arquitetural registrada.

## Como trabalhar com o Codex

As skills locais podem ser chamadas diretamente:

- `$sdd-idea` — transforma uma conversa em proposta de ideia.
- `$sdd-specify` — cria a especificação funcional.
- `$sdd-plan` — cria o plano técnico a partir da spec aprovada.
- `$sdd-tasks` — deriva tarefas e testes rastreáveis.
- `$sdd-implement` — implementa apenas tarefas prontas.
- `$sdd-validate` — valida a entrega contra a spec e registra evidências.

Exemplo: `Use $sdd-specify para especificar a criação de uma tarefa.`

## Ambiente local conhecido

- .NET SDK: `10.0.300`
- npm: `10.8.2`

A versão do Node.js e as dependências serão fixadas quando o primeiro plano técnico justificar o scaffold.
