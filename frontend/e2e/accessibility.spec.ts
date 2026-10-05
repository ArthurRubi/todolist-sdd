import AxeBuilder from '@axe-core/playwright'
import { expect, test } from '@playwright/test'

test.describe('acessibilidade transversal — SPEC-001', () => {
  test('não apresenta violações axe nas superfícies principal e de detalhes', async ({ page }) => {
    const title = `Axe ${Date.now()}`
    await page.goto('/')

    const mainResults = await new AxeBuilder({ page }).analyze()
    expect(mainResults.violations).toEqual([])

    await page.getByLabel('Título').fill(title)
    await page.getByLabel('Título').press('Enter')
    await page.getByRole('button', { name: `Abrir detalhes de ${title}` }).click()

    const dialogResults = await new AxeBuilder({ page }).analyze()
    expect(dialogResults.violations).toEqual([])
  })

  test('mantém ordem previsível, foco visível e nomes acessíveis', async ({ page }) => {
    await page.goto('/')

    const title = page.getByLabel('Título')
    await expect(title).toBeFocused()
    await expect(title).toHaveAccessibleName('Título')
    await page.keyboard.press('Tab')
    await expect(page.getByRole('button', { name: 'Adicionar tarefa' })).toBeFocused()
    await expect(page.getByRole('button', { name: 'Adicionar tarefa' })).toHaveCSS('outline-style', 'solid')
    await page.keyboard.press('Tab')
    await expect(page.getByRole('button', { name: 'Adicionar detalhes' })).toBeFocused()
  })

  test('mantém o foco no diálogo, fecha com Escape e restaura o acionador', async ({ page }) => {
    const title = `Foco modal ${Date.now()}`
    await page.goto('/')
    await page.getByLabel('Título').fill(title)
    await page.getByLabel('Título').press('Enter')
    const trigger = page.getByRole('button', { name: `Abrir detalhes de ${title}` })
    await trigger.click()
    const dialog = page.getByRole('dialog')
    const close = dialog.getByRole('button', { name: 'Fechar detalhes' })
    await close.focus()
    await page.keyboard.press('Shift+Tab')
    await expect(dialog.getByRole('button', { name: 'Concluir tarefa' })).toBeFocused()
    await page.keyboard.press('Escape')
    await expect(dialog).not.toBeVisible()
    await expect(trigger).toBeFocused()
  })

  test('completa criação, edição e ciclo de estado apenas pelo teclado', async ({ page }) => {
    const title = `Teclado integral ${Date.now()}`
    const editedTitle = `${title} editada`
    await page.goto('/')

    await page.getByLabel('Título').pressSequentially(title)
    await page.keyboard.press('Enter')
    await expect(page.getByText('Tarefa criada com sucesso.')).toBeVisible()

    const openDetails = page.getByRole('button', { name: `Abrir detalhes de ${title}` })
    await openDetails.focus()
    await page.keyboard.press('Enter')
    const dialog = page.getByRole('dialog')
    await expect(dialog.getByLabel('Título')).toBeFocused()
    await dialog.getByLabel('Título').fill(editedTitle)
    await dialog.getByRole('button', { name: 'Salvar alterações' }).focus()
    await page.keyboard.press('Enter')
    await expect(page.getByText('Tarefa atualizada com sucesso.')).toBeVisible()

    const reopenDetails = page.getByRole('button', { name: `Abrir detalhes de ${editedTitle}` })
    await reopenDetails.focus()
    await page.keyboard.press('Enter')
    await dialog.getByLabel('Estado ativo').selectOption('blocked')
    await dialog.getByRole('button', { name: 'Salvar estado' }).focus()
    await page.keyboard.press('Enter')
    await expect(dialog).toContainText('Estado atual: Bloqueada')
    await dialog.getByRole('button', { name: 'Concluir tarefa' }).focus()
    await page.keyboard.press('Enter')
    await expect(dialog.getByRole('button', { name: 'Reabrir tarefa' })).toBeFocused()
    await expect(page.getByRole('region', { name: 'Concluídas' })).toContainText(editedTitle)
  })

  test('comunica prioridade, atraso, estado, sucesso e erro também por texto', async ({ page }) => {
    const title = `Não cromática ${Date.now()}`
    await page.goto('/')
    await page.getByRole('button', { name: 'Adicionar detalhes' }).click()
    await page.getByLabel('Título').fill(title)
    await page.getByLabel('Prioridade').selectOption('urgent')
    await page.getByLabel('Prazo').fill('2000-01-02')
    await page.getByRole('button', { name: 'Adicionar tarefa' }).click()

    const card = page.getByRole('article').filter({ hasText: title })
    await expect(card).toContainText('Não iniciada')
    await expect(card).toContainText('Prioridade urgente')
    await expect(card).toContainText('Atrasada')
    await expect(page.getByRole('status')).toContainText('sucesso')

    await page.getByLabel('Título').fill('   ')
    await page.getByRole('button', { name: 'Adicionar tarefa' }).click()
    await expect(page.getByRole('alert')).toContainText('título')
  })
})
