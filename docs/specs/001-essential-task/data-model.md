# Modelo de dados: SPEC-001 — Tarefa essencial

**Data:** 2026-10-04
**Status:** Approved

## Visão geral

O modelo possui uma entidade mutável para o estado atual e um registro imutável de transições. Todas as datas automáticas são instantes UTC; o prazo é uma data de calendário sem horário.

```text
tasks 1 ──── 0..N task_status_events
```

## Tabela `tasks`

| Coluna | Tipo PostgreSQL | Nulo | Regra |
|---|---|---:|---|
| `id` | `uuid` | não | chave primária, gerada pela aplicação |
| `creation_idempotency_key` | `uuid` | não | única; identifica uma tentativa lógica de criação |
| `title` | `varchar(200)` | não | 1–200 caracteres após remover espaços externos |
| `description` | `varchar(10000)` | sim | texto simples; string vazia é normalizada para `null` |
| `status` | `varchar(20)` | não | `not_started`, `in_progress`, `blocked` ou `completed` |
| `priority` | `varchar(10)` | não | `none`, `low`, `medium`, `high` ou `urgent` |
| `due_date` | `date` | sim | data de calendário; datas passadas são válidas |
| `created_at` | `timestamptz` | não | instante UTC definido na criação |
| `updated_at` | `timestamptz` | não | instante UTC renovado em toda alteração efetiva |
| `completed_at` | `timestamptz` | sim | conclusão atual; preenchida somente quando `status = completed` |

### Restrições

- `UNIQUE (creation_idempotency_key)` impede duas tarefas para a mesma tentativa.
- `CHECK (char_length(title) BETWEEN 1 AND 200)` protege o limite essencial no banco; trim e mensagens amigáveis permanecem na API.
- `CHECK (description IS NULL OR char_length(description) <= 10000)`.
- `CHECK` limita `status` e `priority` aos valores definidos.
- `CHECK` garante coerência: status concluído exige `completed_at`; status ativo exige `completed_at IS NULL`.
- `updated_at >= created_at`.

### Índices

- chave única em `creation_idempotency_key`;
- índice em `(status, created_at DESC)` para as listas ativas;
- índice parcial em `(completed_at DESC)` onde `status = 'completed'` para a lista concluída.

## Tabela `task_status_events`

| Coluna | Tipo PostgreSQL | Nulo | Regra |
|---|---|---:|---|
| `id` | `uuid` | não | chave primária, gerada pela aplicação |
| `task_id` | `uuid` | não | FK para `tasks.id`, com exclusão restrita |
| `from_status` | `varchar(20)` | não | estado antes da transição |
| `to_status` | `varchar(20)` | não | estado depois da transição |
| `occurred_at` | `timestamptz` | não | instante UTC da transição |

### Restrições e índices

- `from_status` e `to_status` aceitam somente os quatro estados da spec.
- `CHECK (from_status <> to_status)` evita eventos sem mudança real.
- índice em `(task_id, occurred_at DESC, id DESC)` permite recuperar deterministicamente a transição mais recente.
- eventos são append-only pela aplicação.

## Invariantes de domínio

1. Uma tarefa nova começa em `not_started`, prioridade `none`, `completed_at = null` e não cria evento de transição.
2. Uma edição substitui apenas título, descrição, prioridade e prazo; status e datas automáticas não chegam nesse comando.
3. Uma transição ativa atualiza `status` e `updated_at` e inclui exatamente um evento na mesma transação.
4. Concluir uma tarefa ativa define `status = completed`, `completed_at = agora`, `updated_at = agora` e inclui exatamente um evento.
5. Concluir uma tarefa já concluída não altera linha, instante nem eventos.
6. Reabrir restaura o `from_status` do evento que levou à conclusão atual, define `completed_at = null`, renova `updated_at` e inclui evento `completed → estado restaurado`.
7. Editar uma tarefa concluída renova `updated_at`, mas preserva `status` e `completed_at`.
8. Se qualquer validação ou gravação falhar, tarefa e evento permanecem ambos inalterados.

## Representação e localidade

- A API serializa `due_date` como `YYYY-MM-DD`, sem conversão de fuso.
- Instantes são serializados em UTC com sufixo `Z`; o frontend os formata na localidade do navegador.
- `isOverdue` não é persistido: o frontend deriva `status != completed && due_date < data local atual`, evitando dado obsoleto após a meia-noite.

## Migração inicial

A migração cria as duas tabelas, constraints e índices em uma operação versionada. Não existe backfill. A aplicação não usará `EnsureCreated` nem migração automática em produção.
