import { expect, test } from '@playwright/test'

test.describe('US-03 — acompanhar e alterar o estado', () => {
  test('executa o ciclo completo por teclado e preserva conclusões idempotentes', async ({ page }) => {
    const title = `Ciclo de estado ${Date.now()}`
    await page.goto('/')

    const titleInput = page.getByLabel('Título')
    await titleInput.focus()
    await page.keyboard.insertText(title)
    await page.keyboard.press('Enter')
    await expect(page.getByRole('heading', { name: title })).toBeVisible()

    const detailsButton = page.getByRole('button', { name: `Abrir detalhes de ${title}` })
    await detailsButton.focus()
    await page.keyboard.press('Enter')
    const dialog = page.getByRole('dialog')
    const taskId = (await dialog.locator('dd').first().textContent())!.trim()

    const status = dialog.getByLabel('Estado ativo')
    await status.press('e')
    await expect(status).toHaveValue('in_progress')
    await expect(dialog.getByRole('button', { name: 'Salvar estado' })).toBeEnabled()
    await page.keyboard.press('Tab')
    await page.keyboard.press('Enter')
    await expect(dialog.locator('p').filter({ hasText: 'Estado atual:' })).toContainText('Em andamento')

    const completeButton = dialog.getByRole('button', { name: 'Concluir tarefa' })
    await completeButton.focus()
    await page.keyboard.press('Enter')
    await expect(dialog.getByRole('button', { name: 'Reabrir tarefa' })).toBeFocused()
    await expect(dialog.getByTestId('completed-at')).toBeVisible()
    await expect(page.getByRole('region', { name: 'Concluídas' }).getByRole('heading', { name: title })).toBeVisible()

    const firstRepeat = await page.request.post(`http://127.0.0.1:5080/api/tasks/${taskId}/complete`)
    const secondRepeat = await page.request.post(`http://127.0.0.1:5080/api/tasks/${taskId}/complete`)
    expect(firstRepeat.ok()).toBeTruthy()
    expect(secondRepeat.ok()).toBeTruthy()
    const firstCompletion = (await firstRepeat.json()).completedAt
    expect(firstCompletion).toBe((await secondRepeat.json()).completedAt)

    await dialog.getByRole('button', { name: 'Reabrir tarefa' }).press('Enter')
    await expect(dialog.getByLabel('Estado ativo')).toBeFocused()
    await expect(dialog.locator('p').filter({ hasText: 'Estado atual:' })).toContainText('Em andamento')
    await expect(page.getByRole('region', { name: 'Tarefas ativas' }).getByRole('heading', { name: title })).toBeVisible()

    await dialog.getByRole('button', { name: 'Concluir tarefa' }).focus()
    await page.keyboard.press('Enter')
    await expect(dialog.getByRole('button', { name: 'Reabrir tarefa' })).toBeFocused()
    const currentResponse = await page.request.get(`http://127.0.0.1:5080/api/tasks/${taskId}`)
    expect((await currentResponse.json()).completedAt).not.toBe(firstCompletion)
  })
})
