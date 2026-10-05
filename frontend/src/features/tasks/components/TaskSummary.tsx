import styles from './Tasks.module.css'

type Props = {
  activeCount: number
  completedCount: number
}

export function TaskSummary({ activeCount, completedCount }: Props) {
  return (
    <div className={styles.summary} aria-label="Resumo das tarefas">
      <span>{activeCount} {activeCount === 1 ? 'ativa' : 'ativas'}</span>
      <span aria-hidden="true">·</span>
      <span>{completedCount} concluídas</span>
    </div>
  )
}
