# Documentação

Esta pasta é a memória durável e versionada do projeto.

## Mapa

- `constitution.md`: princípios e gates obrigatórios.
- `project-brief.md`: ideia e limites iniciais do produto.
- `sdd-workflow.md`: estados, artefatos e passagem entre etapas.
- `traceability.md`: convenções de IDs e matriz de rastreabilidade.
- `architecture/decisions/`: Architecture Decision Records (ADRs).
- `specs/`: um diretório por funcionalidade.
- `changes/`: propostas que alteram artefatos já aprovados.
- `templates/`: moldes usados pelas skills SDD.

## Regra de proximidade

Os documentos globais explicam regras globais. Detalhes de uma funcionalidade ficam juntos em `specs/NNN-slug/`; detalhes operacionais de um módulo ficam em seu próprio README. Essa divisão reduz contexto irrelevante sem esconder a fonte de verdade.

Documentos aprovados não são reescritos silenciosamente. Se uma mudança alterar escopo, critério de aceite, contrato ou decisão técnica aprovada, registre-a em `changes/` e atualize os artefatos afetados com ligação bidirecional.
