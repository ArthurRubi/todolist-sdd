---
name: sdd-implement
description: Implemente tarefas SDD já prontas, preservando escopo, testes, rastreabilidade e atualização dos artefatos vivos.
---

# Implementação SDD

1. Leia `docs/constitution.md`, `docs/sdd-workflow.md`, a spec, o plano, `tasks.md`, documentos auxiliares, ADRs e READMEs afetados.
2. Exija spec e plano `Approved` e tarefas `Ready`. Confirme quais tarefas ou história estão no escopo atual; se não foi limitado, comece pelo próximo incremento vertical incompleto.
3. Verifique dependências e estado real do repositório. Preserve mudanças não relacionadas.
4. Para cada comportamento, crie ou ajuste primeiro o teste definido, confirme a falha relevante quando viável e então implemente a menor mudança que o faça passar.
5. Execute verificações proporcionais após cada grupo lógico. Não marque uma tarefa sem observar sua condição de conclusão.
6. Atualize checkboxes e evidências curtas em `tasks.md` conforme o trabalho avança. Mantenha contratos e READMEs coerentes.
7. Se uma descoberta mudar requisito, critério, contrato ou decisão aprovada, não improvise: crie um draft em `docs/changes/` a partir do template e pare o trabalho afetado no gate correto.
8. Ao terminar o incremento selecionado, execute as verificações definidas no plano e deixe o repositório pronto para `$sdd-validate`.

Não declare a funcionalidade validada nesta etapa. Na resposta final, liste tarefas concluídas, testes executados, arquivos principais e qualquer desvio ou bloqueio.
