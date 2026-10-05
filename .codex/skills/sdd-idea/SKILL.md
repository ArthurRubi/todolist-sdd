---
name: sdd-idea
description: Estruture uma solicitação ainda informal como ideia de produto SDD antes de especificar solução ou funcionalidade.
---

# Ideia SDD

Use quando o problema, público, valor ou limite ainda estiverem em descoberta. Não use para uma funcionalidade que já possui ideia aceita e precisa de spec.

1. Leia `docs/constitution.md`, `docs/sdd-workflow.md`, `docs/project-brief.md` e `docs/ideas/README.md` por completo.
2. Verifique ideias e specs existentes para evitar duplicação.
3. Escolha o menor número de ideia ainda não usado e um slug curto.
4. Crie `docs/ideas/NNN-slug.md` a partir de `docs/templates/idea-template.md`.
5. Separe claramente fatos, premissas, dúvidas e itens fora de escopo. Não escolha arquitetura nesta etapa.
6. Use `NEEDS CLARIFICATION` apenas quando a resposta mudar materialmente valor ou limites; faça suposições reversíveis quando seguro e registre-as.
7. Termine em `Ready for review` quando o documento estiver completo. Apresente o resultado, peça aprovação explícita e pare. Somente essa confirmação permite mudar para `Accepted` e iniciar a spec.

Na resposta final, resuma valor, limites e decisões propostas, e peça que o usuário aprove, rejeite ou solicite ajustes. Informe que, após a aprovação, a próxima etapa é `$sdd-specify` e dê um exemplo curto de invocação. Não inicie a especificação antes de uma nova confirmação, mesmo que o pedido original mencione etapas posteriores.
