import type { Task } from './taskTypes'

export function isTaskOverdue(task: Task, now = new Date()): boolean {
  if (!task.dueDate || task.status === 'completed') {
    return false
  }

  return task.dueDate < toLocalCalendarDate(now)
}

export function formatCalendarDate(value: string, locale?: string): string {
  const [year, month, day] = value.split('-').map(Number)
  return new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeZone: 'UTC' }).format(
    new Date(Date.UTC(year, month - 1, day, 12)),
  )
}

export function formatInstant(value: string, locale?: string, timeZone?: string): string {
  return new Intl.DateTimeFormat(locale, {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone,
  }).format(new Date(value))
}

function toLocalCalendarDate(value: Date): string {
  const year = value.getFullYear()
  const month = String(value.getMonth() + 1).padStart(2, '0')
  const day = String(value.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}
