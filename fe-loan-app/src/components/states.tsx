import { type ReactNode } from 'react'
import { FiAlertTriangle, FiInbox } from 'react-icons/fi'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { getErrorMessage } from '@/utils/errors'

/** Emphasise an HTTP status code (e.g. "502 Bad Gateway") within an error message. */
const highlightStatus = (message: string): ReactNode => {
  const match = message.match(/\b([1-5]\d{2}(?:\s+[A-Za-z][A-Za-z]*)*)\b/)
  if (!match || match.index === undefined) return message

  const start = match.index
  const end = start + match[0].length
  return (
    <>
      {message.slice(0, start)}
      <span className="font-semibold text-coral decoration-coral/60 underline-offset-2">
        {match[0]}
      </span>
      {message.slice(end)}
    </>
  )
}

export const Spinner = ({ className }: { className?: string }) => (
  <span
    role="status"
    aria-label="Loading"
    className={cn(
      'inline-block size-5 animate-spin rounded-full border-2 border-current border-t-transparent',
      className,
    )}
  />
)

export const LoadingState = ({ label = 'Loading…' }: { label?: string }) => (
  <div className="flex flex-col items-center justify-center gap-3 py-16 text-brand">
    <Spinner className="text-brand" />
    <p className="text-sm">{label}</p>
  </div>
)

type EmptyStateProps = {
  title: string
  message?: string
  icon?: ReactNode
  action?: ReactNode
  className?: string
}

/** Empty states invite an action — never a dead end. */
export const EmptyState = ({ title, message, icon, action, className }: EmptyStateProps) => (
  <div
    className={cn(
      'flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-line bg-surface px-6 py-14 text-center',
      className,
    )}
  >
    <div className="grid size-12 place-items-center rounded-full bg-brand-tint text-brand-deep">
      {icon ?? <FiInbox className="size-6" />}
    </div>
    <div className="space-y-1 text-black">
      <h3 className="font-heading text-lg font-semibold">{title}</h3>
      {message && <p className="mx-auto max-w-sm">{message}</p>}
    </div>
    {action}
  </div>
)

type ErrorStateProps = {
  error: unknown
  onRetry?: () => void
  title?: string
  className?: string
}

/** Errors say what happened and how to fix it, and never just apologise. */
export const ErrorState = ({
  error,
  onRetry,
  title = 'That didn’t load',
  className,
}: ErrorStateProps) => (
  <div
    className={cn(
      'flex flex-col items-center justify-center gap-3 rounded-xl border border-coral/30 bg-coral/5 px-6 py-14 text-center',
      className,
    )}
  >
    <div className="grid size-12 place-items-center rounded-full bg-coral/12 text-coral">
      <FiAlertTriangle className="size-6" />
    </div>
    <div className="space-y-1 text-black">
      <h3 className="font-heading text-lg font-semibold">{title}</h3>
      <p className="mx-auto max-w-sm text-sm ">{highlightStatus(getErrorMessage(error))}</p>
    </div>
    {onRetry && (
      <Button variant="outline" onClick={onRetry}>
        Try again
      </Button>
    )}
  </div>
)

/** Inline error text for forms/mutations. */
export const InlineError = ({ error }: { error: unknown }) => (
  <p className="text-sm font-medium text-coral" role="alert">
    {getErrorMessage(error)}
  </p>
)
