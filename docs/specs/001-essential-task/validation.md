# Validação parcial: SPEC-001 — incremento T025–T032

**Spec:** [SPEC-001](./spec.md)
**Plano:** [Plano da SPEC-001](./plan.md)
**Tarefas:** [Tarefas da SPEC-001](./tasks.md)
**Contrato:** [Tasks API](./contracts/tasks-api.openapi.yaml)
**Data:** 2026-10-05
**Status:** Accepted
**Veredito técnico:** Passed with exceptions

## Resumo do incremento

Esta validação cobre exclusivamente T025–T032, correspondentes à US-02: editar título, descrição, prioridade e prazo; remover valores opcionais; rejeitar atomicamente entradas inválidas sem apagar o formulário; e editar uma tarefa concluída sem reabri-la ou alterar sua conclusão atual.

Os quatro cenários de aceitação AC-06 a AC-09 foram demonstrados em testes de integração, componente e E2E. A implementação funcional do incremento foi aprovada pelo mantenedor antes desta validação. A SPEC-001 completa permanece `In progress`; US-03 e os gates transversais T042–T054 não integram este veredito parcial.

## Ambiente e estado validado

- Commit de implementação: `d58a33cf0657d7bb620ea1c555f573d01e4b71db` (`feat(SPEC-001): deliver US-02 task editing`).
- Commit de aceite documental: `9666941a1b348397299493456ade2dfc27b08426`.
- Plataforma: macOS 26.6.2 arm64.
- Runtimes: .NET SDK 10.0.300, Node.js 20.20.2 e npm 10.8.2.
- Infraestrutura: Docker 29.8.2, Docker Compose 5.6.0 e PostgreSQL 18.6 Alpine.
- Entre o commit funcional e o commit de aceite documental, o diff contém apenas o registro de aceite em `tasks.md`; nenhum código de produto mudou antes da execução das verificações.
- Nenhum segredo real foi usado; somente as credenciais locais documentadas no `compose.yaml`.

## Evidências automatizadas

| Verificação | Comando | Resultado | Evidência/observação |
|---|---|---|---|
| Build backend | `dotnet build backend/TodoList.slnx --no-restore --disable-build-servers -m:1` | PASS | 3 projetos; 0 avisos e 0 erros. |
| Testes backend | `dotnet test backend/TodoList.slnx --no-restore --disable-build-servers -m:1` | PASS | 19/19 integração e 7/7 unidade; PostgreSQL efêmero real. |
| Análise C# do incremento | `dotnet format backend/TodoList.slnx --verify-no-changes --no-restore --include ...` | PASS | Todos os arquivos C# alterados por T025–T032 estão conformes. |
| Análise C# global | `dotnet format backend/TodoList.slnx --verify-no-changes --no-restore` | EXCEPTION | Falha `CHARSET` na migração T009, anterior a este incremento; ver pendências. |
| Tipos pelo contrato | `npm run generate:api --prefix frontend` + `git diff --exit-code -- frontend/src/features/tasks/api/schema.ts` | PASS | Regeneração determinística, sem drift no schema TypeScript versionado. |
| Testes frontend | `npm run test:run --prefix frontend` | PASS | 5 arquivos e 12/12 testes, incluindo 3/3 do painel da US-02. |
| Build frontend | `npm run build --prefix frontend` | PASS | TypeScript e Vite concluídos sem erro. |
| Configuração PostgreSQL | `docker-compose -f compose.yaml config` | PASS | Configuração resolvida e válida. |
| Migração do zero | `dotnet tool run dotnet-ef database update ...` contra banco temporário vazio | PASS | Migração `20261005022033_InitialCreate`; tabelas `tasks`, `task_status_events` e `__EFMigrationsHistory` confirmadas. Infraestrutura temporária removida após a prova. |
| Jornadas E2E | `npm run test:e2e --prefix frontend` | PASS | 7/7 em Chromium; 3/3 cenários da US-02 com SPA, API e PostgreSQL reais. |
| Higiene do diff | `git diff --check` | PASS | Nenhum erro de whitespace. |

