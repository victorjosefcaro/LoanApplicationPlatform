import { cn } from '@/lib/utils'

type ProgressBarProps = {
  /** 0..1 */
  value: number
  className?: string
  tone?: 'brand' | 'gold'
}

export const ProgressBar = ({ value, className, tone = 'brand' }: ProgressBarProps) => {
  const pct = Math.round(Math.min(1, Math.max(0, value)) * 100)
  return (
    <div
      className={cn('h-2 w-full overflow-hidden rounded-full bg-line', className)}
      role="progressbar"
      aria-valuenow={pct}
      aria-valuemin={0}
      aria-valuemax={100}
    >
      <div
        className={cn('h-full rounded-full transition-[width]', tone === 'gold' ? 'bg-gold' : 'bg-brand')}
        style={{ width: `${pct}%` }}
      />
    </div>
  )
}

export default ProgressBar
