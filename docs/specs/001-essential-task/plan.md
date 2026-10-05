# Plano de implementação: Tarefa essencial

**Spec:** [SPEC-001](./spec.md)
**Ideia de origem:** [IDEA-002](../../ideas/002-customizable-task-model.md)
**Decisões:** [ADR-0001](../../architecture/decisions/0001-monorepo-and-platform-baseline.md), [ADR-0002](../../architecture/decisions/0002-modular-vertical-slices-and-versioned-http-contract.md)
**Branch:** `codex/001-essential-task`
**Data:** 2026-10-04
**Status:** Ready for review

## Resumo

Implementar a primeira fatia vertical do TodoList como uma aplicação web de página única. O frontend React oferecerá inclusão rápida, listas de tarefas ativas e concluídas e um painel de detalhes para edição. A API ASP.NET Core concentrará validação, transições de estado e idempotência; o PostgreSQL persistirá a tarefa e seu histórico de estados.

A implementação será um monólito modular pequeno, organizado por funcionalidade e sem camadas ou serviços distribuídos prematuros. O contrato HTTP OpenAPI 3.1 será a fronteira verificável entre frontend e backend.

## Contexto técnico

- **Linguagens/versões:** TypeScript com React 19.3; Node.js 20.20.2 e npm 10.8.2; C# sobre .NET SDK 10.0.300; SQL do PostgreSQL 18.
- **Dependências principais:** Vite 8, ASP.NET Core Minimal APIs, Entity Framework Core 10, Npgsql EF Core 10 e gerador de tipos TypeScript a partir de OpenAPI. As versões de correção serão fixadas pelos arquivos de lock no scaffold.
- **Persistência:** PostgreSQL 18, acessado exclusivamente pela API via EF Core e migrações versionadas.
- **Testes:** xUnit para unidade e integração da API, PostgreSQL real efêmero com Testcontainers, Vitest e Testing Library no frontend e Playwright para aceitação ponta a ponta e acessibilidade.
- **Plataforma alvo:** navegadores evergreen com suporte atual a ES modules; API Linux em contêiner; desenvolvimento local no macOS atual do mantenedor.
- **Metas de desempenho:** resultado percebido em até 1 segundo no percentil 95 para criação, edição e mudança de estado, com até 1.000 tarefas.
- **Restrições:** sem autenticação neste incremento; portanto, a API de escrita não poderá ser exposta publicamente sem proteção externa. Texto simples apenas, datas de prazo sem fuso, instantes em UTC, navegação completa por teclado e nenhuma informação transmitida somente por cor.

## Constitution Check

| Princípio/gate | Resultado | Evidência ou ação |
|---|---|---|
| Spec aprovada | PASS | A [SPEC-001](./spec.md) está `Approved` desde 2026-10-04. |
| Rastreabilidade | PASS | Histórias, requisitos, cenários, contrato, dados e testes permanecem identificados; `tasks.md` deverá referenciar esses IDs. |
| Estratégia de testes | PASS | O plano cobre unidade, integração com PostgreSQL real, contrato, componentes, E2E, acessibilidade e desempenho. |
| Entrega vertical | PASS | A ordem proposta entrega primeiro US-01 e depois US-02/US-03, sempre atravessando banco, API e interface. |
| Simplicidade | PASS | Uma SPA, uma API e um banco; sem mensageria, microserviços, Redux, autenticação ou abstrações genéricas sem necessidade. |

### Verificação apó o desenho

O desenho final permanece compatível com a Constituição. O mecanismo adicional de idempotência existe para atender explicitamente ao caso de resposta incerta e ao NFR-004; o histórico separado existe para atender FR-017 e reabertura. As demais escolhas reduzem complexidade ou tornam os gates verificáveis.

## Pesquisa e decisões

As incertezas de compatibilidade, persistência, contrato e testes foram resolvidas em [research.md](./research.md). O modelo relacional está em [data-model.md](./data-model.md), e o contrato pretendido da API está em [contracts/tasks-api.openapi.yaml](./contracts/tasks-api.openapi.yaml).

Decisões centrais:

