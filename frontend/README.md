# Frontend

SPA React 19 + TypeScript + Vite da TodoList. O módulo apresenta as tarefas, preserva os dados de formulários rejeitados, comunica resultados de modo acessível e consome exclusivamente a API tipada pelo contrato OpenAPI aprovado.

## Estrutura

```text
frontend/
├── e2e/                         # jornadas, axe e desempenho Playwright
├── scripts/                     # geração e verificação dos tipos OpenAPI
├── src/app/                     # composição da tela
├── src/features/tasks/api/      # cliente HTTP e schema gerado
├── src/features/tasks/components/
├── src/features/tasks/model/
├── src/shared/                  # feedback acessível
└── src/test/                    # setup Vitest/MSW
```

## Configuração

Pré-requisitos: Node.js `20.20.2`, npm `10.8.2`, API e PostgreSQL disponíveis para E2E.

| Variável | Padrão | Uso |
|---|---|---|
| `VITE_API_URL` | `http://localhost:5080/api` | base HTTP consumida pela SPA |

Instale exatamente o lockfile:

```bash
npm ci --prefix frontend
npx --prefix frontend playwright install chromium
```

## Comandos

```bash
npm run dev --prefix frontend          # desenvolvimento em http://localhost:5173
npm run build --prefix frontend        # TypeScript + bundle de produção
npm run test:run --prefix frontend     # Vitest/Testing Library/MSW
npm run test:e2e --prefix frontend     # jornadas, axe e p95 com stack real
npm run generate:api --prefix frontend # regenera schema.ts do contrato aprovado
npm run check:api --prefix frontend    # falha se schema.ts estiver divergente
```

O E2E inicia API e frontend, mas requer o PostgreSQL de `compose.yaml` já saudável e migrado. Os testes de carga usam `docker-compose` localmente e `docker compose` na CI.

## Contrato e limites

`src/features/tasks/api/schema.ts` é gerado de `docs/specs/001-essential-task/contracts/tasks-api.openapi.yaml`; não o edite manualmente. `taskApi.ts` é um cliente `fetch` fino. Regras autoritativas, transações e idempotência pertencem ao backend; o frontend cuida de interação, foco, localidade, atraso derivado e feedback.
