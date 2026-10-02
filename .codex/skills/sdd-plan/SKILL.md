---
name: sdd-plan
description: Produza o plano técnico SDD de uma spec aprovada, incluindo arquitetura, contratos, dados, testes, entrega e verificação da constituição.
---

# Plano técnico SDD

1. Leia `docs/constitution.md`, `docs/sdd-workflow.md`, `docs/traceability.md`, a spec alvo por completo, os READMEs dos módulos afetados e ADRs aplicáveis.
2. Confirme que a spec está `Approved`. Caso contrário, pare e indique o gate pendente.
3. Inspecione o código e configuração reais antes de propor caminhos ou dependências.
4. Crie `plan.md` no diretório da spec usando `docs/templates/plan-template.md`.
5. Resolva incertezas técnicas por pesquisa ou experimento proporcional ao risco. Registre investigação substancial em `research.md`, modelo persistente em `data-model.md` e contratos extensos em `contracts/`.
6. Faça o Constitution Check antes de detalhar a solução e novamente ao terminar. Uma falha não justificada bloqueia `Ready for review`.
7. Mapeie a estratégia de testes aos cenários e riscos da spec. Inclua segurança, privacidade, acessibilidade, observabilidade, migração e rollback quando aplicáveis.
8. Prefira a abordagem mais simples. Crie um ADR somente para decisão durável e transversal.
9. Termine em `Ready for review` sem lacunas materiais. Apresente o plano, peça aprovação explícita e pare. Somente essa nova confirmação permite `Approved` e a criação de tarefas.

Não decomponha todas as tarefas nem implemente código nesta etapa. Na resposta final, resuma decisões, riscos e exceções, e peça aprovação ou ajustes.
