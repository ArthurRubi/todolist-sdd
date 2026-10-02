# Rastreabilidade

## Identificadores

- `IDEA-NNN`: ideia ou oportunidade.
- `SPEC-NNN`: funcionalidade; o número coincide com o diretório `docs/specs/NNN-slug/`.
- `US-NN`: história de usuário dentro de uma spec.
- `FR-NNN`: requisito funcional.
- `NFR-NNN`: requisito não funcional ou atributo de qualidade.
- `SC-NNN`: critério mensurável de sucesso.
- `TNNN`: tarefa de implementação ou teste.
- `ADR-NNNN`: decisão arquitetural.
- `CHG-NNN`: mudança em artefato aprovado.

IDs não são reutilizados depois de publicados. Itens removidos ficam marcados como `Deprecated` com motivo.

## Cadeia mínima

```text
IDEA → SPEC → US/FR/NFR/SC → PLAN/ADR → TASK → TEST/EVIDENCE → VALIDATION
```

Cada `tasks.md` deve declarar referências entre colchetes, por exemplo: `T012 [US-01] [FR-003]`. Cada `validation.md` mantém uma matriz ligando requisito, tarefa, teste/evidência e resultado.

## Commits e pull requests

Quando houver uma spec ativa, prefira incluir seu ID e as tarefas relevantes no commit ou PR, por exemplo: `feat(SPEC-001): concluir tarefa T012`. Um commit não precisa corresponder a uma única tarefa, mas a relação deve permanecer verificável.