- usar Vite 8, compatível com o Node.js 20.20.2 instalado;
- usar Minimal APIs e o suporte OpenAPI nativo do ASP.NET Core 10;
- manter uma única aplicação backend organizada por fatias verticais, com testes em projetos separados;
- usar o PostgreSQL real nos testes de integração, evitando comportamento divergente de banco em memória;
- armazenar cada mudança de estado como evento imutável e atualizar a tarefa na mesma transação;
- tornar criações repetíveis com `Idempotency-Key`, evitando duplicatas apó respostas incertas;
- gerar os tipos do cliente a partir do contrato, sem manter DTOs TypeScript duplicados manualmente.

## Arquitetura e fluxo

### Fronteiras

1. **Frontend:** renderiza e preserva estado de formulário, calcula atraso a partir da data local do navegador, apresenta feedback e envia comandos tipados. Não é a fonte autoritativa das regras.
2. **API:** valida a requisição inteira, normaliza o título, executa transições de estado, controla idempotência e retorna a representação persistida.
3. **Persistência:** garante limites, enums, identidade e unicidade do identificador de idempotência; tarefa e evento de estado mudam atomicamente.
4. **Contrato:** o OpenAPI versionado descreve a fronteira. A implementação da API gera seu documento e a CI compara semanticamente o resultado com o artefato aprovado antes de gerar os tipos do frontend.

### Fluxos principais

- **Criar:** o frontend gera uma chave UUID para a tentativa, envia `POST /api/tasks`, a API valida e insere a tarefa. Uma repetição com a mesma chave e o mesmo conteúdo retorna a tarefa existente; a mesma chave com conteúdo diferente retorna conflito.
- **Editar:** o frontend envia o conjunto completo dos quatro campos editáveis por `PUT`; a API valida tudo antes de alterar qualquer campo e responde com a versão persistida.
- **Mudar estado ativo:** a API aceita apenas `not_started`, `in_progress` ou `blocked`, atualiza o estado atual e acrescenta um evento dentro da mesma transação.
- **Concluir:** a API registra a primeira conclusão uma única vez. Repetir o comando em tarefa já concluída retorna a mesma representação sem novo evento nem novo instante.
- **Reabrir:** a API consulta a transição que originou a conclusão atual, restaura seu estado de origem, limpa somente a conclusão atual e preserva todos os eventos.

### Interface planejada

A tela única terá inclusão rápida por título, regiões distintas para ativas e concluídas e um painel de detalhes acionado por cada item. O painel concentra descrição, prioridade, prazo, status e datas somente de leitura. Validações ficam junto aos campos; sucesso e falha usam uma região anunciada por tecnologia assistiva. O foco retorna a um elemento previsível ao salvar ou fechar o painel.

## Estrutura afetada

```text
frontend/
├── src/
│   ├── app/
│   ├── features/tasks/
│   │   ├── api/
│   │   ├── components/
│   │   ├── model/
│   │   └── styles/
│   ├── shared/
│   └── test/
└── e2e/
backend/
├── TodoList.slnx
├── src/TodoList.Api/
│   ├── Common/
│   ├── Data/
│   └── Features/Tasks/
└── tests/
    ├── TodoList.Api.UnitTests/
    └── TodoList.Api.IntegrationTests/
docs/specs/001-essential-task/
├── plan.md
├── research.md
├── data-model.md
├── contracts/tasks-api.openapi.yaml
├── tasks.md                         # etapa seguinte, ainda inexistente
└── validation.md                    # criado somente na validação
```

O projeto de produção da API não será dividido em bibliotecas `Domain`, `Application` e `Infrastructure` neste incremento. As fronteiras serão pastas e tipos internos, suficientes para o tamanho atual e passíveis de extração quando houver uma necessidade real.

## Dados e migração

Serão criadas as tabelas `tasks` e `task_status_events` conforme [data-model.md](./data-model.md). A primeira migração cria tabelas, restrições e índices; não há dados anteriores a converter.

- `date` representa o prazo sem horário.
- `timestamptz` armazena instantes UTC de criação, atualização, conclusão e eventos.
- status e prioridade usam texto com restrições `CHECK`, mantendo valores legíveis e controlados.
- mudanças de status atualizam a tarefa e inserem o evento em uma transação.
- migrações são aplicadas explicitamente durante a publicação; a API não altera schema automaticamente ao iniciar.

Como esta é a migração inicial, o rollback em ambiente descartável remove a migração/banco. Em um ambiente com dados, exige backup prévio e aplicação consciente da migração de descida; nunca haverá exclusão automática de dados no startup.

