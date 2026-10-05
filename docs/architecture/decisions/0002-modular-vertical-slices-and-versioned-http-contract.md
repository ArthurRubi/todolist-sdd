# ADR-0002: Organizar o monólito por fatias verticais e versionar o contrato HTTP

**Status:** Accepted
**Data:** 2026-10-04
**Relacionada a:** SPEC-001

## Contexto

O ADR-0001 definiu monorepo, React, .NET, PostgreSQL e OpenAPI, mas deixou a organização interna e o uso do contrato para o primeiro plano. A SPEC-001 possui um único agregado e sete operações HTTP, ao mesmo tempo em que exige rastreabilidade entre interface, API e persistência.

Separar o backend em muitas camadas ou serviços aumentaria navegação, projetos e abstrações sem uma segunda fronteira de negócio. Manter modelos HTTP independentes escritos à mão no frontend e backend criaria risco de divergência.

## Decisão

- Manter uma SPA e uma única API implantável.
- Organizar código de produção por funcionalidade/fatia vertical, mantendo endpoint, validação, regra e persistência da tarefa próximos.
- Começar com um projeto de produção no backend; separar projetos apenas por necessidades de teste.
- Versionar OpenAPI 3.1 como contrato HTTP pretendido.
- Gerar o documento da implementação no build, compará-lo semanticamente ao contrato e gerar os tipos TypeScript a partir do artefato aprovado.
- Extrair bibliotecas ou serviços somente quando uma spec futura revelar uma fronteira independente ou necessidade de implantação separada.

## Consequências

- Uma jornada pode ser localizada e alterada sem atravessar camadas artificiais.
- O contrato torna divergências entre backend e frontend detectáveis na CI.
- Tipos gerados reduzem duplicação, mas exigem um passo de geração e verificação no build.
- O projeto de API poderá crescer; limites internos e testes precisarão ser preservados para evitar acoplamento acidental.
- A extração futura continua possível, mas dependerá de evidência e nova decisão.

## Alternativas consideradas

- **Arquitetura em quatro projetos (`Domain`, `Application`, `Infrastructure`, `Api`):** fornece fronteiras de compilação, mas é desproporcional ao agregado e casos de uso atuais.
- **Microserviços:** não existe requisito de escala, equipe ou implantação independente.
- **Organização por camada técnica:** é familiar, mas espalha uma pequena mudança de tarefa por diretórios sem melhorar isolamento.
- **DTOs TypeScript manuais:** começam simples, mas permitem drift silencioso contra a API.
- **SDK completo gerado:** adiciona abstração e código demais para o contrato pequeno; tipos gerados e um cliente `fetch` fino bastam.

## Evidências

- [Plano da SPEC-001](../../specs/001-essential-task/plan.md)
- [Pesquisa da SPEC-001](../../specs/001-essential-task/research.md)
- [Contrato da SPEC-001](../../specs/001-essential-task/contracts/tasks-api.openapi.yaml)
