import { describe, expect, it } from 'vitest'
import type { components } from './schema'

type TaskStatus = components['schemas']['TaskStatus']
type TaskPriority = components['schemas']['TaskPriority']

describe('contrato gerado da Tasks API', () => {
  it('mantém os status e prioridades aprovados', () => {
    const statuses = ['not_started', 'in_progress', 'blocked', 'completed'] satisfies TaskStatus[]
    const priorities = ['none', 'low', 'medium', 'high', 'urgent'] satisfies TaskPriority[]

    expect(statuses).toHaveLength(4)
    expect(priorities).toHaveLength(5)
  })
})
