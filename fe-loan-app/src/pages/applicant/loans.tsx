import { FiChevronRight, FiPlusCircle } from 'react-icons/fi'
import { useLoanApplications } from '@/api/loan-applications/loan-applications.queries'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import { formatDate } from '@/utils/format'
import PageHeader from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { Money } from '@/components/money/money'
import { LoanStatusPill } from '@/components/status-pill'
import { LoadingState, ErrorState, EmptyState } from '@/components/states'
import { useState } from 'react'
import { ApplyLoanModal } from '@/components/loan/apply-loan-modal'
import { LoanApplicationModal } from '@/components/loan/loan-application-modal'

const LoanRow = ({ loan, onOpen }: { loan: LoanApplication; onOpen: (id: number) => void }) => (
  <button
    type="button"
    onClick={() => onOpen(loan.id)}
    className="flex w-full items-center gap-4 rounded-xl border border-line bg-surface px-4 py-4 text-left transition-colors hover:border-brand/40 hover:bg-brand-tint/30 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand"
  >
    <div className="min-w-0 flex-1">
      <div className="flex items-center gap-2">
        <span className="truncate font-bold">{loan.purpose || 'Loan application'}</span>
        <LoanStatusPill status={loan.status} />
      </div>
      <p className="mt-0.5 text-sm text-black">
        {loan.termInMonths} months · applied {formatDate(loan.createdAt)}
      </p>
    </div>
    <div className="text-right">
      <p className="font-heading text-lg font-bold">
        <Money amount={loan.amount} compact />
      </p>
    </div>
    <FiChevronRight className="size-5 shrink-0" />
  </button>
)

export const LoansPage = () => {
  const loansQuery = useLoanApplications({ pageSize: 50 })
  const [openLoan, setOpenLoan] = useState<boolean>(false)
  const [selectedLoanId, setSelectedLoanId] = useState<number | null>(null)

  return (
    <>
      <ApplyLoanModal open={openLoan} onOpenChange={setOpenLoan} />
      {selectedLoanId !== null && (
        <LoanApplicationModal
          loanApplicationId={selectedLoanId}
          open={selectedLoanId !== null}
          onOpenChange={(next) => !next && setSelectedLoanId(null)}
        />
      )}
      <PageHeader
        title="My loans"
        subtitle="Every application you've made with Loanly."
        actions={
          <Button onClick={() => setOpenLoan(true)}>
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
              <LoanRow key={loan.id} loan={loan} onOpen={setSelectedLoanId} />
            ))}
          </div>
        ))}
    </>
  )
}

export default LoansPage
