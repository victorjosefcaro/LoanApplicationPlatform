import { useState, type ReactNode } from 'react'
import { FiCreditCard, FiEdit2 } from 'react-icons/fi'
import {
  useLoanApplication,
  useLoanApplicationHistory,
} from '@/api/loan-applications/loan-applications.queries'
import { useCancelLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import { isEditableLoan } from '@/constants'
import ModalDialog from '@/components/shared/components/modal-dialog'
import ConfirmDialog from '@/components/shared/components/confirm-dialog'
import { Card } from '@/components/shared'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanSummary } from '@/components/loan/loan-summary'
import { HistoryTimeline } from '@/components/loan/history-timeline'
import { LoanEditModal } from '@/components/loan/loan-edit-modal'
import { PaymentScheduleModal } from '@/components/payments/payment-schedule-modal'
import { LoadingState, ErrorState } from '@/components/states'

type LoanApplicationModalProps = {
  loanApplicationId: number
  open: boolean
  onOpenChange: (open: boolean) => void
}

export const LoanApplicationModal = ({
  loanApplicationId: id,
  open,
  onOpenChange,
}: LoanApplicationModalProps) => {
  const loanQuery = useLoanApplication(id, { enabled: open })
  const historyQuery = useLoanApplicationHistory(id, { enabled: open })
  const cancel = useCancelLoanApplication()

  const [confirmOpen, setConfirmOpen] = useState(false)
  const [payOpen, setPayOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)

  const loan = loanQuery.data
  const editable = loan ? isEditableLoan(loan.status) : false
  // Applicants can only revise an application that was returned to them.
  const canEdit = loan?.status === 'Returned'
  const payable = loan?.status === 'Released'

  // Open the edit modal on top of this one — the detail modal stays open behind it.
  const handleEdit = () => setEditOpen(true)

  const handleCancel = () => {
    cancel.mutate(id, {
      onSuccess: () => {
        setConfirmOpen(false)
        onOpenChange(false)
      },
    })
  }

  const actions: {
    type: 'button'
    variant: 'default' | 'outline' | 'destructive'
    value: ReactNode
    onClick: () => void
    disabled?: boolean
  }[] = []
  if (payable) {
    actions.push({
      type: 'button',
      variant: 'default',
      value: (
        <>
          <FiCreditCard className="size-4" /> Make a payment
        </>
      ),
      onClick: () => setPayOpen(true),
    })
  }
  if (editable) {
    actions.push({
      type: 'button',
      variant: 'outline',
      value: (
        <>
          <FiEdit2 className="size-4" /> Edit
        </>
      ),
      onClick: handleEdit,
      disabled: !canEdit,
    })
    actions.push({
      type: 'button',
      variant: 'destructive',
      value: 'Cancel application',
      onClick: () => setConfirmOpen(true),
    })
  }

  return (
    <>
      <ModalDialog
        open={open}
        onOpenChange={onOpenChange}
        title={loan?.purpose || `Application #${id}`}
        desc={`Application #${id}`}
        size="xl"
        actionButton={actions.length ? actions : undefined}
      >
        {loanQuery.isLoading ? (
          <LoadingState label="Loading your application…" />
        ) : loanQuery.isError ? (
          <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />
        ) : loan ? (
          <div className="grid gap-5 lg:grid-cols-[1fr_22rem]">
            <div className="space-y-5">
              <Card title="Status" content={<LoanLifecycle status={loan.status} />} />
              <Card title="Details" content={<LoanSummary loan={loan} />} />
            </div>

            <Card
              title="History"
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
          </div>
        ) : null}
      </ModalDialog>

      {loan && <LoanEditModal loan={loan} open={editOpen} onOpenChange={setEditOpen} />}

      <PaymentScheduleModal
        loanApplicationId={id}
        purpose={loan?.purpose}
        open={payOpen}
        onOpenChange={setPayOpen}
      />

      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={(next) => !next && setConfirmOpen(false)}
        title="Cancel this application?"
        description="This can't be undone. You can always apply again later."
        body={
          <p className="text-sm text-brand">
            Cancelling stops this application from moving forward.
          </p>
        }
        cancelLabel="Keep it"
        confirmLabel="Cancel application"
        pendingLabel="Cancelling…"
        confirmVariant="destructive"
        pending={cancel.isPending}
        error={cancel.isError ? cancel.error : undefined}
        onConfirm={handleCancel}
      />
    </>
  )
}

export default LoanApplicationModal
