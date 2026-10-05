import { useEffect, useRef, useState } from 'react'
import {
  changeTaskStatus,
  completeTask,
  createTask,
  listTasks,
  reopenTask,
  updateTask,
} from '../features/tasks/api/taskApi'
import { QuickAddTaskForm } from '../features/tasks/components/QuickAddTaskForm'
import { TaskDetailsPanel } from '../features/tasks/components/TaskDetailsPanel'
import { TaskList } from '../features/tasks/components/TaskList'
import { TaskSummary } from '../features/tasks/components/TaskSummary'
import type {
  ActiveTaskStatus,
  CreateTaskInput,
  FieldErrors,
  Task,
  UpdateTaskInput,
} from '../features/tasks/model/taskTypes'
import { TaskApiError } from '../features/tasks/model/taskTypes'
import { FeedbackRegion } from '../shared/FeedbackRegion'
import './App.css'

type Feedback = { kind: 'success' | 'error'; message: string }

export function App() {
  const [activeTasks, setActiveTasks] = useState<Task[]>([])
  const [completedTasks, setCompletedTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [createFieldErrors, setCreateFieldErrors] = useState<FieldErrors>({})
  const [editFieldErrors, setEditFieldErrors] = useState<FieldErrors>({})
  const [feedback, setFeedback] = useState<Feedback | null>(null)
  const [selectedTask, setSelectedTask] = useState<Task | null>(null)
  const detailsTriggerRef = useRef<HTMLButtonElement | null>(null)

  useEffect(() => {
    let current = true
    Promise.all([listTasks('active'), listTasks('completed')])
      .then(([active, completed]) => {
        if (current) {
          setActiveTasks(active)
          setCompletedTasks(completed)
        }
      })
      .catch((error: unknown) => {
        if (current) {
          setFeedback({ kind: 'error', message: errorMessage(error) })
        }
      })
      .finally(() => {
        if (current) setLoading(false)
      })

    return () => { current = false }
  }, [])

  async function handleCreate(input: CreateTaskInput): Promise<boolean> {
    setSaving(true)
    setFeedback(null)
    setCreateFieldErrors({})
    try {
      const created = await createTask(input)
      setActiveTasks((tasks) => [created, ...tasks])
      setFeedback({ kind: 'success', message: 'Tarefa criada com sucesso.' })
      return true
    } catch (error) {
      if (error instanceof TaskApiError) {
        setCreateFieldErrors(error.fieldErrors)
      }
      setFeedback({ kind: 'error', message: errorMessage(error) })
      return false
    } finally {
      setSaving(false)
    }
  }

  async function handleUpdate(input: UpdateTaskInput): Promise<boolean> {
    if (!selectedTask) return false
    setSaving(true)
    setFeedback(null)
    setEditFieldErrors({})
    try {
      const updated = await updateTask(selectedTask.id, input)
      const replace = (tasks: Task[]) => tasks.map((task) => task.id === updated.id ? updated : task)
      setActiveTasks(replace)
      setCompletedTasks(replace)
      setSelectedTask(updated)
      setFeedback({ kind: 'success', message: 'Tarefa atualizada com sucesso.' })
      return true
    } catch (error) {
      if (error instanceof TaskApiError) {
        setEditFieldErrors(error.fieldErrors)
      }
      setFeedback({ kind: 'error', message: errorMessage(error) })
      return false
    } finally {
      setSaving(false)
    }
  }

  async function handleStatusCommand(
    command: (taskId: string) => Promise<Task>,
    successMessage: string,
  ): Promise<void> {
    if (!selectedTask) return
    setSaving(true)
    setFeedback(null)
    try {
      const updated = await command(selectedTask.id)
      setActiveTasks((tasks) => placeTask(tasks, updated, 'active'))
      setCompletedTasks((tasks) => placeTask(tasks, updated, 'completed'))
      setSelectedTask(updated)
      setFeedback({ kind: 'success', message: successMessage })
    } catch (error) {
      setFeedback({ kind: 'error', message: errorMessage(error) })
    } finally {
      setSaving(false)
    }
  }

  async function handleChangeStatus(status: ActiveTaskStatus): Promise<void> {
    await handleStatusCommand(
      (taskId) => changeTaskStatus(taskId, status),
      'Estado da tarefa atualizado com sucesso.',
    )
  }

  function openDetails(task: Task, trigger: HTMLButtonElement) {
    detailsTriggerRef.current = trigger
    setEditFieldErrors({})
    setSelectedTask(task)
  }

  function closeDetails() {
    setSelectedTask(null)
    setEditFieldErrors({})
    requestAnimationFrame(() => detailsTriggerRef.current?.focus())
  }

  return (
    <main className="app-shell">
      <header className="app-header">
        <div>
          <p className="brand-kicker">TodoList · SDD</p>
          <h1>Seu dia, com espaço para respirar.</h1>
          <p className="intro">Capture rápido. Detalhe quando fizer sentido. Nada se perde.</p>
        </div>
        <TaskSummary activeCount={activeTasks.length} completedCount={completedTasks.length} />
      </header>

      <QuickAddTaskForm busy={saving} fieldErrors={createFieldErrors} onSubmit={handleCreate} />
      {feedback && <FeedbackRegion kind={feedback.kind} message={feedback.message} />}

      {loading ? (
        <p className="loading" role="status">Carregando suas tarefas…</p>
      ) : (
        <div className="task-columns">
          <TaskList title="Tarefas ativas" emptyMessage="Nenhuma tarefa ativa por aqui." tasks={activeTasks} onOpenTask={openDetails} />
          <TaskList title="Concluídas" emptyMessage="As tarefas concluídas aparecerão aqui." tasks={completedTasks} onOpenTask={openDetails} />
        </div>
      )}

      {selectedTask && (
        <TaskDetailsPanel
          task={selectedTask}
          saving={saving}
          fieldErrors={editFieldErrors}
          onSave={handleUpdate}
          onClose={closeDetails}
          onChangeStatus={handleChangeStatus}
          onComplete={() => handleStatusCommand(completeTask, 'Tarefa concluída com sucesso.')}
          onReopen={() => handleStatusCommand(reopenTask, 'Tarefa reaberta com sucesso.')}
        />
      )}
    </main>
  )
}

function placeTask(tasks: Task[], updated: Task, view: 'active' | 'completed'): Task[] {
  const withoutUpdated = tasks.filter((task) => task.id !== updated.id)
  const belongsToView = view === 'completed'
    ? updated.status === 'completed'
    : updated.status !== 'completed'
  return belongsToView ? [updated, ...withoutUpdated] : withoutUpdated
}

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'A operação não pôde ser concluída.'
}
