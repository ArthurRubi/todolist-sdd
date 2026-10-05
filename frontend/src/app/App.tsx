import { useEffect, useState } from 'react'
import { createTask, listTasks } from '../features/tasks/api/taskApi'
import { QuickAddTaskForm } from '../features/tasks/components/QuickAddTaskForm'
import { TaskList } from '../features/tasks/components/TaskList'
import { TaskSummary } from '../features/tasks/components/TaskSummary'
import type { CreateTaskInput, FieldErrors, Task } from '../features/tasks/model/taskTypes'
import { TaskApiError } from '../features/tasks/model/taskTypes'
import { FeedbackRegion } from '../shared/FeedbackRegion'
import './App.css'

type Feedback = { kind: 'success' | 'error'; message: string }

export function App() {
  const [activeTasks, setActiveTasks] = useState<Task[]>([])
  const [completedTasks, setCompletedTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [feedback, setFeedback] = useState<Feedback | null>(null)

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
    setFieldErrors({})
    try {
      const created = await createTask(input)
      setActiveTasks((tasks) => [created, ...tasks])
      setFeedback({ kind: 'success', message: 'Tarefa criada com sucesso.' })
      return true
    } catch (error) {
      if (error instanceof TaskApiError) {
        setFieldErrors(error.fieldErrors)
      }
      setFeedback({ kind: 'error', message: errorMessage(error) })
      return false
    } finally {
      setSaving(false)
    }
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

      <QuickAddTaskForm busy={saving} fieldErrors={fieldErrors} onSubmit={handleCreate} />
      {feedback && <FeedbackRegion kind={feedback.kind} message={feedback.message} />}

      {loading ? (
        <p className="loading" role="status">Carregando suas tarefas…</p>
      ) : (
        <div className="task-columns">
          <TaskList title="Tarefas ativas" emptyMessage="Nenhuma tarefa ativa por aqui." tasks={activeTasks} />
          <TaskList title="Concluídas" emptyMessage="As tarefas concluídas aparecerão aqui." tasks={completedTasks} />
        </div>
      )}
    </main>
  )
}

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'A operação não pôde ser concluída.'
}
