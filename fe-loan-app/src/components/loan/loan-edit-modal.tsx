import { useState } from 'react'
import { useUpdateLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import type {
  LoanApplication,
  LoanApplicationUpdateRequest,
} from '@/api/loan-applications/loan-applications.types'
import ModalDialog from '@/components/shared/components/modal-dialog'
import ConfirmDialog from '@/components/shared/components/confirm-dialog'
import { LoanForm } from '@/components/loan/loan-form'

type LoanEditModalProps = {
  loan: LoanApplication
  open: boolean
  onOpenChange: (open: boolean) => void
}

export const LoanEditModal = ({ loan, open, onOpenChange }: LoanEditModalProps) => {
  const update = useUpdateLoanApplication()

  // Submitting the form only stages the changes — the applicant confirms before
  // they are saved and the application is resubmitted.
  const [pendingPayload, setPendingPayload] = useState<LoanApplicationUpdateRequest | null>(null)

  const handleSubmit = (payload: LoanApplicationUpdateRequest) => setPendingPayload(payload)

  const handleConfirm = () => {
    if (!pendingPayload) return
    update.mutate(
      { id: loan.id, payload: pendingPayload },
      {
        onSuccess: () => {
          setPendingPayload(null)
          onOpenChange(false)
        },
      },
    )
  }

  return (
    <>
      <ModalDialog
        open={open}
        onOpenChange={onOpenChange}
        onClose={() => onOpenChange(false)}
        title="Edit application"
        desc="Update the details and resubmit."
        size="lg"
      >
        <LoanForm
          initial={{
            applicantName: loan.applicantName,
            amount: String(loan.amount),
            termInMonths: String(loan.termInMonths),
            monthlyIncome: String(loan.monthlyIncome),
            purpose: loan.purpose,
          }}
          submitLabel="Save changes"
          pending={update.isPending}
          error={update.isError ? update.error : undefined}
          onSubmit={handleSubmit}
          onCancel={() => onOpenChange(false)}
        />
      </ModalDialog>

      <ConfirmDialog
        open={pendingPayload !== null}
        onOpenChange={(next) => !next && setPendingPayload(null)}
        title="Save changes to this application?"
        description="This updates your application and resubmits it for review."
        confirmLabel="Save changes"
        pendingLabel="Saving…"
        pending={update.isPending}
        error={update.isError ? update.error : undefined}
        onConfirm={handleConfirm}
      />
    </>
  )
}

export default LoanEditModal
