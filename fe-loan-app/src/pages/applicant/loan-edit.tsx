import { useNavigate, useParams } from 'react-router-dom'
import { useLoanApplication } from '@/api/loan-applications/loan-applications.queries'
import { useUpdateLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import type { LoanApplicationCreateRequest } from '@/api/loan-applications/loan-applications.types'
import { isEditableLoan } from '@/constants'
import PageHeader from '@/components/page-header'
import { Card, CardContent } from '@/components/ui/card'
import { LoanForm } from '@/components/loan/loan-form'
import { LoadingState, ErrorState, EmptyState } from '@/components/states'
import { isNotFoundError, notFound } from '@/api/is-not-found-error'
import { Button } from '@/components/ui/button'

export const LoanEditPage = () => {
  const params = useParams()
  const id = Number(params.id)
  const navigate = useNavigate()

  const loanQuery = useLoanApplication(id)
  const update = useUpdateLoanApplication()

  const handleSubmit = (payload: LoanApplicationCreateRequest) => {
    update.mutate({ id, payload }, { onSuccess: () => navigate(`/loans/${id}`, { replace: true }) })
  }

  if (!Number.isFinite(id)) throw notFound()
  if (loanQuery.isLoading) return <LoadingState label="Loading your application…" />
  if (isNotFoundError(loanQuery.error)) throw notFound()
  if (loanQuery.isError) return <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />

  const loan = loanQuery.data
  if (!loan) throw notFound()

  if (!isEditableLoan(loan.status)) {
    return (
      <EmptyState
        title="This application can't be edited"
        message="It's already moved past submission, so its details are locked."
        action={
          <Button variant="outline" onClick={() => navigate(`/loans/${id}`)}>
            Back to application
          </Button>
        }
      />
    )
  }

  return (
    <div>
      <PageHeader title="Edit application" subtitle="Update the details and resubmit." />
      <Card>
        <CardContent>
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
            onCancel={() => navigate(`/loans/${id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}

export default LoanEditPage
