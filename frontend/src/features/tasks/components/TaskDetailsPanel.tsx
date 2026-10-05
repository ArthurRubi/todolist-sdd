import { type FormEvent, useEffect, useRef, useState } from 'react'
import { formatCalendarDate, formatInstant } from '../model/taskDates'
import type { FieldErrors, Task, TaskPriority, UpdateTaskInput } from '../model/taskTypes'
import styles from './TaskDetailsPanel.module.css'

type Props = {
  task: Task
  saving: boolean
  fieldErrors: FieldErrors
  onSave: (input: UpdateTaskInput) => Promise<boolean>
  onClose: () => void
}

const statusLabels: Record<Task['status'], string> = {
  not_started: 'Não iniciada',
  in_progress: 'Em andamento',
  blocked: 'Bloqueada',
  completed: 'Concluída',
}

export function TaskDetailsPanel({ task, saving, fieldErrors, onSave, onClose }: Props) {
  const [form, setForm] = useState(() => formFromTask(task))
  const titleRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    setForm(formFromTask(task))
    titleRef.current?.focus()
  }, [task.id])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const saved = await onSave({
      title: form.title,
      description: form.description || null,
      priority: form.priority,
      dueDate: form.dueDate || null,
    })
    if (saved) onClose()
  }

  const allErrors = Object.values(fieldErrors).flat()

  return (
    <div className={styles.backdrop}>
      <section
        className={styles.panel}
        role="dialog"
        aria-modal="true"
        aria-labelledby="task-details-title"
      >
        <header className={styles.header}>
          <div>
            <p className={styles.eyebrow}>Detalhes da tarefa</p>
            <h2 id="task-details-title">Detalhes de {task.title}</h2>
          </div>
          <button className={styles.closeButton} type="button" onClick={onClose}>
            Fechar detalhes
          </button>
        </header>

        {allErrors.length > 0 && (
          <div className={styles.errorSummary} role="alert">
            {allErrors.map((error) => <span key={error}>{error}</span>)}
          </div>
        )}

        <form className={styles.form} onSubmit={handleSubmit} noValidate>
          <div className={styles.field}>
            <label htmlFor="details-title">Título</label>
            <input
              ref={titleRef}
              id="details-title"
              maxLength={201}
              value={form.title}
              aria-invalid={Boolean(fieldErrors.title)}
              onChange={(event) => setForm({ ...form, title: event.target.value })}
            />
            {fieldErrors.title?.map((error) => (
              <span className={styles.fieldError} key={error}>{error}</span>
            ))}
          </div>

          <div className={styles.field}>
            <label htmlFor="details-description">Descrição</label>
            <textarea
              id="details-description"
              rows={7}
              maxLength={10001}
              value={form.description}
              aria-invalid={Boolean(fieldErrors.description)}
              onChange={(event) => setForm({ ...form, description: event.target.value })}
            />
            {fieldErrors.description?.map((error) => (
              <span className={styles.fieldError} key={error}>{error}</span>
            ))}
          </div>

          <div className={styles.fieldRow}>
            <div className={styles.field}>
              <label htmlFor="details-priority">Prioridade</label>
              <select
                id="details-priority"
                value={form.priority}
                aria-invalid={Boolean(fieldErrors.priority)}
                onChange={(event) => setForm({ ...form, priority: event.target.value as TaskPriority })}
              >
                <option value="none">Sem prioridade</option>
                <option value="low">Baixa</option>
                <option value="medium">Média</option>
                <option value="high">Alta</option>
                <option value="urgent">Urgente</option>
              </select>
              {fieldErrors.priority?.map((error) => (
                <span className={styles.fieldError} key={error}>{error}</span>
              ))}
            </div>
            <div className={styles.field}>
              <label htmlFor="details-due-date">Prazo</label>
              <input
                id="details-due-date"
                type="date"
                value={form.dueDate}
                onChange={(event) => setForm({ ...form, dueDate: event.target.value })}
              />
            </div>
          </div>

          <button className={styles.saveButton} disabled={saving} type="submit">
            {saving ? 'Salvando…' : 'Salvar alterações'}
          </button>
        </form>

        <dl className={styles.readOnly} aria-label="Informações automáticas">
          <div><dt>Identificador</dt><dd>{task.id}</dd></div>
          <div><dt>Estado</dt><dd>{statusLabels[task.status]}</dd></div>
          {task.dueDate && <div><dt>Prazo atual</dt><dd>{formatCalendarDate(task.dueDate)}</dd></div>}
          <div><dt>Criada em</dt><dd>{formatInstant(task.createdAt)}</dd></div>
          <div><dt>Atualizada em</dt><dd>{formatInstant(task.updatedAt)}</dd></div>
          {task.completedAt && (
            <div><dt>Concluída em</dt><dd data-testid="completed-at">{formatInstant(task.completedAt)}</dd></div>
          )}
        </dl>
      </section>
    </div>
  )
}

function formFromTask(task: Task) {
  return {
    title: task.title,
    description: task.description ?? '',
    priority: task.priority,
    dueDate: task.dueDate ?? '',
  }
}
