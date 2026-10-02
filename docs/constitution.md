# Constituição do TodoList SDD

**Versão:** 1.1.0
**Ratificada em:** 2026-10-02
**Última alteração:** 2026-10-02

## Propósito

Esta constituição define as regras não negociáveis para desenvolver o TodoList como exercício real de Spec-Driven Development. Ela prevalece sobre preferências locais, atalhos de implementação e templates.

## I. A especificação precede o código

Toda mudança que crie ou altere comportamento observável deve possuir uma especificação aprovada antes da implementação. A spec descreve problema, jornadas, requisitos, limites e resultados mensuráveis sem escolher detalhes técnicos prematuramente.

Correções emergenciais podem começar por uma reprodução mínima, mas devem registrar o comportamento esperado e a causa antes do patch.

## II. Gates explícitos entre etapas

O fluxo padrão é ideia aceita, especificação aprovada, plano aprovado, tarefas aprovadas, implementação aceita e validação aceita. Um artefato posterior não pode esconder lacunas do anterior.

Toda etapa termina em `Ready for review` e exige uma confirmação humana explícita antes da próxima. Isso se aplica a ideia, especificação, plano, tarefas, implementação e validação. O Codex pode produzir artefatos e recomendar uma decisão, mas não presumir aprovação nem iniciar automaticamente a etapa seguinte.

## III. Rastreabilidade ponta a ponta

Requisitos, histórias, decisões, tarefas, testes e evidências usam identificadores estáveis. Cada tarefa aponta para a história ou requisito que atende; cada requisito funcional possui ao menos um cenário ou teste de aceitação; cada validação aponta para evidências executadas.

Mudanças de requisito aprovado devem registrar motivo, impacto e artefatos afetados.

## IV. Testes derivam do comportamento

Cenários de aceitação são definidos na spec. O plano escolhe níveis e ferramentas de teste. As tarefas incluem os testes necessários antes da tarefa de implementação correspondente, salvo justificativa registrada. Um teste deve provar comportamento ou risco relevante, não apenas aumentar cobertura.

## V. Entregas verticais e verificáveis

Funcionalidades são divididas em histórias pequenas, priorizadas e independentemente demonstráveis. Cada incremento deve atravessar somente as camadas necessárias e terminar com uma forma concreta de validação. Trabalho estrutural sem valor direto precisa estar vinculado a uma história que o necessita.

## VI. Simplicidade e decisões explícitas

Adote a solução mais simples que satisfaça requisitos e qualidade. Novas abstrações, serviços, dependências ou infraestrutura exigem uma necessidade demonstrável. Decisões arquiteturais relevantes são registradas em ADRs; alternativas descartadas e consequências devem ser claras.

## VII. Documentação viva e localizada

Documentação é parte da entrega. Specs, planos, tarefas e validações são atualizados quando fatos mudam. Cada módulo mantém um README operacional enxuto; ele não duplica requisitos nem serve como changelog.

## Gates mínimos de qualidade

Antes de considerar uma funcionalidade concluída:

- todos os requisitos no escopo têm evidência de validação;
- testes definidos nas tarefas foram executados e seus resultados registrados;
- build, análise estática e migrações aplicáveis foram verificados;
- riscos de segurança, privacidade e acessibilidade foram considerados;
- documentação operacional e contratos afetados estão atualizados;
- desvios ou pendências estão explícitos, nunca mascarados como sucesso.

## Governança

Uma alteração desta constituição requer uma proposta em `docs/changes/`, motivação, impacto e aprovação humana explícita. A versão segue SemVer:

- MAJOR: princípio removido ou redefinido de modo incompatível;
- MINOR: novo princípio ou obrigação material;
- PATCH: esclarecimento sem mudança de obrigação.

Em qualquer conflito, esta constituição vence. Exceções precisam estar no plano da funcionalidade, com justificativa e alternativa mais simples rejeitada.
