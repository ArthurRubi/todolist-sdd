# Plano de implementação: [FUNCIONALIDADE]

**Spec:** [link para spec.md]
**Branch:** `codex/[NNN]-[slug]`
**Data:** [AAAA-MM-DD]
**Status:** Draft

## Resumo

[Requisito principal e abordagem técnica escolhida.]

## Contexto técnico

- **Linguagens/versões:** [ou NEEDS CLARIFICATION]
- **Dependências principais:** [ou nenhuma]
- **Persistência:** [PostgreSQL, N/A ou outra decisão justificada]
- **Testes:** [níveis, ferramentas e riscos cobertos]
- **Plataforma alvo:** [navegadores, servidor e ambiente]
- **Metas de desempenho:** [derivadas dos NFRs]
- **Restrições:** [segurança, privacidade, acessibilidade, operação]

## Constitution Check

Avalie cada princípio de `docs/constitution.md`. Um desvio bloqueia o plano até possuir justificativa explícita.

| Princípio/gate | Resultado | Evidência ou ação |
|---|---|---|
| Spec aprovada | PASS/FAIL | [link/nota] |
| Rastreabilidade | PASS/FAIL | [link/nota] |
| Estratégia de testes | PASS/FAIL | [link/nota] |
| Entrega vertical | PASS/FAIL | [link/nota] |
| Simplicidade | PASS/FAIL | [link/nota] |

## Pesquisa e decisões

[Liste incertezas resolvidas. Crie `research.md` quando a investigação for substancial e ADR para decisão durável.]

## Arquitetura e fluxo

[Explique componentes alterados, responsabilidades, fluxo de dados e fronteiras.]

## Estrutura afetada

```text
frontend/
└── [caminhos reais ou planejados]
backend/
└── [caminhos reais ou planejados]
docs/specs/[NNN-slug]/
└── [artefatos]
```

## Dados e migração

[Modelo, integridade, migrações, compatibilidade e rollback; use N/A quando não aplicável.]

## Contratos

[Endpoints, eventos ou formatos. Coloque artefatos extensos em `contracts/`.]

## Estratégia de testes

[Mapeie cenários e riscos para testes unitários, integração, contrato e E2E.]

## Segurança, privacidade e acessibilidade

[Ameaças relevantes, dados sensíveis, autorização e requisitos de acessibilidade.]

## Observabilidade e operação

[Logs, métricas, tratamento de falhas, configuração e publicação.]

## Entrega e rollback

[Ordem segura, feature flags quando justificadas, compatibilidade e como reverter.]

## Complexidade excepcional

| Exceção | Por que é necessária | Alternativa mais simples rejeitada |
|---|---|---|
| [se houver] | | |

## Aprovação

**Aprovado por:** [NOME ou pendente]
**Data:** [AAAA-MM-DD ou pendente]
