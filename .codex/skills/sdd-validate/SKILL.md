---
name: sdd-validate
description: Valide uma implementação SDD contra requisitos, cenários, tarefas e gates de qualidade, registrando evidências reproduzíveis.
---

# Validação SDD

1. Leia `docs/constitution.md`, a spec, o plano, `tasks.md`, contratos, ADRs e READMEs afetados.
2. Inspecione a implementação e confirme o commit/estado exato validado. Não altere comportamento durante a validação; correções pertencem a uma nova passagem de implementação.
3. Crie ou atualize `validation.md` a partir de `docs/templates/validation-template.md`.
4. Execute os comandos de build, análise estática, testes e migração definidos no plano. Registre comando e resultado reais, inclusive falhas.
5. Percorra cada cenário e requisito no escopo. Preencha a matriz final com tarefa, teste ou evidência e resultado; nenhum requisito pode ficar implícito.
6. Faça validação manual ou exploratória quando automação não provar experiência, acessibilidade ou integração suficiente.
7. Compare a entrega com os gates da constituição e verifique documentação e contratos.
8. Defina `Passed` somente se todos os itens obrigatórios passarem. Use `Failed` para lacuna bloqueante e `Passed with exceptions` apenas com risco, justificativa e próximo passo explícitos.

Na resposta final, comece pelo veredito e resuma evidências, falhas, riscos e ações necessárias. Não esconda comando não executado como aprovação.
