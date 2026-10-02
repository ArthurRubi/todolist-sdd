# Ideia: Modelo de tarefa rico e altamente personalizável

**ID:** IDEA-002
**Status:** Accepted
**Criado em:** 2026-10-02
**Autor:** Arthur Rubi e Codex

## Problema ou oportunidade

Aplicativos de tarefas comuns costumam oferecer um conjunto pequeno e rígido de propriedades. Isso força pessoas mais detalhistas a adaptar seu modo de organização ao aplicativo, em vez de adaptar o aplicativo ao seu modo de pensar.

O TodoList SDD pode se diferenciar permitindo que uma tarefa comece simples, mas aceite um nível maior de detalhamento, organização visual e personalização quando o usuário desejar.

## Usuários afetados

O público principal são pessoas que organizam atividades pessoais com bastante cuidado, gostam de classificar informações e sentem falta de flexibilidade em aplicativos de TO-DO tradicionais.

Esse usuário não deve ser obrigado a preencher muitos campos para criar uma tarefa simples. A complexidade precisa aparecer progressivamente, conforme ele decidir personalizar a tarefa.

## Resultado desejado

Permitir que o usuário registre rapidamente uma tarefa apenas com um título e, opcionalmente, transforme essa tarefa em um item completo de planejamento, com datas, organização, contexto, acompanhamento e campos personalizados.

A experiência deve combinar dois objetivos:

1. criação rápida para tarefas simples;
2. detalhamento profundo para quem deseja controle e personalização.

## Princípios da experiência

- Somente o título deve ser obrigatório na criação inicial.
- Campos avançados devem ser opcionais e apresentados por divulgação progressiva, sem sobrecarregar a tela principal.
- O usuário deve poder escolher quais informações aparecem, em que ordem e em quais visualizações.
- Valores automáticos, como criação e última atualização, devem ser claramente diferentes de valores editáveis.
- Personalização não deve comprometer busca, filtros, ordenação ou acessibilidade.
- Concluir, arquivar ou restaurar uma tarefa não deve apagar seus detalhes.

## Propriedades candidatas

Estas propriedades orientam futuras specs. A ideia não exige que todas sejam entregues no primeiro incremento.

### Conteúdo essencial

- **Título:** nome curto e obrigatório da tarefa.
- **Descrição ou notas:** conteúdo mais longo, com formatação a ser definida posteriormente.
- **Status:** estado atual, começando por padrões como não iniciada, em andamento, bloqueada e concluída.
- **Progresso:** acompanhamento opcional por percentual ou por itens concluídos.

### Ciclo de vida e histórico

- **Data de criação:** registrada automaticamente e não editável.
- **Data da última atualização:** registrada automaticamente.
- **Data de conclusão:** preenchida automaticamente ao concluir, com possibilidade de reabertura preservando histórico.
- **Data de arquivamento:** registrada quando a tarefa sai das visualizações ativas.
- **Lixeira/restauração:** exclusão recuperável antes da remoção definitiva.

### Planejamento de tempo

- **Data de início ou agendamento:** quando o usuário pretende começar.
- **Prazo:** data e, opcionalmente, horário limite.
- **Dia inteiro e fuso horário:** evitam ambiguidades para tarefas com ou sem horário.
- **Lembretes múltiplos:** avisos em momentos diferentes antes ou depois de uma data relevante.
- **Recorrência:** repetição diária, semanal, mensal ou por regra personalizada.
- **Estimativa de duração:** esforço ou tempo esperado.
- **Tempo registrado:** acompanhamento opcional do tempo efetivamente usado.

### Prioridade e contexto

- **Prioridade:** nível de urgência ou importância, com valores padrão e possibilidade futura de personalização.
- **Destaque/favorita:** forma rápida de marcar itens importantes sem alterar a prioridade.
- **Energia ou esforço:** classificação opcional para ajudar a escolher o que fazer conforme a disponibilidade.
- **Contexto:** indicação como casa, trabalho, computador, rua ou outro contexto criado pelo usuário.

### Organização

- **Lista ou projeto:** agrupamento principal da tarefa.
- **Seção:** subdivisão opcional dentro de uma lista.
- **Tags:** classificação transversal, permitindo várias por tarefa.
- **Posição manual:** ordem escolhida pelo usuário em uma lista ou visão.
- **Cor e ícone:** identificação visual opcional, com acessibilidade independente de cor.
- **Modelo de tarefa:** conjunto reutilizável de propriedades e valores iniciais.

