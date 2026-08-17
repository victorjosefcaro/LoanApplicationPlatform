import { useState, type ChangeEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useLoanApplication } from '@/api/loan-applications/loan-applications.queries'
import { usePaymentSchedules } from '@/api/payments/payments.queries'
import { useSubmitPayment } from '@/api/payments/payments.mutations'
import type { PaymentSchedule } from '@/api/payments/payments.types'
import PageHeader from '@/components/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import InputField from '@/components/shared/components/input-field'
import { Money } from '@/components/money/money'
import { ScheduleTable } from '@/components/payments/schedule-table'
import ModalDialog from '@/components/shared/components/modal-dialog'
import { LoadingState, ErrorState, EmptyState, InlineError } from '@/components/states'

const remainingOf = (s: PaymentSchedule): number => Math.max(0, s.amountDue - s.amountPaid)

export const LoanPayPage = () => {
  const params = useParams()
  const id = Number(params.id)
  const navigate = useNavigate()

  const loanQuery = useLoanApplication(id)
  const schedulesQuery = usePaymentSchedules(id)
  const submit = useSubmitPayment()

  const [active, setActive] = useState<PaymentSchedule | null>(null)
  const [amount, setAmount] = useState('')

  const openPay = (schedule: PaymentSchedule) => {
    setActive(schedule)
    setAmount(String(remainingOf(schedule)))
    submit.reset()
  }

  const handleSubmit = () => {
    if (!active) return
    submit.mutate(
      { loanApplicationId: id, scheduleId: active.id, payload: { amount: Number(amount) } },
      { onSuccess: () => setActive(null) },
    )
  }

  if (loanQuery.isLoading || schedulesQuery.isLoading) {
    return <LoadingState label="Loading your schedule…" />
  }
  if (loanQuery.isError) return <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />
  if (schedulesQuery.isError) {
    return <ErrorState error={schedulesQuery.error} onRetry={schedulesQuery.refetch} />
  }

  const schedules = schedulesQuery.data ?? []

  return (
    <div>
      <PageHeader
        title="Make a payment"
        subtitle={loanQuery.data ? `${loanQuery.data.purpose} · application #${id}` : `Application #${id}`}
        actions={
          <Button variant="ghost" onClick={() => navigate(`/loans/${id}`)}>
            Back to application
          </Button>
        }
      />

      <Card>
        <CardHeader>
          <CardTitle>Payment schedule</CardTitle>
        </CardHeader>
        <CardContent>
          {schedules.length === 0 ? (
            <EmptyState
              title="No schedule yet"
              message="Your amortization schedule appears once funds are released."
            />
          ) : (
            <ScheduleTable
              schedules={schedules}
              renderAction={(schedule) =>
                schedule.status === 'Paid' ? (
                  <span className="text-xs text-muted">Settled</span>
                ) : (
                  <Button size="sm" onClick={() => openPay(schedule)}>
                    Pay
                  </Button>
                )
              }
            />
          )}
        </CardContent>
      </Card>

      <ModalDialog
        open={active !== null}
        onOpenChange={() => setActive(null)}
        title="Submit a payment"
        desc={active ? `Installment due ${new Date(active.dueDate).toLocaleDateString()}` : undefined}
        size="sm"
        actionButton={[
          {
            type: 'button',
            variant: 'ghost',
            value: 'Cancel',
            onClick: () => setActive(null),
            disabled: submit.isPending,
          },
          {
            type: 'button',
            variant: 'default',
            value: submit.isPending ? 'Submitting…' : 'Submit payment',
            onClick: handleSubmit,
            disabled: submit.isPending || !(Number(amount) > 0),
          },
        ]}
      >
        <div className="space-y-3">
          {active && (
            <div className="flex justify-between rounded-lg bg-brand-tint/50 px-3 py-2 text-sm">
              <span className="text-brand-deep">Remaining on this installment</span>
              <span className="font-semibold text-ink">
                <Money amount={remainingOf(active)} />
              </span>
            </div>
          )}
          <InputField
            label="Amount (₱)"
            fieldType="number"
            value={amount}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setAmount(e.target.value)}
            isRequired
          />
          {submit.isError && <InlineError error={submit.error} />}
          <p className="text-xs text-muted">
            Your loan officer confirms the payment once it clears.
          </p>
        </div>
      </ModalDialog>
    </div>
  )
}

export default LoanPayPage
