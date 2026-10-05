import { HttpResponse, http } from 'msw'
import { describe, expect, it } from 'vitest'
import { server } from '../../../test/server'
import {
  changeTaskStatus,
  completeTask,
  createTask,
  listTasks,
  reopenTask,
  updateTask,
} from './taskApi'

const task = {
  id: '33333333-3333-4333-8333-333333333333',
  title: 'Cliente tipado',
  description: null,
  status: 'not_started',
  priority: 'none',
  dueDate: null,
  createdAt: '2026-10-04T12:00:00Z',
  updatedAt: '2026-10-04T12:00:00Z',
  completedAt: null,
} as const

describe('taskApi', () => {
  it('lista a visualização solicitada', async () => {
    server.use(
      http.get('*/api/tasks', ({ request }) => {
        expect(new URL(request.url).searchParams.get('view')).toBe('active')
        return HttpResponse.json([task])
      }),
    )

    await expect(listTasks('active')).resolves.toEqual([task])
  })

  it('envia chave idempotente e o corpo tipado na criação', async () => {
    server.use(
      http.post('*/api/tasks', async ({ request }) => {
        expect(request.headers.get('Idempotency-Key')).toMatch(/^[0-9a-f-]{36}$/)
        expect(await request.json()).toEqual({
          title: 'Cliente tipado',
          description: null,
          priority: 'none',
          dueDate: null,
        })
        return HttpResponse.json(task, { status: 201 })
      }),
    )

    await expect(createTask({
      title: 'Cliente tipado',
      description: null,
      priority: 'none',
      dueDate: null,
    })).resolves.toEqual(task)
  })

  it('mapeia Problem Details e erros por campo', async () => {
    server.use(
      http.post('*/api/tasks', () => HttpResponse.json({
        title: 'Um ou mais campos são inválidos.',
        status: 400,
        traceId: 'trace-123',
        errors: { title: ['O título deve ser informado.'] },
      }, { status: 400 })),
    )

    const promise = createTask({
      title: '',
      description: null,
      priority: 'none',
      dueDate: null,
    })

    await expect(promise).rejects.toMatchObject({
      message: 'O título deve ser informado.',
      fieldErrors: { title: ['O título deve ser informado.'] },
      traceId: 'trace-123',
    })
  })

  it('envia a substituição completa dos campos editáveis', async () => {
    server.use(
      http.put(`*/api/tasks/${task.id}`, async ({ request }) => {
        expect(await request.json()).toEqual({
          title: 'Cliente atualizado',
          description: null,
          priority: 'low',
          dueDate: null,
        })
        return HttpResponse.json({
          ...task,
          title: 'Cliente atualizado',
          priority: 'low',
          updatedAt: '2026-10-05T12:00:00Z',
        })
      }),
    )

    await expect(updateTask(task.id, {
      title: 'Cliente atualizado',
      description: null,
      priority: 'low',
      dueDate: null,
    })).resolves.toMatchObject({
      id: task.id,
      title: 'Cliente atualizado',
      priority: 'low',
    })
  })

  it('envia uma alteração tipada de estado ativo', async () => {
    server.use(
      http.put(`*/api/tasks/${task.id}/status`, async ({ request }) => {
        expect(await request.json()).toEqual({ status: 'in_progress' })
        return HttpResponse.json({ ...task, status: 'in_progress' })
      }),
    )

    await expect(changeTaskStatus(task.id, 'in_progress')).resolves.toMatchObject({
      id: task.id,
      status: 'in_progress',
    })
  })

  it('conclui e reabre usando os comandos contratados', async () => {
    server.use(
      http.post(`*/api/tasks/${task.id}/complete`, () => HttpResponse.json({
        ...task,
        status: 'completed',
        completedAt: '2026-10-05T13:00:00Z',
      })),
      http.post(`*/api/tasks/${task.id}/reopen`, () => HttpResponse.json({
        ...task,
        status: 'not_started',
        completedAt: null,
      })),
    )

    await expect(completeTask(task.id)).resolves.toMatchObject({ status: 'completed' })
    await expect(reopenTask(task.id)).resolves.toMatchObject({
      status: 'not_started',
      completedAt: null,
    })
  })
})
