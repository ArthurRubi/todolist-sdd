# Especificação da funcionalidade: Tarefa essencial

**ID:** SPEC-001
**Ideia de origem:** [IDEA-002](../../ideas/002-customizable-task-model.md)
**Branch sugerida:** `codex/001-essential-task`
**Criada em:** 2026-10-02
**Status:** Approved
**Input:** Criar a primeira especificação da IDEA-002, começando pelo incremento de tarefa essencial.

## Problema e resultado

O usuário precisa registrar e acompanhar tarefas sem ser obrigado a preencher um formulário complexo. Ao mesmo tempo, a primeira versão deve estabelecer propriedades essenciais consistentes, capazes de sustentar a personalização planejada para incrementos futuros.

Depois desta funcionalidade, o usuário poderá criar uma tarefa apenas com um título, acrescentar descrição, prioridade e prazo quando desejar, editar essas informações, acompanhar o estado da tarefa, concluí-la e reabri-la sem perder seu histórico essencial.

## Jornadas e testes de aceitação

### US-01 — Criar e visualizar uma tarefa (P1)

**Como** pessoa organizando minhas atividades, **quero** registrar uma tarefa rapidamente, **para** não esquecer o que preciso fazer.

**Por que esta prioridade:** sem criar e reencontrar uma tarefa persistida, nenhuma outra capacidade do produto entrega valor.

**Teste independente:** criar uma tarefa somente com título, atualizar a visualização e confirmar que ela permanece acessível com os valores padrão corretos.

**Cenários de aceitação:**

1. **AC-01 — Criação mínima:** **Dado** que o usuário está na visualização de tarefas, **quando** informa um título válido e confirma, **então** a tarefa é criada como `Não iniciada`, sem prioridade e sem prazo, e aparece entre as tarefas ativas.
2. **AC-02 — Criação detalhada:** **Dado** que o usuário está criando uma tarefa, **quando** informa título, descrição, prioridade e prazo válidos, **então** todos os valores são preservados e exibidos na tarefa criada.
3. **AC-03 — Título inválido:** **Dado** que o título está vazio ou contém somente espaços, **quando** o usuário tenta criar a tarefa, **então** recebe uma mensagem clara, os dados digitados são preservados e nenhuma tarefa é criada.
4. **AC-04 — Persistência:** **Dado** que uma tarefa foi criada com sucesso, **quando** o usuário atualiza ou retorna à aplicação, **então** a mesma tarefa continua disponível sem perda de dados.
5. **AC-05 — Data passada:** **Dado** um prazo anterior à data atual, **quando** o usuário cria a tarefa, **então** a criação é permitida e a tarefa ativa é identificada como atrasada.

---

### US-02 — Editar os detalhes essenciais (P2)

**Como** pessoa que refina seu planejamento, **quero** alterar os detalhes de uma tarefa, **para** manter suas informações corretas conforme a situação muda.

**Por que esta prioridade:** tarefas mudam depois de criadas; sem edição, erros ou mudanças de planejamento exigiriam recriar o item.

**Teste independente:** editar cada propriedade disponibilizada por esta spec, atualizar a visualização e confirmar os novos valores sem alterar propriedades automáticas indevidamente.

**Cenários de aceitação:**

1. **AC-06 — Edição válida:** **Dado** uma tarefa existente, **quando** o usuário altera título, descrição, prioridade ou prazo com valores válidos, **então** todas as alterações confirmadas são preservadas e a última atualização é renovada.
2. **AC-07 — Remoção de opcionais:** **Dado** uma tarefa com descrição, prioridade ou prazo, **quando** o usuário remove um desses valores e confirma, **então** o campo fica sem valor sem afetar os demais dados.
3. **AC-08 — Edição inválida:** **Dado** uma tarefa existente, **quando** o usuário tenta salvar um título inválido ou um campo acima do limite, **então** recebe uma mensagem específica, mantém os dados editados na tela e a versão persistida permanece integralmente inalterada.
4. **AC-09 — Edição após conclusão:** **Dado** uma tarefa concluída, **quando** o usuário altera uma propriedade que não seja o status, **então** a alteração é preservada sem mudar sua data atual de conclusão.

---

### US-03 — Acompanhar e alterar o estado da tarefa (P2)

**Como** pessoa executando minhas atividades, **quero** indicar o estado atual de uma tarefa, **para** distinguir o que ainda não comecei, está em andamento, está bloqueado ou foi concluído.

**Por que esta prioridade:** o estado torna a lista útil para acompanhamento e estabelece a semântica necessária para conclusão e reabertura.

**Teste independente:** criar uma tarefa, movê-la pelos estados disponíveis, concluí-la e reabri-la, verificando estado, datas e preservação do histórico.

**Cenários de aceitação:**

