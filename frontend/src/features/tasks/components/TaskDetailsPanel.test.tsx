import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { formatCalendarDate, formatInstant } from '../model/taskDates'
import type { Task } from '../model/taskTypes'
import { TaskDetailsPanel } from './TaskDetailsPanel'

const completedTask: Task = {
  id: '44444444-4444-4444-8444-444444444444',
  title: 'Preparar apresentação',
  description: 'Versão original',
  status: 'completed',
  priority: 'high',
  dueDate: '2026-10-20',
  createdAt: '2026-10-01T12:00:00Z',
  updatedAt: '2026-10-03T15:30:00Z',
  completedAt: '2026-10-04T18:45:00Z',
}

describe('TaskDetailsPanel', () => {
  it('exibe os campos editáveis e as datas automáticas como somente leitura', () => {
    render(
      <TaskDetailsPanel
        task={completedTask}
        saving={false}
        fieldErrors={{}}
        onSave={vi.fn()}
        onClose={vi.fn()}
      />,
    )

    const dialog = screen.getByRole('dialog', { name: 'Detalhes de Preparar apresentação' })
    expect(within(dialog).getByLabelText('Título')).toHaveValue('Preparar apresentação')
    expect(within(dialog).getByLabelText('Descrição')).toHaveValue('Versão original')
    expect(within(dialog).getByLabelText('Prioridade')).toHaveValue('high')
    expect(within(dialog).getByLabelText('Prazo')).toHaveValue('2026-10-20')
    expect(within(dialog).getByText('Concluída')).toBeInTheDocument()
    expect(within(dialog).getByText(formatCalendarDate('2026-10-20'))).toBeInTheDocument()
    expect(within(dialog).getByText(formatInstant(completedTask.createdAt))).toBeInTheDocument()
    expect(within(dialog).getByText(formatInstant(completedTask.updatedAt))).toBeInTheDocument()
    expect(within(dialog).getByText(formatInstant(completedTask.completedAt!))).toBeInTheDocument()
    expect(within(dialog).queryByLabelText('Data de criação')).not.toBeInTheDocument()
    expect(within(dialog).queryByLabelText('Data de conclusão')).not.toBeInTheDocument()
  })

  it('envia o conjunto completo de campos e permite remover os opcionais', async () => {
    const user = userEvent.setup()
    const onSave = vi.fn().mockResolvedValue(true)
    const onClose = vi.fn()
    render(
      <TaskDetailsPanel
        task={completedTask}
        saving={false}
        fieldErrors={{}}
        onSave={onSave}
        onClose={onClose}
      />,
    )
    const dialog = screen.getByRole('dialog')

    await user.clear(within(dialog).getByLabelText('Título'))
    await user.type(within(dialog).getByLabelText('Título'), 'Apresentação revisada')
    await user.clear(within(dialog).getByLabelText('Descrição'))
    await user.selectOptions(within(dialog).getByLabelText('Prioridade'), 'none')
    await user.clear(within(dialog).getByLabelText('Prazo'))
    await user.click(within(dialog).getByRole('button', { name: 'Salvar alterações' }))

    expect(onSave).toHaveBeenCalledWith({
      title: 'Apresentação revisada',
      description: null,
      priority: 'none',
      dueDate: null,
    })
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('preserva todos os valores editados quando a atualização é rejeitada', async () => {
    const user = userEvent.setup()
    const onSave = vi.fn().mockResolvedValue(false)
    const onClose = vi.fn()
    const view = render(
      <TaskDetailsPanel
        task={completedTask}
        saving={false}
        fieldErrors={{}}
        onSave={onSave}
        onClose={onClose}
      />,
    )
    let dialog = screen.getByRole('dialog')

    await user.clear(within(dialog).getByLabelText('Título'))
    await user.type(within(dialog).getByLabelText('Título'), '   ')
    await user.clear(within(dialog).getByLabelText('Descrição'))
    await user.type(within(dialog).getByLabelText('Descrição'), 'Rascunho ainda não salvo')
    await user.selectOptions(within(dialog).getByLabelText('Prioridade'), 'urgent')
    await user.clear(within(dialog).getByLabelText('Prazo'))
    await user.type(within(dialog).getByLabelText('Prazo'), '2031-09-08')
    await user.click(within(dialog).getByRole('button', { name: 'Salvar alterações' }))

    view.rerender(
      <TaskDetailsPanel
        task={completedTask}
        saving={false}
        fieldErrors={{ title: ['O título deve ser informado.'] }}
        onSave={onSave}
        onClose={onClose}
      />,
    )
    dialog = screen.getByRole('dialog')

    expect(within(dialog).getByRole('alert')).toHaveTextContent('O título deve ser informado.')
    expect(within(dialog).getByLabelText('Título')).toHaveValue('   ')
    expect(within(dialog).getByLabelText('Descrição')).toHaveValue('Rascunho ainda não salvo')
    expect(within(dialog).getByLabelText('Prioridade')).toHaveValue('urgent')
    expect(within(dialog).getByLabelText('Prazo')).toHaveValue('2031-09-08')
    expect(onClose).not.toHaveBeenCalled()
  })
})
