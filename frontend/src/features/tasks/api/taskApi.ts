import type { CreateTaskInput, Task } from '../model/taskTypes'
import { TaskApiError } from '../model/taskTypes'

const apiBaseUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api').replace(/\/$/, '')

type ProblemDetails = {
  title?: string
  detail?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export async function listTasks(view: 'active' | 'completed'): Promise<Task[]> {
  return request<Task[]>(`/tasks?view=${view}`)
}

export async function createTask(input: CreateTaskInput): Promise<Task> {
  return request<Task>('/tasks', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': crypto.randomUUID(),
    },
    body: JSON.stringify(input),
  })
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${apiBaseUrl}${path}`, init)
  } catch {
    throw new TaskApiError('Não foi possível acessar o servidor. Tente novamente.')
  }

  if (!response.ok) {
    let problem: ProblemDetails = {}
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // A mensagem padrão continua útil quando a resposta não possui JSON válido.
    }

    const firstFieldMessage = Object.values(problem.errors ?? {})[0]?.[0]
    throw new TaskApiError(
      firstFieldMessage ?? problem.detail ?? problem.title ?? 'A operação não pôde ser concluída.',
      problem.errors,
      problem.traceId,
    )
  }

  return (await response.json()) as T
}
