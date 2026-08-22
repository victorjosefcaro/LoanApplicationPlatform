import { cn } from '@/lib/utils'

const PHP = new Intl.NumberFormat('en-PH', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const PHP_WHOLE = new Intl.NumberFormat('en-PH', {
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
})

export type MoneyProps = {
  /** Amount in PHP; null/undefined renders an em dash. */
  amount: number | null | undefined
  /** Hide centavos when the value is whole. */
  compact?: boolean
  /** Leading + for positive values. */
  signed?: boolean
  className?: string
}

// Peso figure in tabular-nums mono so money columns align.
export const Money = ({ amount, compact = false, signed = false, className }: MoneyProps) => {
  const isMissing = amount === null || amount === undefined || Number.isNaN(amount)

  let body: string
  if (isMissing) {
    body = '—'
  } else {
    const useWhole = compact && Number.isInteger(amount)
    const formatted = (useWhole ? PHP_WHOLE : PHP).format(Math.abs(amount))
    const sign = amount < 0 ? '−' : signed ? '+' : ''
    body = `${sign}₱${formatted}`
  }

  return (
    <span className={cn('font-mono tabular-nums whitespace-nowrap', className)} data-slot="money">
      {body}
    </span>
  )
}

export default Money
