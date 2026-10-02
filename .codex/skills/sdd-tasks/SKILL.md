---
name: sdd-tasks
description: Converta uma spec e um plano aprovados em tarefas pequenas, ordenadas e rastreáveis, com testes e checkpoints por história.
---

# Tarefas SDD

1. Leia `docs/constitution.md`, `docs/sdd-workflow.md`, `docs/traceability.md`, a spec e o plano completos, seus documentos auxiliares e READMEs afetados.
2. Confirme que spec e plano estão `Approved`. Caso contrário, pare e informe o gate pendente.
3. Crie `tasks.md` no diretório da spec usando `docs/templates/tasks-template.md`.
4. Organize setup mínimo, fundação realmente bloqueante e depois histórias na ordem de prioridade. Evite uma grande fase técnica que adie todo valor.
5. Dê a cada tarefa ID sequencial, referência de história/requisito, caminho concreto, dependências e resultado verificável.
6. Crie tarefas de teste antes das tarefas de implementação correspondentes. Cubra cada cenário de aceitação e risco definido no plano.
7. Marque `[P]` somente quando as tarefas puderem ser executadas simultaneamente sem dependência ou conflito de arquivo.
8. Inclua checkpoints independentes por história e tarefas finais de documentação e validação.
9. Faça uma revisão de cobertura. Quando não houver requisito órfão, caminho fictício ou dependência ambígua, altere o planejamento para `Ready for review`.
10. Apresente as tarefas, peça aprovação explícita e pare. Somente essa nova confirmação permite marcar o planejamento como `Approved` e iniciar a implementação.

Não implemente tarefas nesta etapa. Na resposta final, informe quantidade por fase, caminho crítico e paralelismo real, e peça aprovação ou ajustes.
