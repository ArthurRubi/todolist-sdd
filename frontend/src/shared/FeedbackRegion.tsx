import styles from './FeedbackRegion.module.css'

type Props = {
  kind: 'success' | 'error'
  message: string
}

export function FeedbackRegion({ kind, message }: Props) {
  return (
    <div
      className={`${styles.feedback} ${kind === 'error' ? styles.error : styles.success}`}
      role={kind === 'error' ? 'alert' : 'status'}
      aria-live={kind === 'error' ? 'assertive' : 'polite'}
    >
      {message}
    </div>
  )
}
