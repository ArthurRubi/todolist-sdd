# Tarefas: [FUNCIONALIDADE]

**Spec:** [link]
**Plano:** [link]
**Status:** Draft

## Formato

`- [ ] TNNN [P?] [US-NN] [FR/NFR-NNN] Descrição com caminho exato`

- `[P]` significa executável em paralelo sem conflito de arquivo ou dependência.
- Toda tarefa aponta para uma história ou requisito, salvo setup compartilhado justificado.
- Tarefas de teste precedem a implementação que verificam.
- Cada tarefa deve produzir uma mudança pequena e verificável.

## Fase 1 — Setup

- [ ] T001 [referência] [ação concreta e caminho]

## Fase 2 — Fundação bloqueante

- [ ] T002 [referência] [pré-requisito comum indispensável]

**Checkpoint:** fundação verificada; histórias podem começar.

## Fase 3 — US-01: [título] (P1 / MVP)

**Objetivo:** [valor entregue]
**Teste independente:** [como demonstrar]

### Testes

- [ ] T010 [P] [US-01] [FR-001] Criar teste que inicialmente falha em `[caminho]`.

### Implementação

- [ ] T011 [US-01] [FR-001] Implementar comportamento em `[caminho]`; depende de T010.

### Documentação e evidência

- [ ] T012 [US-01] [SC-001] Atualizar `[documento]` e registrar comando/evidência.

**Checkpoint:** US-01 funciona e pode ser validada isoladamente.

## Fase final — Qualidade transversal

- [ ] T090 [referência] Executar build, lint/análise estática e suíte aplicável.
- [ ] T091 [referência] Atualizar READMEs, contrato e documentação afetados.
- [ ] T092 [referência] Criar ou atualizar `validation.md`.

## Dependências e ordem

[Descreva dependências reais entre fases/tarefas e oportunidades de paralelismo.]

## Registro de execução

Ao marcar uma tarefa, acrescente evidência curta quando ela não for óbvia: commit, teste, comando ou decisão. Não marque tarefa incompleta.