## Matriz de rastreabilidade final do incremento

| Requisito/cenário | Tarefas | Teste ou evidência | Resultado |
|---|---|---|---|
| AC-06; FR-002, FR-003, FR-005, FR-006, FR-008, FR-010, FR-018, FR-020 | T025–T032 | `Valid_update_replaces_editable_fields_and_renews_updated_at`; teste do painel; E2E `edita todos os campos e preserva a atualização após reload` | PASS |
| AC-07; FR-006, FR-010 | T025, T026, T027, T028, T030, T031 | `Optional_fields_can_be_removed_without_affecting_other_data`; componente envia `null`; E2E remove opcionais | PASS |
| AC-08; FR-011, FR-012, FR-019; NFR-003, NFR-004; SC-003 | T025–T032 | `Invalid_update_does_not_change_any_persisted_field`; componente preserva todos os campos; E2E corrige sem redigitação | PASS |
| AC-09; FR-009, FR-010, FR-020 | T025–T032 | `Updating_a_completed_task_preserves_status_and_current_completion`; componente expõe datas somente leitura; E2E preserva estado e conclusão | PASS |
| SC-002 | T027, T032 | Os E2E de edição e remoção recarregam a página e reencontram os valores confirmados | PASS |
| NFR-002 e SC-006 no recorte da US-02 | T026, T027, T030, T031 | Rótulos/roles, foco inicial e retorno ao acionador foram exercitados; auditoria axe e jornada integral somente por teclado estão em T043/T047 | PARTIAL |
| NFR-005 | T026, T030 | Prazo preservado como data de calendário e instantes formatados com `Intl.DateTimeFormat`; matriz de duas localidades/fusos está adiada para T043/T053 | PARTIAL |
| NFR-001 e SC-005 | T044, T048, T053 | Medição p95 com 1.000 tarefas ainda não pertence ao incremento executado | DEFERRED |

## Validação manual e exploratória

- [x] Revisado o handler de atualização: toda validação ocorre antes de buscar/mutar a entidade; somente os quatro campos editáveis são atribuídos; `status`, `createdAt` e `completedAt` não são alterados.
- [x] Revisado o fluxo de interface: a lista só substitui a tarefa após resposta bem-sucedida; erro mantém o painel e seus valores; sucesso fecha o diálogo e devolve foco ao botão de origem.
- [x] Conferido o contrato aprovado: `PUT /tasks/{taskId}` exige o conjunto de campos editáveis, retorna `Task` em 200, `ValidationProblem` em 400 e `ProblemDetails` em 404.
- [x] Inspecionado o banco criado do zero: as duas tabelas de domínio e o histórico da migração estavam presentes antes da remoção do ambiente temporário.

## Qualidade transversal

- Segurança e privacidade: nenhuma ampliação de superfície além do `PUT` contratado; 404 e erros de validação usam Problem Details. Limites de corpo, política CORS final e redação de logs pertencem a T042/T046 e não foram declarados aprovados aqui.
- Acessibilidade: nomes acessíveis, feedback textual, foco inicial e restauração de foco possuem cobertura. A varredura axe, foco visível/ordem completa e as três jornadas somente por teclado continuam programadas em T043/T047.
- Desempenho e confiabilidade: atomicidade, persistência e ausência de atualização otimista enganosa passaram. O p95 de 1 segundo com 1.000 tarefas não foi medido neste incremento.
- Localização temporal: o contrato mantém prazo como `date`; a interface formata data e instantes na localidade do navegador. A matriz multi-localidade/fuso ainda depende do gate transversal.
- Documentação e contrato: spec, plano, tarefas, modelo de dados e OpenAPI estão coerentes com o comportamento observado; os tipos TypeScript regeneram sem drift. A comparação semântica do OpenAPI gerado em runtime está planejada em T045/T049.

## Desvios, riscos e pendências

