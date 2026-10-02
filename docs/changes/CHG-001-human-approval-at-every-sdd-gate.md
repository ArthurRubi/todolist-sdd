# CHG-001: Exigir aprovação humana após todas as etapas SDD

**Status:** Accepted
**Data:** 2026-10-02
**Solicitante:** Arthur Rubi

## Motivo

Os gates de aprovação estavam explícitos para ideia, especificação e plano, mas não para tarefas, implementação e validação. O mantenedor solicitou uma pausa obrigatória ao final de cada etapa para revisar o artefato antes de autorizar a próxima.

## Artefatos afetados

- `docs/constitution.md`
- `docs/sdd-workflow.md`
- `AGENTS.md`
- templates de ideia, tarefas e validação
- skills SDD de `.codex/skills/`

## Mudança proposta

Toda etapa deve terminar em `Ready for review` e pedir uma decisão humana explícita. A próxima skill não pode ser iniciada automaticamente, mesmo quando o pedido original mencionar etapas posteriores.

Tarefas passam a exigir aprovação antes da implementação; a implementação exige aceite antes da validação; e um relatório de validação somente leva a funcionalidade para `Done` após aceite humano.

## Impacto

- Requisitos e critérios: nenhuma mudança no comportamento do produto.
- Arquitetura/dados/contratos: nenhum impacto técnico direto.
- Tarefas, testes e entrega: adiciona checkpoints de revisão e evita progressão automática.
- Compatibilidade e rollback: mudança apenas processual; pode ser revertida por nova emenda constitucional.

## Decisão

Accepted. O pedido explícito do mantenedor aprova a adoção imediata do gate em todas as etapas. A constituição passa de `1.0.0` para `1.1.0`, pois foi adicionada uma obrigação material.

**Aprovado por:** Arthur Rubi, por instrução explícita no pedido de 2026-10-02
**Data:** 2026-10-02
