import { execFileSync } from 'node:child_process'
import { randomUUID } from 'node:crypto'
import { expect, test } from '@playwright/test'

test.describe('US-02 — editar detalhes essenciais', () => {
  test('edita todos os campos e preserva a atualização após reload', async ({ page }) => {
    const originalTitle = `Editar todos ${Date.now()}`
    const updatedTitle = `${originalTitle} revisada`
    await createDetailedTask(page, originalTitle)

    await page.getByRole('button', { name: `Abrir detalhes de ${originalTitle}` }).click()
    const dialog = page.getByRole('dialog')
    await dialog.getByLabel('Título').fill(updatedTitle)
    await dialog.getByLabel('Descrição').fill('Descrição atualizada\ncom duas linhas')
    await dialog.getByLabel('Prioridade').selectOption('low')
    await dialog.getByLabel('Prazo').fill('2032-08-09')
    await dialog.getByRole('button', { name: 'Salvar alterações' }).click()

    await expect(page.getByText('Tarefa atualizada com sucesso.')).toBeVisible()
    await expect(page.getByRole('heading', { name: updatedTitle })).toBeVisible()
    await expect(page.getByRole('button', { name: `Abrir detalhes de ${updatedTitle}` })).toBeFocused()
    await page.reload()
    const updatedCard = page.getByRole('article').filter({ hasText: updatedTitle })
    await expect(updatedCard.getByRole('heading', { name: updatedTitle })).toBeVisible()
    await expect(updatedCard).toContainText('Descrição atualizada')
    await expect(updatedCard).toContainText('Prioridade baixa')
  })

  test('remove opcionais e permite corrigir uma edição inválida sem redigitar os demais campos', async ({ page }) => {
    const title = `Corrigir edição ${Date.now()}`
    await createDetailedTask(page, title)
    await page.getByRole('button', { name: `Abrir detalhes de ${title}` }).click()
    let dialog = page.getByRole('dialog')

    await dialog.getByLabel('Título').fill('   ')
    await dialog.getByLabel('Descrição').fill('Texto que deve permanecer')
    await dialog.getByLabel('Prioridade').selectOption('urgent')
    await dialog.getByLabel('Prazo').fill('2033-04-05')
    await dialog.getByRole('button', { name: 'Salvar alterações' }).click()

    await expect(dialog.getByRole('alert')).toContainText('título')
    await expect(dialog.getByLabel('Descrição')).toHaveValue('Texto que deve permanecer')
    await expect(dialog.getByLabel('Prioridade')).toHaveValue('urgent')
    await expect(dialog.getByLabel('Prazo')).toHaveValue('2033-04-05')

    const correctedTitle = `${title} corrigida`
    await dialog.getByLabel('Título').fill(correctedTitle)
    await dialog.getByLabel('Descrição').fill('')
    await dialog.getByLabel('Prioridade').selectOption('none')
    await dialog.getByLabel('Prazo').fill('')
    await dialog.getByRole('button', { name: 'Salvar alterações' }).click()

    await expect(page.getByRole('heading', { name: correctedTitle })).toBeVisible()
    await expect(page.getByRole('article').filter({ hasText: correctedTitle })).toContainText('Sem prioridade')
    await page.reload()
    await expect(page.getByRole('heading', { name: correctedTitle })).toBeVisible()
  })

  test('edita tarefa concluída sem reabri-la nem alterar sua conclusão', async ({ page }) => {
    const originalTitle = `Concluída editável ${Date.now()}`
    const taskId = seedCompletedTask(originalTitle)
    await page.goto('/')

    await page.getByRole('button', { name: `Abrir detalhes de ${originalTitle}` }).click()
    const dialog = page.getByRole('dialog')
    const completionBefore = await dialog.getByTestId('completed-at').textContent()
    const updatedTitle = `${originalTitle} revisada`
    await dialog.getByLabel('Título').fill(updatedTitle)
    await dialog.getByLabel('Descrição').fill('Ainda concluída')
    await dialog.getByRole('button', { name: 'Salvar alterações' }).click()

    const completedCard = page.getByRole('article').filter({ hasText: updatedTitle })
    await expect(completedCard).toContainText('Concluída')
    await page.getByRole('button', { name: `Abrir detalhes de ${updatedTitle}` }).click()
    await expect(page.getByRole('dialog').getByTestId('completed-at')).toHaveText(completionBefore ?? '')
    await expect(page.getByRole('dialog')).toContainText(taskId)
  })
})

async function createDetailedTask(page: import('@playwright/test').Page, title: string) {
  await page.goto('/')
  await page.getByRole('button', { name: 'Adicionar detalhes' }).click()
  await page.getByLabel('Título').fill(title)
  await page.getByLabel('Descrição').fill('Descrição original')
  await page.getByLabel('Prioridade').selectOption('high')
  await page.getByLabel('Prazo').fill('2030-01-02')
  await page.getByRole('button', { name: 'Adicionar tarefa' }).click()
  await expect(page.getByRole('heading', { name: title })).toBeVisible()
}

function seedCompletedTask(title: string) {
  const id = randomUUID()
  const key = randomUUID()
  const eventId = randomUUID()
  const sql = `
    INSERT INTO tasks (
      id, creation_idempotency_key, title, description, status, priority,
      due_date, created_at, updated_at, completed_at
    ) VALUES (
      '${id}', '${key}', '${title}', 'Descrição concluída', 'completed', 'medium',
      '2026-10-01', '2026-10-01T12:00:00Z', '2026-10-04T18:45:00Z', '2026-10-04T18:45:00Z'
    );
    INSERT INTO task_status_events (id, task_id, from_status, to_status, occurred_at)
    VALUES ('${eventId}', '${id}', 'not_started', 'completed', '2026-10-04T18:45:00Z');
  `
  const executable = process.env.CI ? 'docker' : 'docker-compose'
  const composePrefix = process.env.CI ? ['compose'] : []
  execFileSync(executable, [
    ...composePrefix, '-f', '../compose.yaml', 'exec', '-T', 'postgres',
    'psql', '-U', 'todolist', '-d', 'todolist', '-v', 'ON_ERROR_STOP=1', '-c', sql,
  ])
  return id
}