1. O gate global `dotnet format` falha porque `backend/src/TodoList.Api/Migrations/20261005022033_InitialCreate.cs`, criado em T009, contém BOM UTF-8. Os arquivos C# de T025–T032 passam isoladamente. A correção deve ocorrer em uma etapa de implementação autorizada, e o gate global deve ser repetido em T053.
2. NFR-001/SC-005, a auditoria completa de NFR-002/SC-006 e a matriz ampliada de NFR-005 ainda não possuem evidência final; as tarefas T043, T044, T047, T048 e T053 já rastreiam esse trabalho.
3. `README.md`, `frontend/README.md` e `backend/README.md` ainda descrevem os módulos como não inicializados. A atualização está explicitamente prevista em T051 e deve ser concluída antes da validação integral da SPEC-001.
4. A equivalência semântica entre o OpenAPI exposto pela API e o contrato aprovado ainda depende de T045/T049. Neste recorte foi comprovada apenas a regeneração estável dos tipos a partir do contrato versionado.

## Conclusão técnica

**Passed with exceptions.** T025–T032 entregam e demonstram AC-06 a AC-09 sem falha funcional observada: edição e remoção persistem após reload, atualização inválida é atômica e preserva a entrada, e tarefa concluída mantém estado e instante de conclusão. O veredito não é `Passed` porque gates transversais deliberadamente posteriores ainda estão pendentes e porque a análise estática global encontrou o desvio de codificação anterior ao incremento.

Este aceite, se concedido, encerra somente a validação parcial de T025–T032. A SPEC-001 permanece em andamento.

**Validado por:** Codex
**Data:** 2026-10-05

## Aceite humano da validação T025–T032

**Decisão:** Accepted
**Aceito por:** Arthur Rubi
**Data:** 2026-10-05

---

# Validação parcial: SPEC-001 — incremento T033–T041

**Spec:** [SPEC-001](./spec.md)
**Plano:** [Plano da SPEC-001](./plan.md)
**Tarefas:** [Tarefas da SPEC-001](./tasks.md)
**Contrato:** [Tasks API](./contracts/tasks-api.openapi.yaml)
**Data:** 2026-10-05
**Status:** Ready for review
**Veredito técnico:** Passed with exceptions

## Resumo do incremento

Esta validação cobre exclusivamente T033–T041, correspondentes à US-03: alterar uma tarefa entre estados ativos, concluir de forma idempotente, reabrir para o estado imediatamente anterior e concluir novamente sem apagar o histórico.

Os cinco cenários AC-10 a AC-14 foram demonstrados em testes unitários, integração com PostgreSQL, componentes e E2E. A implementação foi aprovada pelo mantenedor antes desta validação. A SPEC-001 completa permanece `In progress`, pois os gates transversais T042–T054 ainda não foram implementados.

## Ambiente e estado validado

- Commit de implementação: `00e63277df2fdcae1076180514554efbeec6f9f3` (`feat(SPEC-001): deliver US-03 task status lifecycle`).
- Commit de aceite documental: `2de910050ade8017c3325e537aba2896760f60c9`.
- Plataforma: macOS 26.6.2 arm64.
- Runtimes: .NET SDK 10.0.300, Node.js 20.20.2 e npm 10.8.2.
- Infraestrutura: Docker 29.8.2, Docker Compose 5.6.0 e PostgreSQL 18.6 Alpine.
- Entre o commit funcional e o commit de aceite documental, o diff contém apenas o aceite em `tasks.md`; nenhum código de produto mudou antes das verificações.
- Nenhum segredo real foi usado; somente as credenciais locais documentadas no `compose.yaml`.

## Evidências automatizadas

