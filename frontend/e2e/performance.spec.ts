import { execFileSync } from 'node:child_process'
import { randomUUID } from 'node:crypto'
import { expect, test } from '@playwright/test'

test.describe('desempenho transversal — SPEC-001', () => {
  test('apresenta p95 em até um segundo para 100 operações com 1.000 tarefas', async ({ page }) => {
    test.setTimeout(120_000)
    seedTasks(999)
    await page.goto('/')
    await expect(page.getByRole('region', { name: 'Tarefas ativas' }).getByRole('article')).toHaveCount(999)

    const durations: number[] = []
    const initialTitle = `Carga UI ${Date.now()}`
    const titleInput = page.getByLabel('Título')
    await titleInput.fill(initialTitle)
    let startedAt = performance.now()
    await titleInput.press('Enter')
    await expect(page.getByRole('heading', { name: initialTitle })).toBeVisible()
    durations.push(performance.now() - startedAt)

    await page.getByRole('button', { name: `Abrir detalhes de ${initialTitle}` }).click()
    const dialog = page.getByRole('dialog')
    let currentTitle = initialTitle

    for (let index = 0; index < 49; index += 1) {
      const nextTitle = `${initialTitle} ${index % 2 === 0 ? 'A' : 'B'}`
      await dialog.getByLabel('Título').fill(nextTitle)
      startedAt = performance.now()
      await dialog.getByRole('button', { name: 'Salvar alterações' }).click()
      await expect(page.getByRole('heading', { name: nextTitle })).toBeVisible()
      durations.push(performance.now() - startedAt)
      currentTitle = nextTitle
      await page.getByRole('button', { name: `Abrir detalhes de ${currentTitle}` }).click()
    }

    for (let index = 0; index < 50; index += 1) {
      const status = index % 2 === 0 ? 'in_progress' : 'blocked'
      const label = status === 'in_progress' ? 'Em andamento' : 'Bloqueada'
      await dialog.getByLabel('Estado ativo').selectOption(status)
      startedAt = performance.now()
      await dialog.getByRole('button', { name: 'Salvar estado' }).click()
      await expect(dialog.locator('p').filter({ hasText: 'Estado atual:' })).toContainText(label)
      durations.push(performance.now() - startedAt)
    }

    const p95 = percentile95(durations)
    console.log(`PERFORMANCE SPEC-001: operações=${durations.length}; p95=${p95.toFixed(2)}ms; max=${Math.max(...durations).toFixed(2)}ms`)
    expect(durations).toHaveLength(100)
    expect(p95).toBeLessThanOrEqual(1_000)
  })
})

function seedTasks(count: number) {
  const prefix = randomUUID()
  const sql = `
    TRUNCATE TABLE task_status_events, tasks CASCADE;
    INSERT INTO tasks (id, creation_idempotency_key, title, status, priority, created_at, updated_at)
    SELECT
      gen_random_uuid(),
      gen_random_uuid(),
      'Carga ${prefix} ' || to_char(sequence_number, 'FM0000'),
      'not_started',
      'none',
      NOW() - (sequence_number * INTERVAL '1 millisecond'),
      NOW() - (sequence_number * INTERVAL '1 millisecond')
    FROM generate_series(1, ${count}) AS sequence_number;
  `
  const executable = process.env.CI ? 'docker' : 'docker-compose'
  const composePrefix = process.env.CI ? ['compose'] : []
  execFileSync(executable, [
    ...composePrefix, '-f', '../compose.yaml', 'exec', '-T', 'postgres',
    'psql', '-U', 'todolist', '-d', 'todolist', '-v', 'ON_ERROR_STOP=1', '-c', sql,
  ])
}

function percentile95(values: number[]) {
  const ordered = [...values].sort((left, right) => left - right)
  return ordered[Math.ceil(ordered.length * 0.95) - 1]
}
