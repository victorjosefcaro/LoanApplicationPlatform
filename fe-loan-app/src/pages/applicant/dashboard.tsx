import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { FiArrowRight, FiCreditCard, FiFileText, FiPlusCircle } from 'react-icons/fi'
import {
  useLoanApplications,
  useLoanApplicationHistory,
} from '@/api/loan-applications/loan-applications.queries'
import { usePaymentSchedules } from '@/api/payments/payments.queries'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import type { PaymentSchedule } from '@/api/payments/payments.types'
import { formatDate } from '@/utils/format'
import { repaymentProgress } from '@/utils/finance'
import PageHeader from '@/components/page-header'
import { Card } from '@/components/shared'
import { Button } from '@/components/ui/button'
import { Money } from '@/components/money/money'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanStatusPill } from '@/components/status-pill'
import { ProgressBar } from '@/components/progress-bar'
import { Pager } from '@/components/pager'
import { ScheduleTable } from '@/components/payments/schedule-table'
import { LoanSummary } from '@/components/loan/loan-summary'
import { HistoryTimeline } from '@/components/loan/history-timeline'
import { ApplyLoanModal } from '@/components/loan/apply-loan-modal'
import { LoanApplicationModal } from '@/components/loan/loan-application-modal'
import { PaymentScheduleModal } from '@/components/payments/payment-schedule-modal'
import { LoadingState, ErrorState, EmptyState } from '@/components/states'

const ACTIVE = new Set(['Released', 'Completed'])

const nextUnpaid = (schedules: PaymentSchedule[]): PaymentSchedule | null => {
  const pending = schedules
    .filter((s) => s.status !== 'Paid')
    .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime())
  return pending[0] ?? null
}

const QuickActions = ({ onApply, onGo }: { onApply: () => void; onGo: (path: string) => void }) => (
  <Card
    title="Quick actions"
    contentClassName="grid gap-2"
    content={
      <>
        <Button variant="outline" className="justify-start" onClick={onApply}>
          <FiPlusCircle className="size-4" /> Apply for a loan
        </Button>
        <Button variant="outline" className="justify-start" onClick={() => onGo('/loans')}>
          <FiFileText className="size-4" /> View my loans
        </Button>
      </>
    }
  />
)