| Verificação | Comando | Resultado | Evidência/observação |
|---|---|---|---|
| Build backend | `dotnet build backend/TodoList.slnx --no-restore --disable-build-servers -m:1` | PASS | 3 projetos; 0 avisos e 0 erros. |
| Testes backend | `dotnet test backend/TodoList.slnx --no-restore --disable-build-servers -m:1` | PASS | 25/25 integração e 13/13 unidade; PostgreSQL efêmero real. |
| Análise C# do incremento | `dotnet format backend/TodoList.slnx --verify-no-changes --no-restore --include ...` | PASS | Todos os arquivos C# alterados por T033–T041 estão conformes. |
| Análise C# global | `dotnet format backend/TodoList.slnx --verify-no-changes --no-restore` | EXCEPTION | Falha `CHARSET` na migração T009, anterior a este incremento; ver pendências. |
| Tipos pelo contrato | `npm run generate:api --prefix frontend` + `git diff --exit-code -- frontend/src/features/tasks/api/schema.ts` | PASS | Regeneração determinística, sem drift no schema TypeScript versionado. |
| Testes frontend | `npm run test:run --prefix frontend` | PASS | 6 arquivos e 17/17 testes; 3/3 do controle de status e 6/6 do cliente HTTP. |
| Build frontend | `npm run build --prefix frontend` | PASS | TypeScript e Vite concluídos sem erro. |
| Configuração PostgreSQL | `docker-compose -f compose.yaml config` | PASS | Configuração resolvida e válida. |
| Migração do zero | `dotnet tool run dotnet-ef database update ...` contra banco temporário vazio | PASS | Migração `20261005022033_InitialCreate`; tabelas `tasks`, `task_status_events` e `__EFMigrationsHistory` confirmadas. Ambiente temporário removido. |
| Jornadas E2E | `npm run test:e2e --prefix frontend` | PASS | 8/8 em Chromium; ciclo completo da US-03 por teclado com SPA, API e PostgreSQL reais. |
| Higiene do diff | `git diff --check` | PASS | Nenhum erro de whitespace. |

## Matriz de rastreabilidade final do incremento

| Requisito/cenário | Tarefas | Teste ou evidência | Resultado |
|---|---|---|---|
| AC-10; FR-004, FR-008, FR-013, FR-017, FR-018, FR-020 | T033–T041 | `Active_status_change_updates_task_and_appends_one_event`; `Active_status_changes_are_persisted_with_one_event_and_no_completion`; E2E muda para `Em andamento` | PASS |
| AC-11; FR-009, FR-014, FR-017–FR-020 | T033–T041 | `Completion_is_idempotent_and_moves_the_task_to_the_completed_view`; E2E move a tarefa para `Concluídas`, registra e exibe a conclusão | PASS |
| AC-12; FR-015, FR-017; NFR-004 | T033, T034, T037, T038 | Testes unitário/integração repetem conclusão; E2E repete o comando e compara `completedAt`; exatamente um evento permanece | PASS |
| AC-13; FR-016, FR-017–FR-020 | T033–T041 | `Reopening_restores_the_status_before_the_current_completion_and_keeps_history`; integração restaura `blocked`; E2E restaura `in_progress` e move para ativas | PASS |
| AC-14; FR-014, FR-017; SC-004 | T033, T034, T036–T041 | Nova conclusão cria o terceiro evento, mantém duas conclusões históricas e substitui somente a conclusão atual | PASS |
| NFR-002, NFR-003 e SC-006 no recorte da US-03 | T035, T036, T040 | Testes de componente provam nomes e foco; E2E executa o ciclo completo por teclado e confirma feedback/listas textuais | PASS no recorte |
| NFR-004 e SC-004 | T034, T037, T038, T041 | Trigger de teste força falha do insert do evento; tarefa e histórico permanecem integralmente inalterados | PASS |
| NFR-001 e SC-005 | T044, T048, T053 | Medição p95 com 1.000 tarefas ainda não pertence ao incremento executado | DEFERRED |
| NFR-005 | T043, T053 | Instantes continuam formatados com `Intl.DateTimeFormat`; matriz ampliada de localidades/fusos ainda não foi executada | PARTIAL |

## Validação manual e exploratória

