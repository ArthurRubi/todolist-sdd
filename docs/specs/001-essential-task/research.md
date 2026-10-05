# Pesquisa técnica: SPEC-001 — Tarefa essencial

**Data:** 2026-10-04
**Status:** Concluída e aprovada com o plano

## Objetivo

Resolver somente as incertezas que afetam o plano da primeira fatia vertical: compatibilidade do ambiente, forma da aplicação, persistência, contrato, idempotência e testes confiáveis.

## Decisões

### 1. Runtime e ferramenta do frontend

**Decisão:** React 19.3 com TypeScript sobre Vite 8.x. Usar o Node.js 20.20.2 e npm 10.8.2 já instalados.

**Motivo:** a linha Vite 8 requer Node.js 20.19 ou superior, logo o ambiente atual é compatível. React 19.3 é a linha estável atual. O patch exato de cada pacote será resolvido durante o scaffold e fixado em `package-lock.json`, em vez de duplicado em documentação operacional.

**Alternativas rejeitadas:** Create React App, por não ser a linha de ferramenta planejada no projeto; framework com SSR, porque a tela interativa de uma aplicação pessoal não exige renderização no servidor neste incremento.

**Fontes:**

- [React: versões](https://react.dev/versions)
- [React 19.3](https://react.dev/blog/2026/09/09/react-19-3)
- [Vite 8: suporte a Node.js](https://vite.dev/blog/announcing-vite8#node-js-support)
- [Vite: versões suportadas](https://vite.dev/releases)

### 2. API, OpenAPI e persistência

**Decisão:** ASP.NET Core Minimal APIs em .NET 10, OpenAPI nativo, EF Core 10 e Npgsql EF Core 10 sobre PostgreSQL 18.

**Motivo:** o SDK 10.0.300 está instalado; EF Core 10 é LTS e exige .NET 10; o provider Npgsql 10 é compatível com EF Core 10. O ASP.NET Core 10 gera OpenAPI 3.1 nativamente. PostgreSQL 18 é a versão estável atual e preserva portabilidade entre desenvolvimento e hospedagem futura.

**Alternativas rejeitadas:** SQLite, por introduzir diferenças de tipos e concorrência antes da publicação; SQL Server, por não melhorar os requisitos desta spec; controllers, porque os poucos endpoints não precisam dessa estrutura adicional.

**Fontes:**

- [EF Core 10](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [OpenAPI no ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0)
- [Provider Npgsql para EF Core](https://www.npgsql.org/efcore/)
- [Npgsql EF Core 10.0.3](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/)
- [PostgreSQL 18](https://www.postgresql.org/docs/18/)

### 3. Forma da arquitetura

**Decisão:** uma SPA e uma API modular, com código agrupado por funcionalidade; um projeto de produção no backend e projetos separados apenas para testes.

**Motivo:** a SPEC-001 possui um único agregado e nenhum requisito de distribuição. Fatias verticais mantêm endpoint, validação e regra próximos, enquanto projetos de teste separados isolam dependências e ciclos de execução.

**Alternativas rejeitadas:** microserviços; CQRS com mediator; quatro projetos em camadas; Redux. Nenhuma dessas opções resolve um risco presente na spec.

### 4. Contrato como fronteira verificável

**Decisão:** manter o contrato pretendido em `contracts/tasks-api.openapi.yaml`, gerar OpenAPI da API no build, comparar os documentos semanticamente e gerar tipos TypeScript a partir do contrato aprovado.

**Motivo:** isso torna o contrato rastreável sem duplicar DTOs manualmente. Uma comparação semântica evita falsos erros causados apenas por ordenação ou formatação YAML/JSON.

**Alternativas rejeitadas:** escrever tipos dos dois lados; usar o documento dinâmico sem versioná-lo; gerar um SDK completo, que seria peso desnecessário para sete operações.

### 5. Idempotência de criação

**Decisão:** `POST /api/tasks` exige `Idempotency-Key` UUID. A API persiste a chave com restrição única. Repetição com mesmo conteúdo devolve a tarefa original; com conteúdo diferente devolve `409 Conflict`.

**Motivo:** desabilitar o botão evita cliques locais, mas não cobre timeout seguido de retry. A chave persistida atende ao caso-limite e ao NFR-004 mesmo quando a primeira resposta se perde.

**Alternativas rejeitadas:** confiar apenas na interface; usar título como chave natural; aceitar duplicata e avisar depois.

### 6. Histórico e reabertura

**Decisão:** armazenar o status atual na tarefa e cada transição em `task_status_events`. Conclusão e reabertura modificam ambos dentro de uma transação.

**Motivo:** o status atual deixa leituras simples; os eventos preservam o estado anterior e todas as conclusões. `completed_at` representa apenas a conclusão atual, enquanto o histórico permanece imutável.

**Alternativas rejeitadas:** derivar sempre o estado de todos os eventos, por complexidade de leitura; guardar apenas o último estado anterior, pois apagaria o histórico exigido.

### 7. Teste de persistência

**Decisão:** testes de integração iniciam um PostgreSQL efêmero com Testcontainers e exercitam a API em processo.

**Motivo:** constraints, transações e mapeamentos `date`/`timestamptz` precisam do mecanismo real. O módulo oficial de PostgreSQL do Testcontainers fornece ciclo de vida isolado compatível com xUnit.

**Alternativas rejeitadas:** provider EF em memória, mocks de `DbContext` e banco compartilhado do desenvolvedor.

**Fonte:** [Testcontainers for .NET: PostgreSQL](https://dotnet.testcontainers.org/modules/postgres/)

## Dúvidas adiadas conscientemente

- provedor de hospedagem e topologia pública: decidir quando autenticação e publicação entrarem no escopo;
- paginação, busca e filtros: não necessários para o limite atual de 1.000 tarefas;
- concorrência otimista entre usuários: o incremento é de uso individual e sem contas;
- exibição do histórico na interface: a spec exige preservação, não uma jornada de consulta;
- design visual definitivo: será detalhado na implementação dentro dos requisitos de acessibilidade, sem criar um sistema de design prematuro.
