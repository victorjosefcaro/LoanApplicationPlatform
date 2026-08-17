import { useNavigate } from 'react-router-dom'
import { useCreateLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import type { LoanApplicationCreateRequest } from '@/api/loan-applications/loan-applications.types'
import { useAuth } from '@/auth/auth-context'
import PageHeader from '@/components/page-header'
import { Card, CardContent } from '@/components/ui/card'
import { LoanForm } from '@/components/loan/loan-form'

export const LoanNewPage = () => {
  const { user } = useAuth()
  const create = useCreateLoanApplication()
  const navigate = useNavigate()

  const handleSubmit = (payload: LoanApplicationCreateRequest) => {
    create.mutate(payload, {
      onSuccess: (loan) => navigate(`/loans/${loan.id}`, { replace: true }),
    })
  }

  return (
    <div>
      <PageHeader
        title="Apply for a loan"
        subtitle="Tell us what you need. You'll see an estimate as you go."
      />
      <Card>
        <CardContent>
          <LoanForm
            initial={{ applicantName: user?.username ?? '' }}
            submitLabel="Submit application"
            pending={create.isPending}
            error={create.isError ? create.error : undefined}
            onSubmit={handleSubmit}
            onCancel={() => navigate('/loans')}
          />
        </CardContent>
      </Card>
    </div>
  )
}

export default LoanNewPage
