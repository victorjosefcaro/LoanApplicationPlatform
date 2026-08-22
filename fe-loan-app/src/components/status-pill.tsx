import { cn } from '@/lib/utils'
import { loanStatusMeta, paymentStatusMeta, type Tone } from '@/constants'

const TONE_CLASS: Record<Tone, string> = {
  info: 'bg-brand-tint text-brand-deep',
  progress: 'bg-gold-tint text-ink',
  success: 'bg-brand text-white',
  warning: 'bg-gold-tint text-ink ring-1 ring-gold/50',
  danger: 'bg-coral/12 text-coral',
  muted: 'bg-line text-brand',
}

type PillProps = {
  label: string
  tone: Tone
  className?: string
}

const Pill = ({ label, tone, className }: PillProps) => (
  <span
    className={cn(
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap',
      TONE_CLASS[tone],
      className,
    )}
  >
    {label}
  </span>
)

export const LoanStatusPill = ({ status, className }: { status: string; className?: string }) => {
  const meta = loanStatusMeta(status)
  return <Pill label={meta.label} tone={meta.tone} className={className} />
}

export const PaymentStatusPill = ({
  status,
  className,
}: {
  status: string
  className?: string
}) => {
  const meta = paymentStatusMeta(status)
  return <Pill label={meta.label} tone={meta.tone} className={className} />
}

export default Pill
