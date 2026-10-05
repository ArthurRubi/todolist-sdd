import { expect, test } from '@playwright/test'

test.describe('US-01 — criar e visualizar tarefa', () => {
  test('cria tarefa mínima pelo teclado e a reencontra após reload', async ({ page }) => {
    const title = `Tarefa mínima ${Date.now()}`
    await page.goto('/')

    await page.getByLabel('Título').fill(title)
    await page.getByLabel('Título').press('Enter')

    await expect(page.getByText('Tarefa criada com sucesso.')).toBeVisible()
    await expect(page.getByRole('heading', { name: title })).toBeVisible()

    await page.reload()
    await expect(page.getByRole('heading', { name: title })).toBeVisible()
    await expect(page.getByText('Não iniciada')).toBeVisible()
    await expect(page.getByText('Sem prioridade')).toBeVisible()
  })

  test('cria tarefa detalhada com prazo passado e a identifica como atrasada', async ({ page }) => {
    const title = `Tarefa detalhada ${Date.now()}`
    await page.goto('/')
    await page.getByRole('button', { name: 'Adicionar detalhes' }).click()
    await page.getByLabel('Título').fill(title)
    await page.getByLabel('Descrição').fill('Linha um\nLinha dois')
    await page.getByLabel('Prioridade').selectOption('urgent')
    await page.getByLabel('Prazo').fill('2000-01-02')
    await page.getByRole('button', { name: 'Adicionar tarefa' }).click()

    const card = page.getByRole('article').filter({ hasText: title })
    await expect(card.getByText('Linha um')).toBeVisible()
    await expect(card.getByText('Prioridade urgente')).toBeVisible()
    await expect(card.getByText('Atrasada')).toBeVisible()
  })

  test('rejeita título vazio sem apagar os detalhes digitados', async ({ page }) => {
    await page.goto('/')
    await page.getByRole('button', { name: 'Adicionar detalhes' }).click()
    await page.getByLabel('Título').fill('   ')
    await page.getByLabel('Descrição').fill('Informação que não pode sumir')
    await page.getByLabel('Prioridade').selectOption('high')
    await page.getByLabel('Prazo').fill('2030-03-04')
    await page.getByRole('button', { name: 'Adicionar tarefa' }).click()

    await expect(page.getByRole('alert')).toContainText('título')
    await expect(page.getByLabel('Título')).toHaveValue('   ')
    await expect(page.getByLabel('Descrição')).toHaveValue('Informação que não pode sumir')
    await expect(page.getByLabel('Prioridade')).toHaveValue('high')
    await expect(page.getByLabel('Prazo')).toHaveValue('2030-03-04')
  })
})
