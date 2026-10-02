---
name: sdd-specify
description: Crie ou refine uma especificação funcional SDD com histórias, requisitos, cenários de aceitação e critérios mensuráveis, sem projetar a implementação.
---

# Especificação SDD

1. Leia `docs/constitution.md`, `docs/sdd-workflow.md`, `docs/traceability.md`, o README do módulo potencialmente afetado e a ideia aceita de origem.
2. Se a ideia não estiver aceita, não crie a spec; informe o gate pendente.
3. Examine `docs/specs/README.md` e specs existentes. Escolha o menor número de spec ainda não usado e um slug curto.
4. Crie `docs/specs/NNN-slug/spec.md` a partir de `docs/templates/spec-template.md`.
5. Escreva em termos de comportamento e valor. Não escolha bibliotecas, endpoints, tabelas ou estrutura de código.
6. Priorize histórias verticais e independentemente testáveis. Cubra caminho feliz, erros relevantes, limites e qualidade mensurável.
7. Garanta IDs únicos e uma matriz inicial de cobertura. Todo `FR-*` deve ser verificável; todo `SC-*` deve ser independente de tecnologia.
8. Marque somente dúvidas materiais como `NEEDS CLARIFICATION`. Quando não houver lacunas bloqueantes, mude o estado para `Ready for review`.
9. Ao chegar a `Ready for review`, apresente a spec, peça aprovação explícita e pare. Nunca marque como `Approved` nem inicie o plano antes dessa nova confirmação; registre aprovador e data quando houver.

Não produza plano técnico ou código nesta etapa. Na resposta final, destaque decisões de produto e dúvidas, e peça aprovação ou ajustes.
