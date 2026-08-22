import { type ReactNode } from 'react'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import { formatDate } from '@/utils/format'
import { estimateMonthlyPayment } from '@/utils/finance'
import { Money } from '@/components/money/money'

const Fact = ({ label, children }: { label: string; children: ReactNode }) => (
  <div>
    <dt className="text-xs font-medium tracking-wide text-brand uppercase">{label}</dt>
    <dd className="mt-1 text-black">{children}</dd>
  </div>
)

export const LoanSummary = ({ loan }: { loan: LoanApplication }) => (
  <div className="space-y-4">
    <dl className="grid grid-cols-2 gap-x-6 gap-y-5 sm:grid-cols-3">
      <Fact label="Amount">
        <span className="font-heading text-xl font-bold">
          <Money amount={loan.amount} />
        </span>
      </Fact>
      <Fact label="Term">{loan.termInMonths} months</Fact>
      <Fact label="Est. monthly">
        <Money amount={estimateMonthlyPayment(loan.amount, loan.termInMonths)} />
      </Fact>
      <Fact label="Monthly income">
        <Money amount={loan.monthlyIncome} />
      </Fact>
      <Fact label="Purpose">{loan.purpose || '—'}</Fact>
      <Fact label="Applied">{formatDate(loan.createdAt)}</Fact>
    </dl>

    {loan.remarks && (
      <div className="rounded-lg bg-brand-tint/50 px-4 py-3 text-sm text-brand-deep">
        <span className="font-semibold">Note from Loanly: </span>
        {loan.remarks}
      </div>
    )}
  </div>
)

export default LoanSummary
