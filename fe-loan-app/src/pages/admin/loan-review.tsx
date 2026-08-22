import { useState, type ChangeEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  useLoanApplication,
  useLoanApplicationHistory,
} from '@/api/loan-applications/loan-applications.queries'
import {
  useApproveLoanApplication,
  useReleaseLoanApplicationFunds,
  useReviewLoanApplication,
} from '@/api/loan-applications/loan-applications.mutations'
import { ROLES } from '@/constants'
import { useAuth } from '@/auth/auth-context'
import PageHeader from '@/components/page-header'
import Card from '@/components/shared/components/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanSummary } from '@/components/loan/loan-summary'
import { HistoryTimeline } from '@/components/loan/history-timeline'
import { Money } from '@/components/money/money'
import { LoadingState, ErrorState, InlineError } from '@/components/states'
import { isNotFoundError, notFound } from '@/api/is-not-found-error'

export const AdminLoanReviewPage = () => {
  const params = useParams()
  const id = Number(params.id)
  const navigate = useNavigate()
  const { user } = useAuth()

  const loanQuery = useLoanApplication(id)
  const historyQuery = useLoanApplicationHistory(id)
  const review = useReviewLoanApplication()
  const approve = useApproveLoanApplication()
  const release = useReleaseLoanApplicationFunds()

  const [remarks, setRemarks] = useState('')

  if (!Number.isFinite(id)) throw notFound()
  if (loanQuery.isLoading) return <LoadingState label="Loading application…" />
  if (isNotFoundError(loanQuery.error)) throw notFound()
  if (loanQuery.isError) return <ErrorState error={loanQuery.error} onRetry={loanQuery.refetch} />

  const loan = loanQuery.data
  if (!loan) throw notFound()

  const role = user?.role
  const canReview = role === ROLES.REVIEWER || role === ROLES.ADMIN
  const canApprove = role === ROLES.APPROVER || role === ROLES.ADMIN
  const canRelease = role === ROLES.ADMIN

  const pending = review.isPending || approve.isPending || release.isPending
  const actionError = review.error ?? approve.error ?? release.error
  const hasError = review.isError || approve.isError || release.isError

  const runReview = (status: 'Reviewed' | 'Returned' | 'Rejected') =>
    review.mutate({ id, payload: { status, remarks: remarks.trim() || undefined } })
  const runApprove = (status: 'Approved' | 'Returned' | 'Rejected') =>
    approve.mutate({ id, payload: { status, remarks: remarks.trim() || undefined } })

  const renderDecision = () => {
    if (loan.status === 'Submitted') {
      return (
        <DecisionBlock
          note="Move this into review, send it back for changes, or reject it."
          disabled={!canReview}
          disabledNote="Only reviewers can action a submitted application."
          remarks={remarks}
          setRemarks={setRemarks}
          pending={pending}
          primary={{ label: 'Mark under review', onClick: () => runReview('Reviewed') }}
          secondary={{ label: 'Return for changes', onClick: () => runReview('Returned') }}
          danger={{ label: 'Reject', onClick: () => runReview('Rejected') }}
        />
      )
    }
    if (loan.status === 'Reviewed') {
      return (
        <DecisionBlock
          note="Approve to lock in the loan, or send it back."
          disabled={!canApprove}
          disabledNote="Only approvers can approve an application."
          remarks={remarks}
          setRemarks={setRemarks}
          pending={pending}
          primary={{ label: 'Approve', onClick: () => runApprove('Approved') }}
          secondary={{ label: 'Return for changes', onClick: () => runApprove('Returned') }}
          danger={{ label: 'Reject', onClick: () => runApprove('Rejected') }}
        />
      )
    }
    if (loan.status === 'Approved') {
      return (
        <div className="space-y-3">
          <p className="text-sm text-brand">
            Releasing moves <Money amount={loan.amount} /> from Treasury to the applicant. This
            starts their repayment schedule.
          </p>
          {!canRelease && <p className="text-sm text-coral">Only an admin can release funds.</p>}
          <Button
            variant="gold"
            disabled={!canRelease || pending}
            onClick={() => release.mutate(id)}
          >
            {release.isPending ? 'Releasing…' : 'Release funds'}
          </Button>
        </div>
      )
    }
    return (
      <p className="text-sm text-brand">
        No action needed — this application is {loan.status.toLowerCase()}.
      </p>
    )
  }

  return (
    <div>
      <PageHeader
        title={`${loan.applicantName}`}
        subtitle={`Application #${loan.id} · ${loan.purpose}`}
        actions={
          <Button variant="ghost" onClick={() => navigate('/admin')}>
            Back to queue
          </Button>
        }
      />

      <div className="grid gap-5 lg:grid-cols-[1fr_22rem]">
        <div className="space-y-5">
          <Card title="Status" content={<LoanLifecycle status={loan.status} />} />

          <Card title="Application" content={<LoanSummary loan={loan} />} />

          <Card
            title="Decision"
            contentClassName="space-y-3"
            content={
              <>
                {renderDecision()}
                {hasError && <InlineError error={actionError} />}
              </>
            }
          />
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
    </div>
  )
}

type DecisionAction = { label: string; onClick: () => void }

const DecisionBlock = ({
  note,
  disabled,
  disabledNote,
  remarks,
  setRemarks,
  pending,
  primary,
  secondary,
  danger,
}: {
  note: string
  disabled: boolean
  disabledNote: string
  remarks: string
  setRemarks: (value: string) => void
  pending: boolean
  primary: DecisionAction
  secondary: DecisionAction
  danger: DecisionAction
}) => (
  <div className="space-y-3">
    <p className="text-sm text-brand">{note}</p>
    {disabled ? (
      <p className="text-sm text-coral">{disabledNote}</p>
    ) : (
      <>
        <Textarea
          value={remarks}
          onChange={(e: ChangeEvent<HTMLTextAreaElement>) => setRemarks(e.target.value)}
          placeholder="Add a note for the applicant (optional)"
        />
        <div className="flex flex-wrap gap-2">
          <Button disabled={pending} onClick={primary.onClick}>
            {primary.label}
          </Button>
          <Button variant="outline" disabled={pending} onClick={secondary.onClick}>
            {secondary.label}
          </Button>
          <Button variant="destructive" disabled={pending} onClick={danger.onClick}>
            {danger.label}
          </Button>
        </div>
      </>
    )}
  </div>
)

export default AdminLoanReviewPage
