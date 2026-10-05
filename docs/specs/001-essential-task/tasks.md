# Tarefas: Tarefa essencial

**Spec:** [SPEC-001](./spec.md)
**Plano:** [Plano da SPEC-001](./plan.md)
**Contrato:** [Tasks API](./contracts/tasks-api.openapi.yaml)
**Planejamento:** Approved
**Implementação:** In progress

## Formato

`- [ ] TNNN [P?] [US-NN] [FR/NFR/AC/SC-NNN] Descrição com caminho exato; dependências; resultado verificável.`

- `[P]` significa que a tarefa pode ser executada simultaneamente com outras marcadas na mesma etapa porque não depende delas nem modifica os mesmos arquivos.
- `[SETUP]` identifica scaffold ou infraestrutura compartilhada justificada pelo plano e pelos ADRs.
- Testes de comportamento aparecem antes da implementação que devem conduzir.
- Durante a execução, cada checkbox só será marcado depois de anexar uma evidência curta no registro de execução.

## Fase 1 — Setup mínimo

- [ ] T001 [P] [SETUP] [ADR-0001] Criar `global.json`, `backend/TodoList.slnx`, `backend/Directory.Packages.props`, `backend/src/TodoList.Api/TodoList.Api.csproj`, `backend/tests/TodoList.Api.UnitTests/TodoList.Api.UnitTests.csproj` e `backend/tests/TodoList.Api.IntegrationTests/TodoList.Api.IntegrationTests.csproj`, fixando .NET 10 e dependências aprovadas; `dotnet build backend/TodoList.slnx` deve concluir.
- [ ] T002 [P] [SETUP] [ADR-0001] Inicializar React 19.3 + TypeScript + Vite 8 em `frontend/package.json`, `frontend/package-lock.json`, `frontend/index.html`, `frontend/tsconfig*.json`, `frontend/vite.config.ts` e `frontend/src/main.tsx`, sem interface de produto; `npm run build` deve concluir.
- [ ] T003 [P] [SETUP] [FR-018] Criar `compose.yaml`, `.env.example` e as exclusões necessárias em `.gitignore` para um PostgreSQL 18 local com volume nomeado e credenciais apenas de desenvolvimento; `docker compose config` deve validar a configuração sem iniciar serviços.
- [ ] T004 [P] [SETUP] [NFR-004] Preparar a infraestrutura xUnit, `WebApplicationFactory` e PostgreSQL Testcontainers em `backend/tests/TodoList.Api.IntegrationTests/Infrastructure/`, incluindo limpeza isolada por teste; depende de T001 e T003; um teste de infraestrutura deve iniciar e encerrar o banco efêmero.
- [ ] T005 [P] [SETUP] [NFR-002] Configurar Vitest, Testing Library, MSW e Playwright em `frontend/vitest.config.ts`, `frontend/playwright.config.ts`, `frontend/src/test/` e `frontend/e2e/`; depende de T002; testes mínimos dos dois runners devem ser descobertos sem erro de configuração.

**Checkpoint:** os dois módulos compilam, os runners descobrem testes e o PostgreSQL local/efêmero possui configuração reproduzível; ainda não existe comportamento de produto.

## Fase 2 — Fundação bloqueante

### Testes primeiro

- [ ] T006 [P] [US-01] [FR-008] [FR-018] [NFR-004] Escrever testes inicialmente falhos da migração, constraints, tipos `date`/`timestamptz`, chave idempotente e transação tarefa/evento em `backend/tests/TodoList.Api.IntegrationTests/Data/DatabaseSchemaTests.cs`; depende de T004.
- [ ] T007 [P] [US-01] [FR-019] [NFR-004] Escrever testes inicialmente falhos para `application/problem+json` com `traceId`, CORS configurável, `/health/live` e `/health/ready` em `backend/tests/TodoList.Api.IntegrationTests/Common/ApplicationInfrastructureTests.cs`; depende de T004.
- [ ] T008 [P] [US-01] [FR-004] [FR-005] Escrever teste inicialmente falho de geração dos enums e DTOs TypeScript a partir do OpenAPI em `frontend/src/features/tasks/api/schema.contract.test.ts`; depende de T005 e do contrato aprovado.

### Implementação da fundação