1. **AC-10 — Alteração de estado ativo:** **Dado** uma tarefa ativa, **quando** o usuário escolhe `Em andamento` ou `Bloqueada`, **então** o novo estado é preservado sem preencher uma data de conclusão.
2. **AC-11 — Conclusão:** **Dado** uma tarefa em qualquer estado ativo, **quando** o usuário a conclui, **então** o estado passa para `Concluída`, a data de conclusão é registrada automaticamente e a tarefa fica disponível na visualização de concluídas.
3. **AC-12 — Conclusão repetida:** **Dado** uma tarefa já concluída, **quando** a mesma ação de conclusão é repetida, **então** não são criados eventos duplicados nem alterada a data de conclusão original.
4. **AC-13 — Reabertura:** **Dado** uma tarefa concluída, **quando** o usuário a reabre, **então** ela retorna ao estado ativo que possuía imediatamente antes da conclusão, volta à visualização de tarefas ativas e a conclusão anterior permanece registrada no histórico.
5. **AC-14 — Nova conclusão:** **Dado** uma tarefa reaberta, **quando** ela é concluída novamente, **então** uma nova conclusão é registrada e passa a ser a conclusão atual, sem apagar os eventos anteriores.

## Casos-limite

- Espaços no início e no fim do título são removidos; espaços internos são preservados.
- Um título com mais de 200 caracteres ou uma descrição com mais de 10.000 caracteres é rejeitado com indicação do limite aplicável.
- A descrição preserva quebras de linha e texto simples; formatação rica não faz parte desta spec.
- Um prazo é uma data de calendário sem horário. Horários, fusos e lembretes pertencem a uma spec posterior.
- Um prazo no passado é permitido. A tarefa só deixa de ser considerada atrasada quando está concluída ou quando o prazo deixa de estar no passado.
- Alterar título, descrição, prioridade ou prazo de uma tarefa concluída não a reabre.
- Uma falha ao criar ou salvar não pode exibir a operação como concluída nem deixar alterações parcialmente persistidas.
- Repetir uma ação após resposta incerta não pode criar tarefas duplicadas sem que o usuário seja informado.
- Datas automáticas são exibidas de acordo com a localidade do usuário e não podem ser editadas nesta funcionalidade.

## Requisitos

### Funcionais

- **FR-001:** O sistema DEVE permitir criar uma tarefa informando somente um título.
- **FR-002:** O sistema DEVE normalizar espaços externos do título e aceitar de 1 a 200 caracteres após essa normalização.
- **FR-003:** O sistema DEVE permitir uma descrição opcional em texto simples, preservando quebras de linha, com até 10.000 caracteres.
- **FR-004:** O sistema DEVE oferecer os estados fixos `Não iniciada`, `Em andamento`, `Bloqueada` e `Concluída`, usando `Não iniciada` como padrão.
- **FR-005:** O sistema DEVE oferecer as prioridades fixas `Sem prioridade`, `Baixa`, `Média`, `Alta` e `Urgente`, usando `Sem prioridade` como padrão.
- **FR-006:** O sistema DEVE permitir um prazo opcional representado por uma data de calendário, inclusive no passado, e permitir sua remoção.
- **FR-007:** O sistema DEVE identificar como atrasada toda tarefa ativa cujo prazo seja anterior à data atual do usuário.
- **FR-008:** O sistema DEVE gerar uma identidade estável para cada tarefa e registrar automaticamente criação e última atualização.
- **FR-009:** O sistema DEVE exibir tarefas ativas e concluídas de maneira distinguível e permitir acessar seus detalhes essenciais.
- **FR-010:** O sistema DEVE permitir editar título, descrição, prioridade e prazo, inclusive em tarefas concluídas.
- **FR-011:** O sistema DEVE validar uma alteração por inteiro; se qualquer campo for inválido, nenhum dos campos enviados naquela confirmação pode substituir a versão persistida.
- **FR-012:** O sistema DEVE preservar os valores digitados quando uma criação ou edição for rejeitada, permitindo correção sem redigitação desnecessária.
- **FR-013:** O sistema DEVE permitir alterar uma tarefa ativa entre `Não iniciada`, `Em andamento` e `Bloqueada` sem registrar conclusão.
- **FR-014:** O sistema DEVE registrar automaticamente o instante de conclusão quando uma tarefa ativa passa para `Concluída`.
- **FR-015:** O sistema DEVE tornar uma ação repetida de conclusão inofensiva, sem duplicar eventos ou substituir a conclusão atual.
- **FR-016:** O sistema DEVE permitir reabrir uma tarefa concluída, restaurando o estado ativo imediatamente anterior à conclusão.
- **FR-017:** O sistema DEVE manter um histórico mínimo das mudanças de estado e conclusões, mesmo quando a tarefa for reaberta.
- **FR-018:** O sistema DEVE persistir tarefas e alterações confirmadas entre sessões da aplicação.
- **FR-019:** O sistema DEVE informar sucesso ou falha de cada operação de forma compreensível e não apresentar como persistida uma alteração que falhou.
- **FR-020:** O sistema DEVE apresentar criação, última atualização e conclusão atual como informações somente de leitura.

### Qualidade

