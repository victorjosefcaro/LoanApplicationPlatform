import type { LoanApplicationStatusHistory } from '@/api/loan-applications/loan-applications.types'
import { formatDateTime } from '@/utils/format'
import { LoanStatusPill } from '@/components/status-pill'
import { EmptyState } from '@/components/states'

export const HistoryTimeline = ({ items }: { items: LoanApplicationStatusHistory[] }) => {
  if (items.length === 0) {
    return (
      <EmptyState
        title="No history yet"
        message="Status changes will appear here as the application moves along."
      />
    )
  }

  const ordered = [...items].sort(
    (a, b) => new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime(),
  )

  return (
    <ol className="space-y-4">
      {ordered.map((entry, index) => (
        <li key={entry.id} className="flex gap-3">
          <div className="flex flex-col items-center">
            <span
              className={`mt-1 size-2.5 rounded-full ${index === 0 ? 'bg-brand' : 'bg-line'}`}
              aria-hidden="true"
            />
            {index < ordered.length - 1 && <span className="w-px flex-1 bg-line" aria-hidden="true" />}
          </div>
          <div className="flex-1 pb-1">
            <div className="flex flex-wrap items-center gap-2">
              <LoanStatusPill status={entry.newStatus} />
              <span className="text-xs text-muted">{formatDateTime(entry.changedAt)}</span>
            </div>
            {entry.remarks && <p className="mt-1 text-sm text-ink">{entry.remarks}</p>}
            {entry.changedByUsername && (
              <p className="mt-0.5 text-xs text-muted">by {entry.changedByUsername}</p>
            )}
          </div>
        </li>
      ))}
    </ol>
  )
}

export default HistoryTimeline