- [ ] T009 [US-01] [FR-004] [FR-005] [FR-008] [FR-017] [FR-018] Implementar enums, `TaskItem`, `TaskStatusEvent`, `TodoListDbContext`, mapeamentos e migração inicial em `backend/src/TodoList.Api/Features/Tasks/`, `backend/src/TodoList.Api/Data/` e `backend/src/TodoList.Api/Migrations/`; depende de T006; `DatabaseSchemaTests` deve passar.
- [ ] T010 [US-01] [FR-019] [NFR-004] Configurar DI, banco, Problem Details, CORS, health checks, logging estruturado básico e OpenAPI 3.1 em `backend/src/TodoList.Api/Program.cs` e `backend/src/TodoList.Api/Common/`; depende de T007 e T009; `ApplicationInfrastructureTests` deve passar.
- [ ] T011 [P] [US-01] [FR-004] [FR-005] Adicionar o comando de geração de tipos em `frontend/package.json`, o script em `frontend/scripts/generate-api-types.mjs` e a saída versionada em `frontend/src/features/tasks/api/schema.ts`; depende de T008; regenerar não pode deixar diferença no Git.
- [ ] T012 [SETUP] [ADR-0002] Executar os testes de fundação e registrar em `docs/specs/001-essential-task/tasks.md` os comandos e resultados; depende de T009, T010 e T011; nenhuma falha inesperada pode permanecer.

**Checkpoint:** schema, infraestrutura HTTP e tipos do contrato estão verificáveis; as histórias podem avançar sem criar novas camadas compartilhadas.

## Fase 3 — US-01: Criar e visualizar uma tarefa (P1 / MVP)

**Objetivo:** criar uma tarefa mínima ou detalhada, reencontrá-la após reload e distinguir tarefas ativas, concluídas e atrasadas.

**Teste independente:** criar uma tarefa somente com título, atualizar a página e confirmar identidade, valores padrão e permanência na lista ativa.

### Testes

- [ ] T013 [P] [US-01] [AC-01] [AC-02] [AC-03] [FR-001] [FR-002] [FR-003] [FR-004] [FR-005] [FR-006] Escrever testes unitários inicialmente falhos para trim do título, limites, descrição multilinha, opcionais e valores padrão em `backend/tests/TodoList.Api.UnitTests/Features/Tasks/TaskInputValidatorTests.cs`; depende de T012.
- [ ] T014 [P] [US-01] [AC-01] [AC-02] [AC-03] [AC-04] [AC-05] [FR-001] [FR-007] [FR-008] [FR-009] [FR-018] [NFR-004] [NFR-005] Escrever testes de integração inicialmente falhos para criar/listar/obter, persistir após novo cliente, aceitar prazo passado e repetir `Idempotency-Key` com conteúdo igual ou conflitante em `backend/tests/TodoList.Api.IntegrationTests/Features/Tasks/CreateAndReadTaskTests.cs`; depende de T012.
- [ ] T015 [P] [US-01] [AC-01] [AC-02] [AC-03] [AC-05] [FR-007] [FR-009] [FR-012] [FR-019] [FR-020] [NFR-002] [NFR-003] [NFR-005] Escrever testes de componentes inicialmente falhos para inclusão rápida/detalhada, preservação de campos, feedback acessível, resumo e indicador textual de atraso em `frontend/src/features/tasks/components/CreateAndListTasks.test.tsx`; depende de T012.
- [ ] T016 [P] [US-01] [AC-01] [AC-04] [SC-002] [SC-003] [SC-006] Escrever E2E inicialmente falho de criação mínima por teclado, criação detalhada, rejeição sem perda de dados e reload em `frontend/e2e/us01-create-and-view.spec.ts`; depende de T012.

### Implementação

