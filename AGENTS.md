# Instruções do repositório

Este projeto usa Spec-Driven Development. Documentação aprovada é fonte de verdade; o código deve implementá-la e nunca substituí-la silenciosamente.

## Leitura obrigatória

Antes de planejar, editar ou revisar qualquer coisa:

1. Leia `docs/constitution.md` por completo.
2. Leia `docs/sdd-workflow.md` e `docs/project-brief.md`.
3. Leia o `README.md` do módulo afetado.
4. Localize em `docs/specs/` a especificação, o plano, as tarefas e a validação relevantes.
5. Consulte os ADRs aplicáveis em `docs/architecture/decisions/`.

Se um artefato obrigatório não existir ou ainda não tiver o estado exigido pelo fluxo, interrompa a implementação e indique exatamente qual etapa SDD falta. Não invente requisitos para preencher lacunas relevantes.

## Regras de execução

- Para nova capacidade ou mudança observável, siga: ideia → spec → plano → tarefas/testes → implementação → validação.
- Não implemente uma funcionalidade sem `spec.md` e `plan.md` aprovados e `tasks.md` pronto.
- Use os templates de `docs/templates/` e mantenha os identificadores de rastreabilidade.
- Atualize artefatos quando decisões ou escopo mudarem; registre o motivo em `docs/changes/` quando a mudança afetar um artefato aprovado.
- Escreva testes a partir dos cenários de aceitação antes ou junto da implementação, conforme definido nas tarefas.
- Execute somente as tarefas selecionadas; não amplie o escopo por conveniência.
- Ao concluir, produza ou atualize `validation.md` com comandos, resultados e pendências reais.
- Mantenha o README do módulo coerente com sua estrutura e seus comandos. Não transforme READMEs em histórico de alterações.

## Organização

- `frontend/`: cliente web React; siga `frontend/README.md`.
- `backend/`: API e persistência; siga `backend/README.md`.
- `docs/`: memória durável do projeto; siga `docs/README.md`.
- `.codex/skills/`: workflows reutilizáveis, não documentação de produto.

Use as skills `$sdd-idea`, `$sdd-specify`, `$sdd-plan`, `$sdd-tasks`, `$sdd-implement` e `$sdd-validate` quando a solicitação corresponder à etapa.