### Decomposição e relações

- **Checklist:** passos simples que podem ser marcados individualmente.
- **Subtarefas:** tarefas relacionadas que possuem seu próprio ciclo de vida.
- **Tarefa pai:** vínculo com a tarefa que originou a subtarefa.
- **Dependências:** indicação de que uma tarefa bloqueia ou depende de outra.
- **Links relacionados:** URLs úteis associadas à tarefa.
- **Anexos:** arquivos relacionados, sujeitos a decisões futuras de armazenamento e custo.

### Personalização avançada

- **Campos personalizados:** propriedades criadas pelo usuário, como texto, número, data, opção única, múltiplas opções, verdadeiro/falso e URL.
- **Status personalizados:** fluxos além dos estados padrão, preservando estados reconhecíveis pelo sistema.
- **Campos visíveis e ordem:** seleção de quais propriedades aparecem na edição e nos cartões.
- **Visualizações salvas:** combinações reutilizáveis de filtros, agrupamento e ordenação.
- **Valores padrão por lista ou modelo:** preenchimento automático configurável para novas tarefas.

## Recomendação de escopo incremental

Construir todas as propriedades em uma única funcionalidade criaria risco alto e dificultaria a validação. A recomendação é dividir esta ideia em specs progressivas:

1. **Tarefa essencial:** título, descrição, status, prioridade, prazo e datas automáticas.
2. **Organização:** listas, seções, tags e ordenação manual.
3. **Decomposição:** checklist e subtarefas.
4. **Planejamento:** início, lembretes, recorrência e duração.
5. **Personalização:** campos personalizados, modelos e configuração de exibição.
6. **Relações avançadas:** dependências, anexos e visualizações salvas.

Cada incremento deve manter a criação básica rápida e entregar valor independente.

## Escopo inicial proposto

- Definir um modelo conceitual de tarefa que suporte evolução sem exigir todos os recursos no primeiro lançamento.
- Entregar inicialmente os campos essenciais e datas automáticas.
- Preparar a experiência para propriedades opcionais e expansão progressiva.
- Tratar personalização avançada como diferencial planejado, não como requisito do primeiro incremento.

## Fora do escopo

- colaboração em tempo real e atribuição de tarefas a outras pessoas;
- sincronização com calendários ou serviços externos;
- inteligência artificial para preencher ou priorizar tarefas;
- aplicativos móveis nativos;
- implementação simultânea de todas as propriedades candidatas;
- definição de banco, endpoints ou componentes de interface nesta etapa.

## Riscos, dependências e decisões propostas

- **Excesso de complexidade:** muitos campos podem tornar o aplicativo cansativo; a mitigação proposta é divulgação progressiva e configuração de campos visíveis.
- **Campos personalizados:** aumentam o valor de personalização, mas afetam filtros, ordenação, validação e evolução de dados; devem possuir uma spec própria.
- **Status personalizados:** a proposta é começar com estados padrão e permitir personalização em etapa posterior, mantendo equivalências internas para recursos como conclusão.
- **Datas e recorrência:** precisam de regras claras de fuso horário e de geração da próxima ocorrência.
- **Anexos:** exigem armazenamento e limites operacionais; não pertencem ao primeiro incremento.
- **Premissa inicial:** o produto continua pessoal e de usuário único; colaboração futura exigirá nova ideia ou mudança aprovada.

## Critério para aceitar a ideia

A ideia pode ser aceita se o mantenedor concordar com:

- a abordagem híbrida de campos padrão mais campos personalizados;
- título como único campo obrigatório na criação inicial;
- interface simples por padrão, com detalhes revelados sob demanda;
- divisão do trabalho em várias specs incrementais;
- colaboração e integrações externas fora do escopo inicial.

## Registro de decisão

- 2026-10-02: ideia criada a partir da necessidade de maior personalização que aplicativos de TO-DO tradicionais.
- 2026-10-02: proposta organizada como modelo híbrido e incremental; estado alterado para `Ready for review`.
- 2026-10-02: ideia aprovada pelo mantenedor sem alterações; estado alterado para `Accepted`.

## Aprovação

**Decisão:** Accepted
**Aprovado por:** Arthur Rubi
**Data:** 2026-10-02