- [ ] T017 [US-01] [FR-001] [FR-002] [FR-003] [FR-004] [FR-005] [FR-006] Implementar `TaskInputValidator` e os modelos HTTP de criação/resposta em `backend/src/TodoList.Api/Features/Tasks/Create/` e `backend/src/TodoList.Api/Features/Tasks/TaskResponse.cs`; depende de T013; todos os testes unitários da tarefa devem passar.
- [ ] T018 [US-01] [FR-001] [FR-008] [FR-012] [FR-018] [FR-019] [NFR-004] Implementar `POST /api/tasks` com transação e semântica completa de `Idempotency-Key` em `backend/src/TodoList.Api/Features/Tasks/Create/CreateTaskEndpoint.cs`; depende de T014 e T017; casos 201, replay 200, validação 400 e conflito 409 devem passar.
- [ ] T019 [US-01] [FR-007] [FR-009] [FR-018] [FR-020] Implementar `GET /api/tasks` e `GET /api/tasks/{taskId}` em `backend/src/TodoList.Api/Features/Tasks/Read/`, com ordenação estável e limite de 1.000 itens; depende de T014 e T018; testes de leitura e persistência devem passar.
- [ ] T020 [P] [US-01] [FR-001] [FR-009] [FR-019] Implementar cliente `fetch` tipado, mapeamento de Problem Details e estado de carregamento em `frontend/src/features/tasks/api/taskApi.ts` e `frontend/src/features/tasks/model/taskTypes.ts`; depende de T011; testes do cliente com MSW devem passar.
- [ ] T021 [P] [US-01] [FR-001] [FR-003] [FR-005] [FR-006] [FR-012] [FR-019] Implementar formulário de inclusão rápida com expansão opcional de descrição, prioridade e prazo em `frontend/src/features/tasks/components/QuickAddTaskForm.tsx` e seu CSS Module; depende de T015 e T020; entradas devem permanecer após erro.
- [ ] T022 [P] [US-01] [FR-007] [FR-009] [FR-020] [NFR-003] [NFR-005] Implementar `TaskList`, `TaskSummary` e formatadores de data/atraso em `frontend/src/features/tasks/components/`, `frontend/src/features/tasks/model/taskDates.ts` e CSS Modules; depende de T015 e T020; atraso e estados devem possuir texto além de cor.
- [ ] T023 [US-01] [FR-009] [FR-019] [NFR-002] Integrar carregamento, formulário, listas ativa/concluída e região de feedback em `frontend/src/app/App.tsx` e `frontend/src/shared/FeedbackRegion.tsx`; depende de T019, T021 e T022; testes de componente devem passar.
- [ ] T024 [US-01] [AC-01] [AC-02] [AC-03] [AC-04] [AC-05] Executar testes unitários, integração, componentes e E2E da US-01 e registrar resultados em `docs/specs/001-essential-task/tasks.md`; depende de T016, T018, T019 e T023; o checkpoint só passa com AC-01 a AC-05 demonstrados.

**Checkpoint:** US-01 entrega um MVP persistente e demonstrável isoladamente, inclusive por teclado e após reload.

## Fase 4 — US-02: Editar os detalhes essenciais (P2)

**Objetivo:** editar ou remover campos opcionais atomicamente, inclusive em tarefa concluída, preservando entrada inválida e datas automáticas corretas.

**Teste independente:** alterar cada campo editável, atualizar a página e comprovar os novos valores sem mudança indevida de status ou conclusão.

### Testes

- [ ] T025 [P] [US-02] [AC-06] [AC-07] [AC-08] [AC-09] [FR-010] [FR-011] [FR-018] [NFR-004] Escrever testes de integração inicialmente falhos para `PUT /api/tasks/{taskId}`, remoção de opcionais, rollback integral e edição de concluída em `backend/tests/TodoList.Api.IntegrationTests/Features/Tasks/UpdateTaskTests.cs`; depende de T024.
- [ ] T026 [P] [US-02] [AC-06] [AC-07] [AC-08] [AC-09] [FR-012] [FR-019] [FR-020] [NFR-002] [NFR-003] [NFR-005] Escrever testes de componente inicialmente falhos do painel de detalhes, validação sem perda, datas localizadas e campos automáticos somente leitura em `frontend/src/features/tasks/components/TaskDetailsPanel.test.tsx`; depende de T024.
- [ ] T027 [P] [US-02] [AC-06] [AC-07] [AC-08] [AC-09] [SC-002] [SC-003] Escrever E2E inicialmente falho de editar, limpar opcionais, corrigir erro e editar concluída em `frontend/e2e/us02-edit-task.spec.ts`; depende de T024.

### Implementação

