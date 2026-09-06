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
import ConfirmDialog from '@/components/shared/components/confirm-dialog'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { LoanSummary } from '@/components/loan/loan-summary'
import { HistoryTimeline } from '@/components/loan/history-timeline'
import { Money } from '@/components/money/money'
import { LoadingState, ErrorState } from '@/components/states'
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
  const [pendingConfirm, setPendingConfirm] = useState<ConfirmConfig | null>(null)

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

  const closeConfirm = () => setPendingConfirm(null)

  // Every decision opens a confirmation first — the mutation only runs once the
  // admin confirms, and the dialog stays open while pending / on error.
  const askReview = (
    status: 'Reviewed' | 'Returned' | 'Rejected',
    copy: Omit<ConfirmConfig, 'run'>,
  ) =>
    setPendingConfirm({
      ...copy,
      run: () =>
        review.mutate(
          { id, payload: { status, remarks: remarks.trim() || undefined } },
          { onSuccess: closeConfirm },
        ),
    })

  const askApprove = (
    status: 'Approved' | 'Returned' | 'Rejected',
    copy: Omit<ConfirmConfig, 'run'>,
  ) =>
    setPendingConfirm({
      ...copy,
      run: () =>
        approve.mutate(
          { id, payload: { status, remarks: remarks.trim() || undefined } },
          { onSuccess: closeConfirm },
        ),
    })

  const askRelease = () =>
    setPendingConfirm({
      title: 'Release funds?',
      description: `This moves the loan amount from Treasury to the applicant and starts their repayment schedule for application #${id}. This can't be undone.`,
      confirmLabel: 'Release funds',
      run: () => release.mutate(id, { onSuccess: closeConfirm }),
    })

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
          primary={{
            label: 'Mark under review',
            onClick: () =>
              askReview('Reviewed', {
                title: 'Mark this application under review?',
                description: `This moves application #${id} into review.`,
                confirmLabel: 'Mark under review',
              }),
          }}
          secondary={{
            label: 'Return for changes',
            onClick: () =>
              askReview('Returned', {
                title: 'Return this application?',
                description: `The applicant will be asked to make changes to application #${id}.`,
                confirmLabel: 'Return',
              }),
          }}
          danger={{
            label: 'Reject',
            onClick: () =>
              askReview('Rejected', {
                title: 'Reject this application?',
                description: `This rejects application #${id}. This can't be undone.`,
                confirmLabel: 'Reject',
                confirmVariant: 'destructive',
              }),
          }}
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
          primary={{
            label: 'Approve',
            onClick: () =>
              askApprove('Approved', {
                title: 'Approve this application?',
                description: `This locks in the loan for application #${id}.`,
                confirmLabel: 'Approve',
              }),
          }}
          secondary={{
            label: 'Return for changes',
            onClick: () =>
              askApprove('Returned', {
                title: 'Return this application?',
                description: `The applicant will be asked to make changes to application #${id}.`,
                confirmLabel: 'Return',
              }),
          }}
          danger={{
            label: 'Reject',
            onClick: () =>
              askApprove('Rejected', {
                title: 'Reject this application?',
                description: `This rejects application #${id}. This can't be undone.`,
                confirmLabel: 'Reject',
                confirmVariant: 'destructive',
              }),
          }}
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
          <Button variant="gold" disabled={!canRelease || pending} onClick={askRelease}>
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
          <div className="flex gap-2">
            {loan.status === 'Released' && (
              <Button onClick={() => navigate(`/admin/loans/${loan.id}/payments`)}>
                Manage payments
              </Button>
            )}
            <Button variant="ghost" onClick={() => navigate('/admin')}>
              Back to queue
            </Button>
          </div>
        }
      />

      <div className="grid gap-5 lg:grid-cols-[1fr_22rem]">
        <div className="space-y-5">
          <Card title="Status" content={<LoanLifecycle status={loan.status} />} />

          <Card title="Application" content={<LoanSummary loan={loan} />} />

          <Card title="Decision" contentClassName="space-y-3" content={renderDecision()} />
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

      <ConfirmDialog
        open={!!pendingConfirm}
        onOpenChange={(next) => !next && closeConfirm()}
        title={pendingConfirm?.title ?? ''}
        description={pendingConfirm?.description}
        confirmLabel={pendingConfirm?.confirmLabel ?? 'Confirm'}
        confirmVariant={pendingConfirm?.confirmVariant}
        pending={pending}
        error={hasError ? actionError : undefined}
        onConfirm={() => pendingConfirm?.run()}
      />
    </div>
  )
}

type ConfirmConfig = {
  title: string
  description: string
  confirmLabel: string
  confirmVariant?: 'default' | 'destructive'
  run: () => void
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
