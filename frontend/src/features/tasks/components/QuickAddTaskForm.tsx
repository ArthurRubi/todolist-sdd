import { type FormEvent, useState } from 'react'
import type { CreateTaskInput, FieldErrors, TaskPriority } from '../model/taskTypes'
import styles from './QuickAddTaskForm.module.css'

type Props = {
  busy: boolean
  fieldErrors: FieldErrors
  onSubmit: (input: CreateTaskInput) => Promise<boolean>
}

const initialForm = {
  title: '',
  description: '',
  priority: 'none' as TaskPriority,
  dueDate: '',
}

export function QuickAddTaskForm({ busy, fieldErrors, onSubmit }: Props) {
  const [form, setForm] = useState(initialForm)
  const [detailsOpen, setDetailsOpen] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const saved = await onSubmit({
      title: form.title,
      description: form.description || null,
      priority: form.priority,
      dueDate: form.dueDate || null,
    })

    if (saved) {
      setForm(initialForm)
      setDetailsOpen(false)
    }
  }

  return (
    <section className={styles.shell} aria-labelledby="quick-add-heading">
      <div className={styles.headingRow}>
        <div>
          <p className={styles.eyebrow}>Entrada rápida</p>
          <h2 id="quick-add-heading">O que você quer tirar da cabeça?</h2>
        </div>
        <span className={styles.shortcut}>Enter para adicionar</span>
      </div>

      <form onSubmit={handleSubmit} noValidate>
        <div className={styles.titleRow}>
          <div className={styles.field}>
            <label htmlFor="task-title">Título</label>
            <input
              id="task-title"
              autoFocus
              maxLength={201}
              placeholder="Ex.: agendar consulta"
              value={form.title}
              aria-invalid={Boolean(fieldErrors.title)}
              aria-describedby={fieldErrors.title ? 'task-title-error' : undefined}
              onChange={(event) => setForm({ ...form, title: event.target.value })}
            />
            {fieldErrors.title?.map((error) => (
              <span className={styles.fieldError} id="task-title-error" key={error}>
                {error}
              </span>
            ))}
          </div>
          <button className={styles.primaryButton} disabled={busy} type="submit">
            {busy ? 'Adicionando…' : 'Adicionar tarefa'}
          </button>
        </div>

        <button
          className={styles.detailsToggle}
          type="button"
          aria-expanded={detailsOpen}
          onClick={() => setDetailsOpen((open) => !open)}
        >
          {detailsOpen ? 'Ocultar detalhes' : 'Adicionar detalhes'}
        </button>

        {detailsOpen && (
          <div className={styles.detailsGrid}>
            <div className={`${styles.field} ${styles.description}`}>
              <label htmlFor="task-description">Descrição</label>
              <textarea
                id="task-description"
                rows={4}
                maxLength={10001}
                placeholder="Contexto, observações ou próximos passos…"
                value={form.description}
                aria-invalid={Boolean(fieldErrors.description)}
                onChange={(event) => setForm({ ...form, description: event.target.value })}
              />
              {fieldErrors.description?.map((error) => (
                <span className={styles.fieldError} key={error}>{error}</span>
              ))}
            </div>
            <div className={styles.field}>
              <label htmlFor="task-priority">Prioridade</label>
              <select
                id="task-priority"
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
              <label htmlFor="task-due-date">Prazo</label>
              <input
                id="task-due-date"
                type="date"
                value={form.dueDate}
                onChange={(event) => setForm({ ...form, dueDate: event.target.value })}
              />
            </div>
          </div>
        )}
      </form>
    </section>
  )
}