- [ ] T028 [US-02] [FR-002] [FR-003] [FR-005] [FR-006] [FR-010] [FR-011] Implementar atualização validada e atômica em `backend/src/TodoList.Api/Features/Tasks/Update/UpdateTaskEndpoint.cs`; depende de T025 e reutiliza T017; testes devem provar que uma falha não altera nenhum campo persistido.
- [ ] T029 [P] [US-02] [FR-010] [FR-019] Estender `frontend/src/features/tasks/api/taskApi.ts` com atualização tipada e tratamento de erros por campo; depende de T025 e T020; testes MSW devem passar.
- [ ] T030 [US-02] [FR-010] [FR-012] [FR-020] [NFR-002] [NFR-005] Implementar `TaskDetailsPanel.tsx`, formulário editável, datas somente leitura e restauração previsível de foco em `frontend/src/features/tasks/components/`; depende de T026 e T029.
- [ ] T031 [US-02] [FR-009] [FR-019] [NFR-003] Integrar abertura/fechamento do painel, atualização da lista somente após sucesso e feedback acessível em `frontend/src/app/App.tsx`; depende de T028 e T030; componentes e E2E não podem exibir falha como persistida.
- [ ] T032 [US-02] [AC-06] [AC-07] [AC-08] [AC-09] Executar a suíte da US-02 e registrar evidências em `docs/specs/001-essential-task/tasks.md`; depende de T027, T028 e T031; AC-06 a AC-09 devem passar isoladamente.

**Checkpoint:** US-02 é demonstrável sem a US-03 e mantém atomicidade, dados digitados e datas automáticas.

## Fase 5 — US-03: Acompanhar e alterar o estado (P2)

**Objetivo:** alternar estados ativos, concluir idempotentemente e reabrir para o estado anterior sem apagar o histórico.

**Teste independente:** mover uma tarefa entre estados, concluir duas vezes, reabrir e concluir novamente, verificando tarefa atual e eventos persistidos.

### Testes

- [ ] T033 [P] [US-03] [AC-10] [AC-11] [AC-12] [AC-13] [AC-14] [FR-013] [FR-014] [FR-015] [FR-016] [FR-017] Escrever testes unitários inicialmente falhos da máquina de estados em `backend/tests/TodoList.Api.UnitTests/Features/Tasks/TaskStateTransitionsTests.cs`; depende de T032.
- [ ] T034 [P] [US-03] [AC-10] [AC-11] [AC-12] [AC-13] [AC-14] [FR-013] [FR-014] [FR-015] [FR-016] [FR-017] [NFR-004] Escrever testes de integração inicialmente falhos para status ativo, conclusão repetida, reabertura, nova conclusão, histórico e rollback transacional em `backend/tests/TodoList.Api.IntegrationTests/Features/Tasks/TaskStatusTests.cs`; depende de T032.
- [ ] T035 [P] [US-03] [AC-10] [AC-11] [AC-13] [FR-009] [FR-019] [NFR-002] [NFR-003] Escrever testes de componente inicialmente falhos dos controles de status, conclusão/reabertura, foco e movimentação entre listas em `frontend/src/features/tasks/components/TaskStatusActions.test.tsx`; depende de T032.
- [ ] T036 [P] [US-03] [AC-10] [AC-11] [AC-12] [AC-13] [AC-14] [SC-004] [SC-006] Escrever E2E inicialmente falho de todo o ciclo de estado usando teclado em `frontend/e2e/us03-task-status.spec.ts`; depende de T032.

### Implementação

- [ ] T037 [US-03] [FR-013] [FR-014] [FR-015] [FR-016] [FR-017] Implementar transições e resolução do estado anterior em `backend/src/TodoList.Api/Features/Tasks/Status/TaskStateTransitions.cs`; depende de T033; testes unitários devem passar.
- [ ] T038 [US-03] [FR-013] [FR-014] [FR-015] [FR-016] [FR-017] [NFR-004] Implementar `PUT /status`, `POST /complete` e `POST /reopen` com tarefa e evento na mesma transação em `backend/src/TodoList.Api/Features/Tasks/Status/TaskStatusEndpoints.cs`; depende de T034 e T037; todos os testes de integração da fase devem passar.
- [ ] T039 [P] [US-03] [FR-013] [FR-014] [FR-016] [FR-019] Estender `frontend/src/features/tasks/api/taskApi.ts` com comandos tipados de status, conclusão e reabertura; depende de T035 e T020; testes MSW devem passar.
- [ ] T040 [US-03] [FR-009] [FR-013] [FR-014] [FR-016] [FR-019] [NFR-002] [NFR-003] Implementar `TaskStatusActions.tsx` e integrar as ações ao painel/listas em `frontend/src/features/tasks/components/` e `frontend/src/app/App.tsx`; depende de T038 e T039; foco, texto de estado e feedback devem permanecer corretos.
- [ ] T041 [US-03] [AC-10] [AC-11] [AC-12] [AC-13] [AC-14] Executar a suíte da US-03 e registrar evidências em `docs/specs/001-essential-task/tasks.md`; depende de T036, T038 e T040; AC-10 a AC-14 devem passar isoladamente.

