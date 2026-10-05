import { useEffect, useRef, useState } from 'react'
import type { ActiveTaskStatus, Task } from '../model/taskTypes'
import styles from './TaskDetailsPanel.module.css'

type Props = {
  task: Task
  busy: boolean
  onChangeStatus: (status: ActiveTaskStatus) => Promise<void>
  onComplete: () => Promise<void>
  onReopen: () => Promise<void>
}

const activeStatusLabels: Record<ActiveTaskStatus, string> = {
  not_started: 'Não iniciada',
  in_progress: 'Em andamento',
  blocked: 'Bloqueada',
}

export function TaskStatusActions({
  task,
  busy,
  onChangeStatus,
  onComplete,
  onReopen,
}: Props) {
  const [selectedStatus, setSelectedStatus] = useState<ActiveTaskStatus>(() => activeStatus(task))
  const previousStatus = useRef(task.status)
  const activeStatusRef = useRef<HTMLSelectElement>(null)
  const reopenRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    if (task.status !== 'completed') {
      setSelectedStatus(task.status)
    }

    if (previousStatus.current !== task.status) {
      if (task.status === 'completed') {
        reopenRef.current?.focus()
      } else {
        activeStatusRef.current?.focus()
      }
      previousStatus.current = task.status
    }
  }, [task.status])

  return (
    <section className={styles.statusActions} aria-labelledby="task-status-heading">
      <div>
        <p className={styles.eyebrow}>Acompanhamento</p>
        <h3 id="task-status-heading">Estado da tarefa</h3>
      </div>

      {task.status === 'completed' ? (
        <div className={styles.completedActions}>
          <p><strong>Estado atual:</strong> Concluída</p>
          <button
            ref={reopenRef}
            className={styles.secondaryAction}
            disabled={busy}
            type="button"
            onClick={() => void onReopen()}
          >
            Reabrir tarefa
          </button>
          <p className={styles.actionHint}>A tarefa retornará ao estado ativo anterior.</p>
        </div>
      ) : (
        <div className={styles.activeActions}>
          <p><strong>Estado atual:</strong> {activeStatusLabels[task.status]}</p>
          <div className={styles.statusField}>
            <label htmlFor="task-active-status">Estado ativo</label>
            <select
              ref={activeStatusRef}
              id="task-active-status"
              disabled={busy}
              value={selectedStatus}
              onChange={(event) => setSelectedStatus(event.target.value as ActiveTaskStatus)}
            >
              <option value="not_started">Não iniciada</option>
              <option value="in_progress">Em andamento</option>
              <option value="blocked">Bloqueada</option>
            </select>
            <button
              className={styles.secondaryAction}
              disabled={busy || selectedStatus === task.status}
              type="button"
              onClick={() => void onChangeStatus(selectedStatus)}
            >
              Salvar estado
            </button>
          </div>
          <button
            className={styles.completeAction}
            disabled={busy}
            type="button"
            onClick={() => void onComplete()}
          >
            Concluir tarefa
          </button>
          <p className={styles.actionHint}>A conclusão registra data e histórico automaticamente.</p>
        </div>
      )}
    </section>
  )
}

function activeStatus(task: Task): ActiveTaskStatus {
  return task.status === 'completed' ? 'not_started' : task.status
}
