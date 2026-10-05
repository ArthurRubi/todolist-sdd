import { formatCalendarDate, formatInstant, isTaskOverdue } from '../model/taskDates'
import type { Task } from '../model/taskTypes'
import styles from './Tasks.module.css'

type Props = {
  title: string
  emptyMessage: string
  tasks: Task[]
}

const statusLabels: Record<Task['status'], string> = {
  not_started: 'Não iniciada',
  in_progress: 'Em andamento',
  blocked: 'Bloqueada',
  completed: 'Concluída',
}

const priorityLabels: Record<Task['priority'], string> = {
  none: 'Sem prioridade',
  low: 'Prioridade baixa',
  medium: 'Prioridade média',
  high: 'Prioridade alta',
  urgent: 'Prioridade urgente',
}

export function TaskList({ title, emptyMessage, tasks }: Props) {
  return (
    <section className={styles.listSection} aria-labelledby={`list-${toId(title)}`}>
      <div className={styles.listHeading}>
        <h2 id={`list-${toId(title)}`}>{title}</h2>
        <span>{tasks.length}</span>
      </div>
      {tasks.length === 0 ? (
        <p className={styles.empty}>{emptyMessage}</p>
      ) : (
        <div className={styles.list}>
          {tasks.map((task) => (
            <article className={styles.card} key={task.id}>
              <div className={styles.cardTop}>
                <div>
                  <p className={styles.status}>{statusLabels[task.status]}</p>
                  <h3>{task.title}</h3>
                </div>
                {isTaskOverdue(task) && <span className={styles.overdue}>Atrasada</span>}
              </div>
              {task.description && <p className={styles.description}>{task.description}</p>}
              <div className={styles.metadata}>
                <span>{priorityLabels[task.priority]}</span>
                {task.dueDate && <span>Prazo: {formatCalendarDate(task.dueDate)}</span>}
                <span>Criada em {formatInstant(task.createdAt)}</span>
                <span>Atualizada em {formatInstant(task.updatedAt)}</span>
                {task.completedAt && <span>Concluída em {formatInstant(task.completedAt)}</span>}
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}

function toId(value: string) {
  return value.toLocaleLowerCase().replace(/\s+/g, '-')
}
