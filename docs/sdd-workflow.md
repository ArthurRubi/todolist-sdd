# Workflow de Spec-Driven Development

## Visão geral

```text
IDEA ──accepted──▶ SPEC ──approved──▶ PLAN ──approved──▶ TASKS
                                                        │ approved
                                                        ▼
                                                  IMPLEMENTATION
                                                        │ accepted
                                                        ▼
                                                   VALIDATION
                                                        │ accepted
                                                        ▼
                                                      DONE
```

Qualquer descoberta que altere um artefato aprovado abre um change record e retorna ao gate afetado.

## 1. Ideia

**Entrada:** problema, oportunidade ou solicitação ainda informal.
**Saída:** `docs/ideas/NNN-slug.md` a partir de `templates/idea-template.md`, ou atualização do project brief quando for visão global.
**Estados:** `Draft → Ready for review → Accepted | Rejected`.

O artefato explica problema, usuários, valor, limites e dúvidas. Não define arquitetura detalhada.

## 2. Especificação

**Entrada:** ideia aceita.
**Saída:** `docs/specs/NNN-slug/spec.md`.
**Estados:** `Draft → Ready for review → Approved`.

A spec contém histórias priorizadas e independentes, cenários Given/When/Then, requisitos `FR-*`, requisitos de qualidade `NFR-*`, entidades conceituais, fora de escopo e resultados mensuráveis `SC-*`. Ela deve permanecer tecnológica e comercialmente compreensível.

## 3. Plano técnico

**Entrada:** spec aprovada.
**Saída:** `plan.md` e, quando necessários, `research.md`, `data-model.md` e `contracts/` no mesmo diretório.
**Estados:** `Draft → Ready for review → Approved`.

O plano resolve dúvidas técnicas, verifica a constituição, define arquitetura, migração, observabilidade, segurança, testes e rollback. Decisões duráveis também geram ADR.

## 4. Tarefas e testes

**Entrada:** spec e plano aprovados.
**Saída:** `tasks.md`.
**Estados:** `Draft → Ready for review → Approved`.

Tarefas são pequenas, ordenadas, têm caminhos concretos, dependências e links para histórias/requisitos. Testes necessários aparecem antes das tarefas de implementação que eles verificam. A implementação não começa antes da aprovação humana do conjunto de tarefas.

## 5. Implementação

**Entrada:** tarefas prontas e escopo de execução selecionado.
**Saída:** código, testes, migrações e documentação; checkboxes e notas de evidência atualizados em `tasks.md`.
**Estados:** `Not started → In progress → Ready for review → Accepted`.

Implemente por história vertical. Ao descobrir conflito com a spec ou o plano, pare, registre a mudança e retorne ao gate correto.

Ao concluir o incremento combinado, apresente mudanças e evidências e aguarde aceite humano antes de iniciar a validação formal.

## 6. Validação

**Entrada:** tarefas do incremento concluídas.
**Saída:** `validation.md` criado a partir do template.
**Estados:** `Draft → Ready for review → Accepted | Rejected`.
**Veredito técnico:** `Passed | Failed | Passed with exceptions`.

Validação combina evidências automatizadas e revisão dos critérios de aceite. `Passed with exceptions` exige riscos e próximos passos explícitos; não equivale silenciosamente a concluído. Mesmo com veredito `Passed`, a funcionalidade somente vai para `Done` após aceite humano do relatório.

## Aprovação e mudanças

O autor do artefato não presume aprovação. Ao finalizar qualquer etapa, deve parar, apresentar o resultado e pedir aprovação explícita para avançar. A confirmação humana deve ser registrada no artefato com data. Mudanças editoriais podem ser aplicadas diretamente; mudanças de significado em artefatos aprovados usam `docs/changes/CHG-NNN-slug.md`.

## Skills do Codex

As skills de `.codex/skills/` materializam cada etapa. Elas usam os documentos deste diretório como fonte de verdade e não substituem os gates humanos.