export const DashboardPage = () => {
  const navigate = useNavigate()
  const loansQuery = useLoanApplications({ pageSize: 50 })
  const [applyOpen, setApplyOpen] = useState(false)
  const [payOpen, setPayOpen] = useState(false)
  const [detailOpen, setDetailOpen] = useState(false)
  const [current, setCurrent] = useState(0)

  const loans = [...(loansQuery.data?.items ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )

  // Clamp the carousel index in case the loan list shrank between renders.
  const index = Math.min(current, Math.max(0, loans.length - 1))
  const selected: LoanApplication | undefined = loans[index]
  const isActive = selected ? ACTIVE.has(selected.status) : false

  const schedulesQuery = usePaymentSchedules(selected?.id ?? Number.NaN, { enabled: isActive })
  const schedules = isActive ? (schedulesQuery.data ?? []) : []

  // Before funds are released there is no repayment to show, so surface the
  // application's details and status history instead.
  const historyQuery = useLoanApplicationHistory(selected?.id ?? Number.NaN, {
    enabled: !!selected && !isActive,
  })

  const totalDue = schedules.reduce((sum, s) => sum + s.amountDue, 0)
  const totalPaid = schedules.reduce((sum, s) => sum + s.amountPaid, 0)
  const next = nextUnpaid(schedules)

  if (loansQuery.isLoading) return <LoadingState label="Loading your dashboard…" />
  if (loansQuery.isError) {
    return <ErrorState error={loansQuery.error} onRetry={loansQuery.refetch} />
  }

  const nothingYet = loans.length === 0

  // Shared reference stamped on every loan-specific card so it's unambiguous that all
  // sections describe the same currently-selected loan, not separate records.
  const loanRef = selected ? `Application #${selected.id} · ${selected.purpose}` : ''

  return (
    <>
      <ApplyLoanModal open={applyOpen} onOpenChange={setApplyOpen} />
      {selected && (
        <LoanApplicationModal
          loanApplicationId={selected.id}
          open={detailOpen}
          onOpenChange={setDetailOpen}
        />
      )}
      {selected && isActive && (
        <PaymentScheduleModal
          loanApplicationId={selected.id}
          purpose={selected.purpose}
          open={payOpen}
          onOpenChange={setPayOpen}
        />
      )}

      <PageHeader title="Overview" subtitle="Here's where your loans stand." />

      {nothingYet || !selected ? (
        <EmptyState
          title="Welcome to Loanly"
          message="You don't have any loans yet. Apply for your first one — it only takes a minute."
          action={
            <Button onClick={() => setApplyOpen(true)}>
              <FiPlusCircle className="size-4" /> Apply for a loan
            </Button>
          }
        />
      ) : (
        <div className="space-y-5">
          <div className="grid gap-5 lg:grid-cols-2">
            <QuickActions onApply={() => setApplyOpen(true)} onGo={navigate} />
          </div>
          <div className="grid gap-5 lg:grid-cols-2">
            <Card
              title="Loan application"
              subtitle={loanRef}
              action={<LoanStatusPill status={selected.status} />}
              contentClassName="space-y-4"
              content={
                <>
                  <div>
                    <p className="text-sm text-brand">{selected.purpose}</p>
                    <p className="font-heading text-2xl font-bold text-ink">
                      <Money amount={selected.amount} />
                    </p>
                  </div>
                  <LoanLifecycle status={selected.status} />
                  <div className="flex items-center justify-between">
                    <Button variant="ghost" onClick={() => setDetailOpen(true)}>
                      View application <FiArrowRight className="size-4" />
                    </Button>
                    <Pager index={index} count={loans.length} onChange={setCurrent} label="loan" />
                  </div>
                </>
              }
            />

            {isActive ? (
              <Card
                title="Your loan"
                subtitle={loanRef}
                action={<LoanStatusPill status={selected.status} />}
                contentClassName="space-y-4"
                content={
                  <>
                    <div className="flex items-end justify-between">
                      <div>
                        <p className="text-sm text-brand">Repaid</p>
                        <p className="font-heading text-2xl font-bold text-ink">
                          <Money amount={totalPaid} />
                        </p>
                      </div>
                      <p className="text-sm text-brand">
                        of <Money amount={totalDue} />
                      </p>
                    </div>
                    <ProgressBar value={repaymentProgress(totalPaid, totalDue)} />
                    {next && (
                      <div className="flex items-center justify-between rounded-lg bg-gold-tint px-3 py-2 text-sm">
                        <span className="text-ink">Next payment · {formatDate(next.dueDate)}</span>
                        <span className="font-semibold text-ink">
                          <Money amount={Math.max(0, next.amountDue - next.amountPaid)} />
                        </span>
                      </div>
                    )}
                    <Button onClick={() => setPayOpen(true)}>
                      <FiCreditCard className="size-4" /> Make a payment
                    </Button>
                  </>
                }
              />
            ) : (
              <Card
                title="Details"
                subtitle={loanRef}
                content={<LoanSummary loan={selected} />}
              />
            )}
          </div>

          {isActive && schedules.length > 0 && (
            <Card
              title="Payment schedule"
              subtitle={loanRef}
              content={<ScheduleTable schedules={schedules} />}
            />
          )}

          {!isActive && (
            <Card
              title="History"
              subtitle={loanRef}
              content={
                <>
                  {historyQuery.isLoading && <p className="text-sm text-brand">Loading…</p>}
                  {historyQuery.isError && (
                    <ErrorState error={historyQuery.error} onRetry={historyQuery.refetch} />
                  )}
                  {historyQuery.data && <HistoryTimeline items={historyQuery.data} />}
                </>
              }
            />
          )}
        </div>
      )}
    </>
  )
}

export default DashboardPage