- **NFR-001 — Rapidez percebida:** criar, editar ou alterar o estado deve apresentar um resultado ao usuário em até 1 segundo em pelo menos 95% das tentativas, considerando uma coleção de até 1.000 tarefas e condições operacionais normais.
- **NFR-002 — Acessibilidade por teclado:** todas as jornadas desta spec devem poder ser concluídas sem dispositivo apontador, com ordem de foco previsível e foco visível.
- **NFR-003 — Comunicação acessível:** estado, prioridade, atraso, sucesso e erro não podem ser comunicados somente por cor; mensagens de validação devem identificar o campo e o problema.
- **NFR-004 — Integridade:** falhas de criação ou atualização não podem resultar em tarefas duplicadas, registros parcialmente alterados ou divergência silenciosa entre o que é exibido e o que foi persistido.
- **NFR-005 — Localização temporal:** datas e instantes exibidos ao usuário devem respeitar sua localidade; o prazo sem horário deve permanecer na mesma data de calendário independentemente do ambiente de execução.

## Entidades conceituais

- **Tarefa:** unidade de trabalho pessoal com identidade, título, descrição opcional, status atual, prioridade, prazo opcional e datas automáticas de ciclo de vida.
- **Evento de estado:** registro imutável de uma mudança relevante de status, contendo estado anterior, novo estado e instante da mudança; sustenta reabertura e histórico de conclusões.

## Fora do escopo

- contas, autenticação, compartilhamento ou colaboração;
- exclusão, lixeira, restauração e arquivamento;
- listas, projetos, seções, tags e ordenação manual;
- checklist, subtarefas e dependências;
- lembretes, recorrência, data de início, duração ou registro de tempo;
- anexos, links relacionados, cores, ícones e modelos;
- campos, prioridades ou status personalizados;
- pesquisa, filtros e visualizações salvas;
- notificações externas, sincronização ou funcionamento offline;
- formatação rica na descrição;
- definição de banco, API, componentes de interface ou ferramentas de implementação.

## Critérios mensuráveis de sucesso

- **SC-001:** Em teste de usabilidade, pelo menos 90% das pessoas conseguem criar uma tarefa somente com título e reencontrá-la em até 20 segundos, sem orientação.
- **SC-002:** Todos os cenários de aceitação preservam 100% dos valores confirmados após atualizar e reabrir a aplicação.
- **SC-003:** Em todos os testes de título vazio, somente com espaços ou acima do limite, nenhuma tarefa nova é persistida e nenhuma existente é sobrescrita.
- **SC-004:** Em todos os cenários de conclusão, repetição, reabertura e nova conclusão, o estado atual, a conclusão atual e o histórico correspondem à sequência executada.
- **SC-005:** Pelo menos 95% das criações, edições e mudanças de estado apresentam resultado em até 1 segundo com uma coleção de 1.000 tarefas em condições operacionais normais.
- **SC-006:** As três jornadas prioritárias podem ser concluídas integralmente usando apenas teclado, sem perda de informação comunicada visualmente.

## Premissas e dependências

- O primeiro incremento atende uma única pessoa em seu próprio contexto; identidade e autenticação serão tratadas separadamente antes de uma publicação multiusuário.
- A aplicação possui uma visualização simples de tarefas ativas e outra de concluídas; organização avançada virá em specs posteriores.
- Prioridades e estados são fixos neste incremento para que a personalização seja especificada separadamente.
- O prazo representa somente uma data de calendário. Agendamento com horário, fuso e lembretes não está implícito.
- Tarefas concluídas continuam editáveis; somente uma mudança de status as reabre.
- O sistema conhece a localidade e a data atual do usuário para exibição e cálculo de atraso.

## Matriz inicial de cobertura

| História | Requisitos principais | Cenários de aceitação |
|---|---|---|
| US-01 | FR-001–FR-009, FR-012, FR-018–FR-020, NFR-001–NFR-005 | AC-01–AC-05 |
| US-02 | FR-002–FR-003, FR-005–FR-006, FR-008–FR-012, FR-018–FR-020, NFR-001–NFR-005 | AC-06–AC-09 |
| US-03 | FR-004, FR-008–FR-009, FR-013–FR-020, NFR-001–NFR-005 | AC-10–AC-14 |

## Registro de decisões

- 2026-10-02: spec criada como `Draft` a partir da `IDEA-002` aceita.
- 2026-10-02: escopo limitado ao ciclo essencial da tarefa, adiando personalização e organização avançadas para specs independentes.
- 2026-10-02: prazo definido como data sem horário; horários e fusos serão tratados no incremento de planejamento.
- 2026-10-02: definidos quatro estados e cinco prioridades fixos como base inicial.
- 2026-10-02: spec revisada quanto a cobertura e movida para `Ready for review`.
- 2026-10-04: spec aprovada pelo mantenedor sem alterações; estado alterado para `Approved`.

## Aprovação

**Decisão:** Approved
**Aprovada por:** Arthur Rubi
**Data:** 2026-10-04
