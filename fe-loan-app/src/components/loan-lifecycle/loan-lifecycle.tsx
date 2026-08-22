import { Fragment } from 'react'
import { FiCheck, FiSlash, FiX } from 'react-icons/fi'
import { cn } from '@/lib/utils'
import { lifecycleView, type LifecycleStage, type NodeState } from './lifecycle'

const NODE_CLASS: Record<NodeState, string> = {
  done: 'bg-brand text-white border-brand',
  current: 'bg-surface text-brand-deep border-brand ring-4 ring-gold/60',
  upcoming: 'bg-surface text-black border-line',
}

const LABEL_CLASS: Record<NodeState, string> = {
  done: 'text-ink',
  current: 'text-ink font-semibold',
  upcoming: 'text-black',
}

const Node = ({ stage, index }: { stage: LifecycleStage; index: number }) => (
  <li className="flex min-w-0 flex-1 flex-col items-center gap-2 text-center">
    <span
      className={cn(
        'grid size-9 shrink-0 place-items-center rounded-full border-2 text-sm font-semibold transition-colors',
        NODE_CLASS[stage.state],
      )}
      aria-hidden="true"
    >
      {stage.state === 'done' ? <FiCheck className="size-4" /> : index + 1}
    </span>
    <span className={cn('text-xs leading-tight sm:text-sm', LABEL_CLASS[stage.state])}>
      {stage.label}
    </span>
  </li>
)

const Connector = ({ filled }: { filled: boolean }) => (
  <li
    aria-hidden="true"
    className={cn('mt-4 h-0.5 min-w-4 flex-1 rounded-full', filled ? 'bg-brand' : 'bg-line')}
  />
)

export type LoanLifecycleProps = {
  status: string
  className?: string
}

export const LoanLifecycle = ({ status, className }: LoanLifecycleProps) => {
  const { stages, terminal, note } = lifecycleView(status)

  return (
    <div className={cn('space-y-3', className)}>
      <ol
        className={cn('flex items-start overflow-x-auto py-2', terminal && 'opacity-50')}
        aria-label={`Loan status: ${status}`}
      >
        {stages.map((stage, index) => (
          <Fragment key={stage.label}>
            {index > 0 && <Connector filled={stages[index - 1].state === 'done'} />}
            <Node stage={stage} index={index} />
          </Fragment>
        ))}
      </ol>

      {terminal && (
        <div
          className={cn(
            'flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium',
            terminal.kind === 'rejected' ? 'bg-coral/10 text-coral' : 'bg-line text-black',
          )}
        >
          {terminal.kind === 'rejected' ? (
            <FiX className="size-4 shrink-0" />
          ) : (
            <FiSlash className="size-4 shrink-0" />
          )}
          {terminal.label}
        </div>
      )}

      {note && (
        <div className="rounded-lg bg-gold-tint px-3 py-2 text-sm font-medium text-ink">{note}</div>
      )}
    </div>
  )
}

export default LoanLifecycle