## Contratos

O contrato HTTP completo está em [contracts/tasks-api.openapi.yaml](./contracts/tasks-api.openapi.yaml). A base é `/api` e os recursos planejados são:

| Método e caminho | Responsabilidade | Rastreabilidade principal |
|---|---|---|
| `GET /tasks?view=active\|completed` | listar a visualização solicitada | FR-007, FR-009, FR-018 |
| `GET /tasks/{taskId}` | obter detalhes essenciais | FR-009, FR-020 |
| `POST /tasks` | criar com `Idempotency-Key` | FR-001–FR-008, FR-012, NFR-004 |
| `PUT /tasks/{taskId}` | substituir campos editáveis atomicamente | FR-010–FR-012 |
| `PUT /tasks/{taskId}/status` | trocar entre estados ativos | FR-013, FR-017 |
| `POST /tasks/{taskId}/complete` | concluir de forma idempotente | FR-014–FR-015, FR-017 |
| `POST /tasks/{taskId}/reopen` | reabrir para o estado anterior | FR-016–FR-017 |

Datas usam `YYYY-MM-DD`; instantes usam ISO 8601 em UTC. Falhas seguem `application/problem+json`, com erros de campo quando aplicável e um `traceId` para correlação. O contrato não expõe estrutura interna das tabelas nem o histórico nesta primeira interface.

## Estratégia de testes

| Nível | Responsabilidade | Cobertura planejada |
|---|---|---|
| Unidade da API | regras puras e máquina de estados | normalização e limites; valores padrão; conclusão repetida; reabertura para estado anterior; cálculo de datas automáticas |
| Integração API + PostgreSQL | contrato, persistência e atomicidade | AC-01–AC-14; reload; `date` e UTC; restrições; rollback de transação; idempotência com conteúdo igual e conflitante |
| Contrato | impedir divergência entre artefato e execução | comparar OpenAPI gerado pela API ao contrato versionado e gerar tipos TypeScript sem diferença pendente |
| Componentes frontend | interação e estados visuais | preservação de campos inválidos, feedback, remoção de opcionais, texto multilinha, atraso e datas localizadas |
| E2E | jornadas independentes | uma jornada por US-01, US-02 e US-03; atualização da página; separação ativa/concluída; fluxo completo apenas por teclado |
| Acessibilidade | NFR-002 e NFR-003 | axe automatizado, ordem/foco visível, nomes acessíveis, regiões de feedback e verificação de que cor não é o único indicador |
| Desempenho | NFR-001 e SC-005 | base semeada com 1.000 tarefas; amostra repetida de criação, edição e status; registrar percentil 95 ponta a ponta e exigir até 1 segundo no ambiente de validação |

As tarefas de teste serão declaradas antes da implementação correspondente. Testes de integração usarão PostgreSQL efêmero, não o provider EF em memória, para provar tipos, constraints e transações reais.

### Cobertura rastreável

| Jornada/qualidade | IDs cobertos | Evidência planejada |
|---|---|---|
| Criar e visualizar | US-01; AC-01, AC-02, AC-03, AC-04, AC-05; FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, FR-012, FR-018, FR-019, FR-020 | unidade de validação, integração de API/banco, componente de criação e E2E com reload |
| Editar detalhes | US-02; AC-06, AC-07, AC-08, AC-09; FR-002, FR-003, FR-005, FR-006, FR-008, FR-009, FR-010, FR-011, FR-012, FR-018, FR-019, FR-020 | integração de atualização atômica, componentes de formulário e E2E de edição |
| Estado e conclusão | US-03; AC-10, AC-11, AC-12, AC-13, AC-14; FR-004, FR-008, FR-009, FR-013, FR-014, FR-015, FR-016, FR-017, FR-018, FR-019, FR-020 | unidade da máquina de estados, integração transacional e E2E de concluir/reabrir |
| Rapidez percebida | NFR-001; SC-005 | medição repetida com 1.000 tarefas e percentil 95 registrado |
| Acessibilidade | NFR-002, NFR-003; SC-006 | componentes, axe, E2E por teclado e inspeção dos indicadores não cromáticos |
| Integridade | NFR-004; SC-002, SC-003, SC-004 | testes de idempotência, transação, validação integral, persistência e histórico |
| Localização temporal | NFR-005 | testes de serialização `date`, UTC e formatação em pelo menos duas localidades/fusos |
| Usabilidade de criação | SC-001 | sessão de validação com pelo menos 10 participantes; aprovação exige ao menos 9 sucessos em até 20 segundos sem orientação |

