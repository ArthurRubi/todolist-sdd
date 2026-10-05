# CHG-002: Informar sempre a próxima etapa SDD

**Status:** Accepted
**Data:** 2026-10-04
**Solicitante:** Arthur Rubi

## Motivo

O fluxo já exigia aprovação humana ao final de cada etapa, mas a resposta nem sempre tornava explícita a skill que deveria ser invocada depois da aprovação. Para facilitar o aprendizado e evitar dúvida operacional, o mantenedor solicitou que todo encerramento de etapa apresente o próximo passo e um exemplo de invocação.

## Artefatos afetados

- `docs/constitution.md`
- `docs/sdd-workflow.md`
- `AGENTS.md`
- skills SDD de `.codex/skills/`

## Mudança proposta

Ao finalizar uma etapa, o Codex deve:

1. pedir aprovação ou ajustes;
2. informar qual é a próxima skill após a aprovação;
3. fornecer um exemplo curto de como invocá-la;
4. não executar a próxima skill antes da confirmação humana.

Na validação final, quando não houver uma próxima etapa obrigatória para a mesma funcionalidade, o Codex deve declarar que ela irá para `Done` e indicar a skill adequada para o próximo item do backlog.

## Impacto

- Requisitos e critérios: nenhuma mudança no produto.
- Arquitetura/dados/contratos: nenhum impacto técnico.
- Tarefas, testes e entrega: melhora a orientação do mantenedor sem eliminar gates.
- Compatibilidade e rollback: mudança processual reversível por nova emenda.

## Decisão

Accepted. A constituição passa de `1.1.0` para `1.2.0` porque uma obrigação explícita foi adicionada ao encerramento de cada etapa.

**Aprovado por:** Arthur Rubi, por instrução explícita no pedido de 2026-10-04
**Data:** 2026-10-04
