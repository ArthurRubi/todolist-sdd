import { expect, test } from '@playwright/test'

test('carrega a aplicação', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('heading', { name: 'Seu dia, com espaço para respirar.' })).toBeVisible()
})
