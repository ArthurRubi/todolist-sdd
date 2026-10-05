import type { components } from '../api/schema'

export type Task = components['schemas']['Task']
export type TaskPriority = components['schemas']['TaskPriority']
export type ActiveTaskStatus = components['schemas']['ActiveTaskStatus']
export type UpdateTaskInput = components['schemas']['UpdateTaskRequest']

export type CreateTaskInput = {
  title: string
  description: string | null
  priority: TaskPriority
  dueDate: string | null
}

export type FieldErrors = Record<string, string[]>

export class TaskApiError extends Error {
  constructor(
    message: string,
    public readonly fieldErrors: FieldErrors = {},
    public readonly traceId?: string,
  ) {
    super(message)
    this.name = 'TaskApiError'
  }
}
