import { useNavigate } from 'react-router-dom'
import { useCreateLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import type { LoanApplicationCreateRequest } from '@/api/loan-applications/loan-applications.types'
import { useAuth } from '@/auth/auth-context'
import { useProfile } from '@/hooks/profile/use-profile'
import { LoanForm } from '@/components/loan/loan-form'
import ModalDialog from '@/components/shared/components/modal-dialog'

type ApplyLoanModalProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export const ApplyLoanModal = ({ open, onOpenChange }: ApplyLoanModalProps) => {
  const navigate = useNavigate()
  const create = useCreateLoanApplication()
  const { user } = useAuth()
  const { fullName } = useProfile()

  const handleSubmit = (payload: LoanApplicationCreateRequest) => {
    create.mutate(payload, {
      onSuccess: () => {
        onOpenChange(false)
        navigate('/loans')
      },
    })
  }

  return (
    <ModalDialog
      open={open}
      onOpenChange={onOpenChange}
      onClose={() => onOpenChange(false)}
      title="Apply for a loan"
      desc="Tell us what you need. You'll see an estimate as you go."
      size="lg"
    >
      <LoanForm
        initial={{ applicantName: fullName.trim() || user?.username || '' }}
        submitLabel="Submit application"
        pending={create.isPending}
        error={create.isError ? create.error : undefined}
        onSubmit={handleSubmit}
        onCancel={() => onOpenChange(false)}
      />
    </ModalDialog>
  )
}

export default ApplyLoanModal