- [x] Revisada a máquina de estados: cada mudança efetiva gera exatamente um evento; repetir o mesmo estado ativo ou a conclusão atual é no-op; reabertura localiza a transição da conclusão atual antes de restaurar o estado.
- [x] Revisadas as rotas: `PUT /status`, `POST /complete` e `POST /reopen` correspondem ao contrato; tarefa inexistente retorna 404, estado inválido retorna 400 e transição conflitante retorna 409.
- [x] Revisada a atomicidade: tarefa e evento são salvos dentro da mesma transação explícita; uma falha forçada no evento retornou 500 e o estado da tarefa permaneceu inalterado.
- [x] Revisado o fluxo de interface: listas só mudam depois da resposta persistida; conclusão foca `Reabrir tarefa`; reabertura foca `Estado ativo`; estado, sucesso e erro possuem comunicação textual.
- [x] Inspecionado o banco criado do zero: tabelas de tarefa, eventos e histórico da migração estavam presentes antes da remoção do ambiente temporário.

## Qualidade transversal

- Segurança e privacidade: as três rotas de escrita usam EF parametrizado e Problem Details, sem expor o histórico interno no contrato. Limite de corpo, CORS final e redação de logs pertencem a T042/T046 e não foram declarados aprovados.
- Acessibilidade: a jornada US-03 passou integralmente por teclado, com nomes acessíveis, foco previsível e indicadores textuais. A varredura axe e a auditoria conjunta das três jornadas permanecem em T043/T047.
- Desempenho e confiabilidade: idempotência, persistência, histórico e rollback atômico passaram. O p95 de 1 segundo com 1.000 tarefas não foi medido neste incremento.
- Localização temporal: instantes de atualização/conclusão continuam formatados na localidade do navegador. A matriz multi-localidade/fuso depende do gate transversal.
- Documentação e contrato: spec, plano, tarefas, modelo e OpenAPI descrevem as rotas e transições observadas; tipos TypeScript regeneram sem drift. A comparação semântica do OpenAPI runtime está programada em T045/T049.

## Desvios, riscos e pendências

1. O gate global `dotnet format` continua falhando porque `backend/src/TodoList.Api/Migrations/20261005022033_InitialCreate.cs`, criado em T009, contém BOM UTF-8. Os arquivos C# de T033–T041 passam isoladamente. A correção precisa ocorrer numa passagem de implementação autorizada antes de T050/T053.
2. NFR-001/SC-005, auditoria axe e conjunta de NFR-002/SC-006 e matriz ampliada de NFR-005 ainda dependem de T043, T044, T047, T048 e T053.
3. `README.md`, `frontend/README.md` e `backend/README.md` ainda descrevem os módulos como não inicializados. A atualização está prevista em T051 e é obrigatória antes da validação integral.
4. A comparação semântica entre o OpenAPI exposto pela API e o contrato aprovado continua pendente em T045/T049. Neste recorte foi comprovada a geração estável dos tipos a partir do contrato versionado.
5. A API continua sem autenticação, conforme o escopo aprovado; portanto, não deve ser publicada abertamente até existir proteção especificada ou fornecida pela plataforma.

## Conclusão técnica

**Passed with exceptions.** T033–T041 demonstram AC-10 a AC-14 sem falha funcional observada: estados ativos persistem sem conclusão, concluir é idempotente, reabrir restaura o estado anterior, nova conclusão preserva todo o histórico e falhas transacionais não deixam estado parcial. O veredito não é `Passed` porque gates transversais posteriores continuam pendentes e a análise estática global mantém o desvio de codificação anterior.

Este aceite, se concedido, encerra somente a validação parcial de T033–T041. A SPEC-001 permanece em andamento até T042–T054 e sua validação integral.

**Validado por:** Codex
**Data:** 2026-10-05

## Aceite humano da validação T033–T041

**Decisão:** Accepted
**Aceito por:** Arthur Rubi
**Data:** 2026-10-05