SC-002 a SC-006 serão avaliados pelos testes e medições indicados. SC-001 exige evidência humana e não será declarado aprovado apenas com automação; se a amostra não estiver disponível na validação, o veredito deverá registrar explicitamente a exceção.

## Segurança, privacidade e acessibilidade

- A publicação aberta da API fica bloqueada até existir autenticação em spec própria ou uma barreira de acesso provida pela plataforma. Desenvolvimento local e previews privados são permitidos.
- CORS aceitará somente a origem configurada do frontend; não será usado curinga em ambiente publicado.
- Título e descrição serão tratados como texto simples. React fará escaping normal e a aplicação não usará renderização de HTML fornecido pelo usuário.
- Limites de campos e de corpo serão impostos no servidor. Acesso ao banco será parametrizado pelo EF Core.
- Conteúdo de tarefas, chaves e strings de conexão não serão registrados em logs. `EnableSensitiveDataLogging` permanecerá desabilitado fora de testes intencionais.
- Segredos serão fornecidos por variáveis de ambiente ou secret store local; apenas `.env.example` sem credenciais poderá ser versionado.
- Elementos interativos usarão HTML semântico, rótulos, foco visível e ordem previsível. Estado, prioridade, atraso e resultado terão texto ou ícone rotulado além de cor.

## Observabilidade e operação

- Logs estruturados da API registrarão nome da operação, resultado, duração, identificador da tarefa quando seguro e `traceId`, sem conteúdo do usuário.
- Respostas de erro incluirão o `traceId`; o frontend mostrará mensagem compreensível e manterá esse identificador disponível para diagnóstico.
- `/health/live` comprovará que o processo responde; `/health/ready` verificará também a conexão com o PostgreSQL.
- A configuração incluirá conexão do banco, origem CORS e URL da API no frontend, sempre com exemplos seguros e validação no startup.
- Não será adicionado APM, fila ou serviço externo neste incremento. Logs, health checks e medidas de teste atendem ao risco atual.

## Entrega e rollback

Ordem segura planejada:

1. criar scaffold, infraestrutura local e testes-base;
2. aplicar a migração inicial;
3. entregar US-01 verticalmente e demonstrá-la;
4. entregar US-02 e US-03 com seus testes;
5. executar contrato, builds, testes, acessibilidade e desempenho;
6. atualizar os READMEs operacionais com comandos reais;
7. produzir a revisão da implementação, sem publicar a API abertamente.

Não é necessário feature flag: não existe versão anterior nem consumidor externo. Se o frontend precisar ser revertido, a API permanece compatível com o contrato aprovado. Se API ou migração precisarem ser revertidas em ambiente com dados, primeiro restaura-se o binário compatível e usa-se backup/migração de descida planejada; nenhuma automação destrutiva será executada implicitamente.

## Complexidade excepcional

| Exceção | Por que é necessária | Alternativa mais simples rejeitada |
|---|---|---|
| Registro separado de eventos de estado | FR-016 e FR-017 exigem restaurar o estado anterior e preservar conclusões passadas | Manter apenas status e `completed_at` apagaria o histórico ao reabrir |
| Chave de idempotência persistida na criação | O caso de resposta incerta e o NFR-004 proíbem duplicata silenciosa | Desabilitar o botão somente no frontend não protege contra retry de rede |
| PostgreSQL em contêiner nos testes de integração | Tipos `date`/`timestamptz`, constraints e transações precisam ser verificados no banco real | Banco em memória não reproduz essas garantias |

## Aprovação

**Decisão:** Pendente
**Aprovado por:** pendente
**Data:** pendente

## Registro de decisões

- 2026-10-04: plano criado a partir da SPEC-001 aprovada.
- 2026-10-04: pesquisa, modelo de dados, contrato OpenAPI e ADR-0002 adicionados.
- 2026-10-04: Constitution Check executado antes e depois do desenho, sem desvios.
- 2026-10-04: plano movido para `Ready for review`; nenhuma tarefa ou implementação foi iniciada.