**Checkpoint:** as três histórias estão funcionais; estado atual, conclusão atual e histórico correspondem a cada sequência executada.

## Fase 6 — Qualidade transversal e preparação do aceite

### Testes e verificações primeiro

- [ ] T042 [P] [NFR-004] [FR-019] Escrever testes inicialmente falhos de limite de corpo, origem CORS, `traceId`, health readiness sem banco e ausência de título/descrição nos logs em `backend/tests/TodoList.Api.IntegrationTests/Common/SecurityAndOperationsTests.cs`; depende de T041.
- [ ] T043 [P] [NFR-002] [NFR-003] [SC-006] Acrescentar varredura axe e verificações de ordem de foco, foco visível, nomes acessíveis e indicadores não cromáticos em `frontend/e2e/accessibility.spec.ts`; depende de T041.
- [ ] T044 [P] [NFR-001] [SC-005] Criar cenário reprodutível com 1.000 tarefas e medição de ao menos 100 operações de criar/editar/status em `frontend/e2e/performance.spec.ts` e `backend/tests/TodoList.Api.IntegrationTests/Performance/TaskPerformanceTests.cs`, registrando o percentil 95; depende de T041.
- [ ] T045 [P] [ADR-0002] Escrever teste inicialmente falho que gera o OpenAPI da API e o compara semanticamente ao contrato aprovado em `backend/tests/TodoList.Api.IntegrationTests/Contract/OpenApiContractTests.cs`, além de verificar que `frontend/src/features/tasks/api/schema.ts` está regenerado; depende de T041.

### Ajustes e automação

- [ ] T046 [NFR-004] [FR-019] Implementar limites de requisição, política CORS estrita, correlação de erros, readiness e redação de logs em `backend/src/TodoList.Api/Common/` e `backend/src/TodoList.Api/Program.cs`; depende de T042; `SecurityAndOperationsTests` deve passar.
- [ ] T047 [NFR-002] [NFR-003] [SC-006] Corrigir semântica, foco, mensagens e estilos acessíveis nos componentes e CSS de `frontend/src/features/tasks/` e `frontend/src/app/`; depende de T043; a varredura axe e as três jornadas por teclado devem passar.
- [ ] T048 [NFR-001] [SC-005] Ajustar consultas/índices em `backend/src/TodoList.Api/Data/` e renderização/atualizações em `frontend/src/features/tasks/` somente conforme gargalos medidos; depende de T044; o percentil 95 registrado deve ser de até 1 segundo com 1.000 tarefas.
- [ ] T049 [ADR-0002] Implementar normalização/comparação OpenAPI e verificação de tipos gerados em `backend/tests/TodoList.Api.IntegrationTests/Contract/`, `frontend/scripts/` e scripts dos manifests; depende de T045; qualquer drift deve falhar o comando de verificação.
- [ ] T050 [SETUP] [ADR-0001] Criar `.github/workflows/ci.yml` para restore determinístico, análise/formatação, build, migração, testes backend/frontend, contrato e E2E aplicáveis, sem segredos reais; depende de T046, T047, T048 e T049; o workflow deve ser sintaticamente válido e reproduzir os comandos locais.
- [ ] T051 [P] [FR-018] Atualizar `README.md`, `frontend/README.md` e `backend/README.md` com estrutura real, pré-requisitos, comandos, migrações, variáveis, testes e a restrição de não publicar a API sem proteção; depende de T041.
- [ ] T052 [P] [SC-001] Criar protocolo de teste com pelo menos 10 participantes em `docs/specs/001-essential-task/usability-test-protocol.md`, definindo início, tarefa, cronômetro e registro sem orientação; depende de T041; não inventar resultados, deixando a coleta para a validação quando necessário.
- [ ] T053 [FR-001] [FR-020] [NFR-001] [NFR-005] Executar restore, geração de contrato/tipos, análise estática, builds, migração do zero, suítes unitária/integração/componentes/E2E, acessibilidade e desempenho; depende de T050, T051 e T052; registrar comandos, versões, resultados e pendências reais em `docs/specs/001-essential-task/tasks.md`.
- [ ] T054 [SPEC-001] Revisar cobertura de FR-001 a FR-020, NFR-001 a NFR-005, AC-01 a AC-14 e SC-001 a SC-006, atualizar checkboxes/evidências e mover apenas `Implementação` para `Ready for review` em `docs/specs/001-essential-task/tasks.md`; depende de T053; não criar `validation.md` nem presumir aceite humano.

