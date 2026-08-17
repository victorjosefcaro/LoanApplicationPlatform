import { Link, useNavigate } from 'react-router-dom'
import { FiChevronRight, FiPlusCircle } from 'react-icons/fi'
import { useLoanApplications } from '@/api/loan-applications/loan-applications.queries'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import { formatDate } from '@/utils/format'
import PageHeader from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { Money } from '@/components/money/money'
import { LoanStatusPill } from '@/components/status-pill'
import { LoadingState, ErrorState, EmptyState } from '@/components/states'

const LoanRow = ({ loan }: { loan: LoanApplication }) => (
  <Link
    to={`/loans/${loan.id}`}
    className="flex items-center gap-4 rounded-xl border border-line bg-surface px-4 py-4 transition-colors hover:border-brand/40 hover:bg-brand-tint/30 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand"
  >
    <div className="min-w-0 flex-1">
      <div className="flex items-center gap-2">
        <span className="truncate font-medium text-ink">{loan.purpose || 'Loan application'}</span>
        <LoanStatusPill status={loan.status} />
      </div>
      <p className="mt-0.5 text-sm text-muted">
        {loan.termInMonths} months · applied {formatDate(loan.createdAt)}
      </p>
    </div>
    <div className="text-right">
      <p className="font-heading text-lg font-bold text-ink">
        <Money amount={loan.amount} compact />
      </p>
    </div>
    <FiChevronRight className="size-5 shrink-0 text-muted" />
  </Link>
)

export const LoansPage = () => {
  const navigate = useNavigate()
  const loansQuery = useLoanApplications({ pageSize: 50 })

  return (
    <div>
      <PageHeader
        title="My loans"
        subtitle="Every application you've made with Loanly."
        actions={
          <Button onClick={() => navigate('/loans/new')}>
            <FiPlusCircle className="size-4" />
            Apply for a loan
          </Button>
        }
      />

      {loansQuery.isLoading && <LoadingState label="Loading your loans…" />}
      {loansQuery.isError && <ErrorState error={loansQuery.error} onRetry={loansQuery.refetch} />}

      {loansQuery.data &&
        (loansQuery.data.items.length === 0 ? (
          <EmptyState
            title="No loans yet"
            message="When you're ready, apply for your first loan — it only takes a minute."
            
          />
        ) : (
          <div className="space-y-3">
            {loansQuery.data.items.map((loan) => (
              <LoanRow key={loan.id} loan={loan} />
            ))}
          </div>
        ))}
    </div>
  )
}

export default LoansPage
