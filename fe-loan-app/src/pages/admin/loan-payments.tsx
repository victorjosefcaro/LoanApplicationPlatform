import { useState, type ChangeEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useLoanApplication } from '@/api/loan-applications/loan-applications.queries'
import { usePaymentSchedules } from '@/api/payments/payments.queries'
import { usePostPayment } from '@/api/payments/payments.mutations'
import type { PaymentSchedule } from '@/api/payments/payments.types'
import PageHeader from '@/components/page-header'
import Card from '@/components/shared/components/card'
import { Button } from '@/components/ui/button'
import InputField from '@/components/shared/components/input-field'
import { Money } from '@/components/money/money'
import { ScheduleTable } from '@/components/payments/schedule-table'
import ModalDialog from '@/components/shared/components/modal-dialog'
import { LoadingState, ErrorState, EmptyState, InlineError } from '@/components/states'
import { isNotFoundError, notFound } from '@/api/is-not-found-error'

const remainingOf = (s: PaymentSchedule): number => Math.max(0, s.amountDue - s.amountPaid)
const defaultPost = (s: PaymentSchedule): number =>
  s.submittedAmount && s.submittedAmount > 0 ? s.submittedAmount : remainingOf(s)

export const AdminLoanPaymentsPage = () => {
  const params = useParams()
  const id = Number(params.id)
  const navigate = useNavigate()

  const loanQuery = useLoanApplication(id)
  const schedulesQuery = usePaymentSchedules(id)
  const post = usePostPayment()

  const [active, setActive] = useState<PaymentSchedule | null>(null)
  const [amount, setAmount] = useState('')

  const openPost = (schedule: PaymentSchedule) => {
    setActive(schedule)
    setAmount(String(defaultPost(schedule)))
    post.reset()
  }

  const handlePost = () => {
    if (!active) return
    post.mutate(
      { loanApplicationId: id, scheduleId: active.id, payload: { amount: Number(amount) } },
      { onSuccess: () => setActive(null) },
    )
  }

  if (!Number.isFinite(id)) throw notFound()
  if (loanQuery.isLoading || schedulesQuery.isLoading) {
    return <LoadingState label="Loading payments…" />
  }
  if (isNotFoundError(loanQuery.error)) throw notFound()
  if (loanQuery.isError) return <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />
  if (schedulesQuery.isError) {
    return <ErrorState error={schedulesQuery.error} onRetry={schedulesQuery.refetch} />
  }

  const schedules = schedulesQuery.data ?? []

  return (
    <div>
      <PageHeader
        title="Post payments"
        subtitle={
          loanQuery.data
            ? `${loanQuery.data.applicantName} · application #${id}`
            : `Application #${id}`
        }
        actions={
          <Button variant="ghost" onClick={() => navigate(`/admin/loans/${id}`)}>
            Back to application
          </Button>
        }
      />

      <Card
        title="Payment schedule"
        content={
          schedules.length === 0 ? (
            <EmptyState
              title="No schedule yet"
              message="The schedule is generated once funds are released to the applicant."
            />
          ) : (
            <ScheduleTable
              schedules={schedules}
              renderAction={(schedule) =>
                schedule.status === 'Paid' ? (
                  <span className="text-xs text-brand">Settled</span>
                ) : (
                  <Button
                    size="sm"
                    variant={schedule.status === 'PaymentSubmitted' ? 'default' : 'outline'}
                    onClick={() => openPost(schedule)}
                  >
                    {schedule.status === 'PaymentSubmitted' ? 'Confirm' : 'Post'}
                  </Button>
                )
              }
            />
          )
        }
      />

      <ModalDialog
        open={active !== null}
        onOpenChange={() => setActive(null)}
        title="Post a payment"
        desc={
          active ? `Installment due ${new Date(active.dueDate).toLocaleDateString()}` : undefined
        }
        size="sm"
        actionButton={[
          {
            type: 'button',
            variant: 'ghost',
            value: 'Cancel',
            onClick: () => setActive(null),
            disabled: post.isPending,
          },
          {
            type: 'button',
            variant: 'default',
            value: post.isPending ? 'Posting…' : 'Post payment',
            onClick: handlePost,
            disabled: post.isPending || !(Number(amount) > 0),
          },
        ]}
      >
        <div className="space-y-3">
          {active && (
            <div className="space-y-2 rounded-lg bg-brand-tint/50 px-3 py-2 text-sm">
              <div className="flex justify-between">
                <span className="text-brand-deep">Applicant submitted</span>
                <span className="font-semibold text-ink">
                  <Money amount={active.submittedAmount ?? 0} />
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-brand-deep">Remaining on installment</span>
                <span className="font-semibold text-ink">
                  <Money amount={remainingOf(active)} />
                </span>
              </div>
            </div>
          )}
          <InputField
            label="Amount to post (₱)"
            fieldType="number"
            value={amount}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setAmount(e.target.value)}
            isRequired
          />
          {post.isError && <InlineError error={post.error} />}
        </div>
      </ModalDialog>
    </div>
  )
}

export default AdminLoanPaymentsPage