**Checkpoint:** implementação completa, documentada e pronta para revisão humana; a validação formal continua bloqueada até o aceite da implementação.

## Dependências e ordem

```text
Fase 1 (setup)
  └── Fase 2 (fundação)
        └── US-01 / MVP
              └── US-02
                    └── US-03
                          └── qualidade transversal
                                └── revisão da implementação
```

- O caminho crítico é `T001/T003 → T004 → T006 → T009 → T010 → T012 → T013/T014 → T017 → T018 → T019 → T023 → T024 → T025 → T028 → T031 → T032 → T033/T034 → T037 → T038 → T040 → T041 → T042/T044/T045 → T046/T048/T049 → T050 → T053 → T054`.
- T001, T002 e T003 podem ocorrer em paralelo por atuarem em caminhos distintos. Depois deles, T004 e T005 também podem avançar em paralelo.
- Em cada história, os arquivos de teste marcados `[P]` podem ser preparados simultaneamente; a implementação só começa quando os testes correspondentes existem e falham pelo motivo esperado.
- Backend e frontend podem avançar em paralelo apenas quando o contrato e os tipos já estiverem disponíveis e os arquivos modificados forem distintos; T020, T021/T022, T029, T039 e as tarefas explicitamente marcadas refletem esses casos.
- Checkpoints T024, T032 e T041 são barreiras: uma história posterior não deve mascarar uma falha da anterior.
- T047, T048 e T049 começam após seus verificadores respectivos, mas não são marcadas em paralelo porque ajustes de desempenho e contrato podem tocar arquivos compartilhados com frontend ou backend.

## Auditoria de cobertura

| Escopo | Testes que demonstram | Implementação principal |
|---|---|---|
| AC-01 a AC-05 / US-01 | T013, T014, T015, T016 | T017 a T023 |
| AC-06 a AC-09 / US-02 | T025, T026, T027 | T028 a T031 |
| AC-10 a AC-14 / US-03 | T033, T034, T035, T036 | T037 a T040 |
| FR-001 a FR-009 | T013 a T016 | T017 a T023 |
| FR-010 a FR-012 | T025 a T027 | T028 a T031 |
| FR-013 a FR-017 | T033 a T036 | T037 a T040 |
| FR-018 a FR-020 | T006, T007, T014, T015, T025, T026, T034, T042 | T009, T010, T018, T019, T023, T028, T031, T038, T040, T046 |
| NFR-001 / SC-005 | T044 | T048 e T053 |
| NFR-002, NFR-003 / SC-006 | T015, T016, T026, T027, T035, T036, T043 | T021, T022, T023, T030, T031, T040, T047 |
| NFR-004 / SC-002, SC-003, SC-004 | T006, T014, T025, T034, T042 | T009, T018, T028, T037, T038, T046 |
| NFR-005 | T014, T015, T026, T043 | T022 e T030 |
| SC-001 | T052 | coleta humana na etapa de validação; exceção explícita se indisponível |

Nenhum requisito ou cenário está órfão. SC-001 depende legitimamente de participantes humanos e não bloqueia a implementação técnica, mas impede um veredito `Passed` sem evidência ou exceção declarada na validação.

## Registro de execução

Ao concluir uma tarefa, acrescente uma entrada no formato:

```text
- TNNN — commit ou arquivos; comando executado; resultado objetivo.
```

Não marque tarefa incompleta, não substitua falha por descrição otimista e não crie `validation.md` durante a implementação.

## Aprovação do planejamento

**Decisão:** Approved
**Aprovado por:** Arthur Rubi
**Data:** 2026-10-04

## Aceite da implementação

**Decisão:** Pending
**Aceito por:** pendente
**Data:** pendente

## Registro de decisões

- 2026-10-04: tarefas derivadas da SPEC-001 e do plano aprovados.
- 2026-10-04: tarefas organizadas em setup mínimo, fundação bloqueante, três histórias verticais e qualidade transversal.
- 2026-10-04: testes posicionados antes da implementação correspondente e cobertura revisada sem itens órfãos.
- 2026-10-04: planejamento movido para `Ready for review`; implementação mantida em `Not started`.
- 2026-10-04: planejamento aprovado pelo mantenedor; implementação T001–T024 iniciada.
