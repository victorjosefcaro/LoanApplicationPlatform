import { useUpdateLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import type {
  LoanApplication,
  LoanApplicationUpdateRequest,
} from '@/api/loan-applications/loan-applications.types'
import ModalDialog from '@/components/shared/components/modal-dialog'
import { LoanForm } from '@/components/loan/loan-form'

type LoanEditModalProps = {
  loan: LoanApplication
  open: boolean
  onOpenChange: (open: boolean) => void
}

export const LoanEditModal = ({ loan, open, onOpenChange }: LoanEditModalProps) => {
  const update = useUpdateLoanApplication()

  const handleSubmit = (payload: LoanApplicationUpdateRequest) => {
    update.mutate({ id: loan.id, payload }, { onSuccess: () => onOpenChange(false) })
  }

  return (
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
  )
}

export default LoanEditModal
