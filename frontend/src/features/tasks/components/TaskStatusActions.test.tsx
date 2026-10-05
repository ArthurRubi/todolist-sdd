import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { Task } from '../model/taskTypes'
import { TaskStatusActions } from './TaskStatusActions'

const activeTask: Task = {
  id: '55555555-5555-4555-8555-555555555555',
  title: 'Acompanhar entrega',
  description: null,
  status: 'not_started',
  priority: 'none',
  dueDate: null,
  createdAt: '2026-10-05T10:00:00Z',
  updatedAt: '2026-10-05T10:00:00Z',
  completedAt: null,
}

describe('TaskStatusActions', () => {
  it('altera um estado ativo e oferece conclusão com nomes acessíveis', async () => {
    const user = userEvent.setup()
    const onChangeStatus = vi.fn().mockResolvedValue(undefined)
    const onComplete = vi.fn().mockResolvedValue(undefined)
    render(
      <TaskStatusActions
        task={activeTask}
        busy={false}
        onChangeStatus={onChangeStatus}
        onComplete={onComplete}
        onReopen={vi.fn()}
      />,
    )

    await user.selectOptions(screen.getByLabelText('Estado ativo'), 'blocked')
    await user.click(screen.getByRole('button', { name: 'Salvar estado' }))
    await user.click(screen.getByRole('button', { name: 'Concluir tarefa' }))

    expect(onChangeStatus).toHaveBeenCalledWith('blocked')
    expect(onComplete).toHaveBeenCalledOnce()
    expect(screen.getByText('A conclusão registra data e histórico automaticamente.')).toBeInTheDocument()
  })

  it('oferece somente reabertura para tarefa concluída', async () => {
    const user = userEvent.setup()
    const onReopen = vi.fn().mockResolvedValue(undefined)
    render(
      <TaskStatusActions
        task={{ ...activeTask, status: 'completed', completedAt: '2026-10-05T11:00:00Z' }}
        busy={false}
        onChangeStatus={vi.fn()}
        onComplete={vi.fn()}
        onReopen={onReopen}
      />,
    )

    expect(screen.queryByLabelText('Estado ativo')).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Reabrir tarefa' }))
    expect(onReopen).toHaveBeenCalledOnce()
  })

  it('move o foco previsivelmente quando conclusão e reabertura mudam os controles', () => {
    const props = {
      busy: false,
      onChangeStatus: vi.fn(),
      onComplete: vi.fn(),
      onReopen: vi.fn(),
    }
    const view = render(<TaskStatusActions task={activeTask} {...props} />)

    view.rerender(
      <TaskStatusActions
        task={{ ...activeTask, status: 'completed', completedAt: '2026-10-05T11:00:00Z' }}
        {...props}
      />,
    )
    expect(screen.getByRole('button', { name: 'Reabrir tarefa' })).toHaveFocus()

    view.rerender(<TaskStatusActions task={{ ...activeTask, status: 'blocked' }} {...props} />)
    expect(screen.getByLabelText('Estado ativo')).toHaveFocus()
  })
})
