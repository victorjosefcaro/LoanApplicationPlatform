import { useNavigate } from 'react-router-dom'
import { FiArrowRight, FiCreditCard, FiFileText, FiPlusCircle } from 'react-icons/fi'
import { useLoanApplications } from '@/api/loan-applications/loan-applications.queries'
import { usePaymentSchedules } from '@/api/payments/payments.queries'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import type { PaymentSchedule } from '@/api/payments/payments.types'
import { formatDate } from '@/utils/format'
import { repaymentProgress } from '@/utils/finance'
import PageHeader from '@/components/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Money } from '@/components/money/money'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanStatusPill } from '@/components/status-pill'
import { ProgressBar } from '@/components/progress-bar'
import { ScheduleTable } from '@/components/payments/schedule-table'
import { LoadingState, ErrorState, EmptyState } from '@/components/states'

const IN_PROGRESS = new Set(['Submitted', 'Returned', 'Reviewed', 'Approved'])
const ACTIVE = new Set(['Released', 'Completed'])

const nextUnpaid = (schedules: PaymentSchedule[]): PaymentSchedule | null => {
  const pending = schedules
    .filter((s) => s.status !== 'Paid')
    .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime())
  return pending[0] ?? null
}

const QuickActions = ({ onGo }: { onGo: (path: string) => void }) => (
  <Card>
    <CardHeader>
      <CardTitle>Quick actions</CardTitle>
    </CardHeader>
    <CardContent className="grid gap-2">
      <Button variant="outline" className="justify-start" onClick={() => onGo('/loans/new')}>
        <FiPlusCircle className="size-4" /> Apply for a loan
      </Button>
      <Button variant="outline" className="justify-start" onClick={() => onGo('/loans')}>
        <FiFileText className="size-4" /> View my loans
      </Button>
    </CardContent>
  </Card>
)

export const DashboardPage = () => {
  const navigate = useNavigate()
  const loansQuery = useLoanApplications({ pageSize: 50 })

  const loans = loansQuery.data?.items ?? []
  const inReview: LoanApplication | undefined = loans.find((l) => IN_PROGRESS.has(l.status))
  const active: LoanApplication | undefined = loans.find((l) => ACTIVE.has(l.status))

  const schedulesQuery = usePaymentSchedules(active?.id ?? Number.NaN)
  const schedules = schedulesQuery.data ?? []

  const totalDue = schedules.reduce((sum, s) => sum + s.amountDue, 0)
  const totalPaid = schedules.reduce((sum, s) => sum + s.amountPaid, 0)
  const next = nextUnpaid(schedules)

  if (loansQuery.isLoading) return <LoadingState label="Loading your dashboard…" />
  if (loansQuery.isError) return <ErrorState error={loansQuery.error} onRetry={loansQuery.refetch} />

  const nothingYet = loans.length === 0

  return (
    <>
      <PageHeader title="Overview" subtitle="Here's where your loans stand." />

      {nothingYet ? (
        <EmptyState
          title="Welcome to Loanly"
          message="You don't have any loans yet. Apply for your first one — it only takes a minute."
          action={
            <Button onClick={() => navigate('/loans/new')}>
              <FiPlusCircle className="size-4" /> Apply for a loan
            </Button>
          }
        />
      ) : (
        <div className="space-y-5">
          <div className="grid gap-5 lg:grid-cols-2">
            {/* In-review application */}
            {inReview ? (
              <Card>
                <CardHeader className="flex-row items-center justify-between">
                  <CardTitle>Application in progress</CardTitle>
                  <LoanStatusPill status={inReview.status} />
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <p className="text-sm text-muted">{inReview.purpose}</p>
                    <p className="font-heading text-2xl font-bold text-ink">
                      <Money amount={inReview.amount} />
                    </p>
                  </div>
                  <LoanLifecycle status={inReview.status} />
                  <Button variant="ghost" className="px-0" onClick={() => navigate(`/loans/${inReview.id}`)}>
                    View application <FiArrowRight className="size-4" />
                  </Button>
                </CardContent>
              </Card>
            ) : (
              <Card>
                <CardHeader>
                  <CardTitle>Apply for a loan</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <p className="text-sm text-muted">
                    Need funds? Start an application and see your estimate instantly.
                  </p>
                  <Button onClick={() => navigate('/loans/new')}>
                    <FiPlusCircle className="size-4" /> Apply for a loan
                  </Button>
                </CardContent>
              </Card>
            )}

            {/* Active loan repayment */}
            {active && (
              <Card>
                <CardHeader className="flex-row items-center justify-between">
                  <CardTitle>Your loan</CardTitle>
                  <LoanStatusPill status={active.status} />
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex items-end justify-between">
                    <div>
                      <p className="text-sm text-muted">Repaid</p>
                      <p className="font-heading text-2xl font-bold text-ink">
                        <Money amount={totalPaid} />
                      </p>
                    </div>
                    <p className="text-sm text-muted">
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
                  <Button onClick={() => navigate(`/loans/${active.id}/pay`)}>
                    <FiCreditCard className="size-4" /> Make a payment
                  </Button>
                </CardContent>
              </Card>
            )}
          </div>

          {/* Schedule for the active loan */}
          {active && schedules.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Payment schedule</CardTitle>
              </CardHeader>
              <CardContent>
                <ScheduleTable schedules={schedules} />
              </CardContent>
            </Card>
          )}

          <div className="grid gap-5 lg:grid-cols-2">
            <QuickActions onGo={navigate} />
          </div>
        </div>
      )}
    </>
  )
}

export default DashboardPage
