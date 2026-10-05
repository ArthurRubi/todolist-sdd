import { describe, expect, it } from 'vitest'
import { formatCalendarDate, formatInstant } from './taskDates'

describe('localização temporal', () => {
  it('mantém a data de calendário em localidades diferentes', () => {
    const en = formatCalendarDate('2026-10-20', 'en-US')
    const pt = formatCalendarDate('2026-10-20', 'pt-BR')

    expect(en).toContain('Oct 20, 2026')
    expect(pt).toContain('20')
    expect(pt).toContain('2026')
  })

  it('converte instantes para o fuso solicitado sem alterar o valor persistido', () => {
    const instant = '2026-10-01T12:00:00Z'

    expect(formatInstant(instant, 'en-US', 'UTC')).toContain('12:00 PM')
    expect(formatInstant(instant, 'en-US', 'America/Sao_Paulo')).toContain('9:00 AM')
  })
})
