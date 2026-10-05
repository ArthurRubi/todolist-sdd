import { HttpResponse, http } from 'msw'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'
import { App } from '../../../app/App'
import { server } from '../../../test/server'

const activeTask = {
  id: '11111111-1111-4111-8111-111111111111',
  title: 'Renovar documento',
  description: null,
  status: 'not_started',
  priority: 'high',
  dueDate: '2000-01-02',
  createdAt: '2026-10-04T12:00:00Z',
  updatedAt: '2026-10-04T12:00:00Z',
  completedAt: null,
} as const

describe('criação e visualização de tarefas', () => {
  beforeEach(() => {
    server.use(
      http.get('*/api/tasks', ({ request }) => {
        const view = new URL(request.url).searchParams.get('view')
        return HttpResponse.json(view === 'completed' ? [] : [activeTask])
      }),
    )
  })

  it('resume e distingue tarefa ativa atrasada sem depender apenas de cor', async () => {
    render(<App />)

    expect(await screen.findByText('Renovar documento')).toBeInTheDocument()
    expect(screen.getByText('1 ativa')).toBeInTheDocument()
    expect(screen.getByText('0 concluídas')).toBeInTheDocument()
    expect(screen.getByText('Atrasada')).toBeInTheDocument()
    expect(screen.getByText('Prioridade alta')).toBeInTheDocument()
  })

  it('cria uma tarefa mínima e limpa o formulário somente após sucesso', async () => {
    const user = userEvent.setup()
    let submittedBody: unknown
    server.use(
      http.post('*/api/tasks', async ({ request }) => {
        submittedBody = await request.json()
        return HttpResponse.json(
          {
            ...activeTask,
            id: '22222222-2222-4222-8222-222222222222',
            title: 'Comprar café',
            priority: 'none',
            dueDate: null,
          },
          { status: 201 },
        )
      }),
    )
    render(<App />)
    const title = screen.getByLabelText('Título')

    await user.type(title, 'Comprar café')
    await user.click(screen.getByRole('button', { name: 'Adicionar tarefa' }))

    expect(await screen.findByText('Tarefa criada com sucesso.')).toBeInTheDocument()
    expect(submittedBody).toEqual({
      title: 'Comprar café',
      description: null,
      priority: 'none',
      dueDate: null,
    })
    expect(title).toHaveValue('')
    expect(screen.getByText('Comprar café')).toBeInTheDocument()
  })

  it('preserva todos os campos e anuncia o erro de validação', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('*/api/tasks', () =>
        HttpResponse.json(
          {
            type: 'https://httpstatuses.com/400',
            title: 'Um ou mais campos são inválidos.',
            status: 400,
            traceId: 'test-trace',
            errors: { title: ['O título deve ser informado.'] },
          },
          { status: 400, headers: { 'Content-Type': 'application/problem+json' } },
        ),
      ),
    )
    render(<App />)

    await user.click(screen.getByRole('button', { name: 'Adicionar detalhes' }))
    await user.type(screen.getByLabelText('Título'), '   ')
    await user.type(screen.getByLabelText('Descrição'), 'Linha um\nLinha dois')
    await user.selectOptions(screen.getByLabelText('Prioridade'), 'urgent')
    await user.type(screen.getByLabelText('Prazo'), '2000-01-02')
    await user.click(screen.getByRole('button', { name: 'Adicionar tarefa' }))

    const alert = await screen.findByRole('alert')
    expect(within(alert).getByText('O título deve ser informado.')).toBeInTheDocument()
    expect(screen.getByLabelText('Título')).toHaveValue('   ')
    expect(screen.getByLabelText('Descrição')).toHaveValue('Linha um\nLinha dois')
    expect(screen.getByLabelText('Prioridade')).toHaveValue('urgent')
    expect(screen.getByLabelText('Prazo')).toHaveValue('2000-01-02')

    await waitFor(() => expect(screen.queryByText('Tarefa criada com sucesso.')).not.toBeInTheDocument())
  })
})
