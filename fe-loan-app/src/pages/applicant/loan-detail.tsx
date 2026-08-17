import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { FiCreditCard, FiEdit2 } from 'react-icons/fi'
import {
  useLoanApplication,
  useLoanApplicationHistory,
} from '@/api/loan-applications/loan-applications.queries'
import { useCancelLoanApplication } from '@/api/loan-applications/loan-applications.mutations'
import { isEditableLoan } from '@/constants'
import PageHeader from '@/components/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanSummary } from '@/components/loan/loan-summary'
import { HistoryTimeline } from '@/components/loan/history-timeline'
import { LoadingState, ErrorState, EmptyState, InlineError } from '@/components/states'
import ModalDialog from '@/components/shared/components/modal-dialog'

export const LoanDetailPage = () => {
  const params = useParams()
  const id = Number(params.id)
  const navigate = useNavigate()

  const loanQuery = useLoanApplication(id)
  const historyQuery = useLoanApplicationHistory(id)
  const cancel = useCancelLoanApplication()
  const [confirmOpen, setConfirmOpen] = useState(false)

  if (loanQuery.isLoading) return <LoadingState label="Loading your application…" />
  if (loanQuery.isError) return <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />

  const loan = loanQuery.data
  if (!loan) {
    return (
      <EmptyState
        title="We couldn't find that application"
        message="It may have been removed, or the link is out of date."
        action={
          <Button variant="outline" onClick={() => navigate('/loans')}>
            Back to my loans
          </Button>
        }
      />
    )
  }

  const editable = isEditableLoan(loan.status)
  const payable = loan.status === 'Released'

  const handleCancel = () => {
    cancel.mutate(id, { onSuccess: () => setConfirmOpen(false) })
  }

  return (
    <div>
      <PageHeader
        title={loan.purpose || `Application #${loan.id}`}
        subtitle={`Application #${loan.id}`}
        actions={
          <div className="flex gap-2">
            {payable && (
              <Button onClick={() => navigate(`/loans/${id}/pay`)}>
                <FiCreditCard className="size-4" />
                Make a payment
              </Button>
            )}
            {editable && (
              <>
                <Button variant="outline" onClick={() => navigate(`/loans/${id}/edit`)}>
                  <FiEdit2 className="size-4" />
                  Edit
                </Button>
                <Button variant="destructive" onClick={() => setConfirmOpen(true)}>
                  Cancel application
                </Button>
              </>
            )}
          </div>
        }
      />

      <div className="grid gap-5 lg:grid-cols-[1fr_22rem]">
        <div className="space-y-5">
          <Card>
            <CardHeader>
              <CardTitle>Status</CardTitle>
            </CardHeader>
            <CardContent>
              <LoanLifecycle status={loan.status} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Details</CardTitle>
            </CardHeader>
            <CardContent>
              <LoanSummary loan={loan} />
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>History</CardTitle>
          </CardHeader>
          <CardContent>
            {historyQuery.isLoading && <p className="text-sm text-muted">Loading…</p>}
            {historyQuery.isError && (
              <ErrorState error={historyQuery.error} onRetry={historyQuery.refetch} />
            )}
            {historyQuery.data && <HistoryTimeline items={historyQuery.data} />}
          </CardContent>
        </Card>
      </div>

      <ModalDialog
        open={confirmOpen}
        onOpenChange={() => setConfirmOpen(false)}
        title="Cancel this application?"
        desc="This can't be undone. You can always apply again later."
        size="sm"
        actionButton={[
          {
            type: 'button',
            variant: 'ghost',
            value: 'Keep it',
            onClick: () => setConfirmOpen(false),
            disabled: cancel.isPending,
          },
          {
            type: 'button',
            variant: 'destructive',
            value: cancel.isPending ? 'Cancelling…' : 'Cancel application',
            onClick: handleCancel,
            disabled: cancel.isPending,
          },
        ]}
      >
        {cancel.isError ? <InlineError error={cancel.error} /> : (
          <p className="text-sm text-muted">
            Cancelling stops this application from moving forward.
          </p>
        )}
      </ModalDialog>
    </div>
  )
}

export default LoanDetailPage
