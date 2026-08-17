import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { FiEye } from 'react-icons/fi'
import { useLoanApplications } from '@/api/loan-applications/loan-applications.queries'
import type { LoanApplication } from '@/api/loan-applications/loan-applications.types'
import { formatDate } from '@/utils/format'
import { cn } from '@/lib/utils'
import PageHeader from '@/components/page-header'
import { DataTable, type ColumnDef, type ActionDef } from '@/components/shared'
import { Money } from '@/components/money/money'
import { LoanStatusPill } from '@/components/status-pill'
import { ErrorState } from '@/components/states'

const FILTERS: { label: string; value: string | undefined }[] = [
  { label: 'All', value: undefined },
  { label: 'Submitted', value: 'Submitted' },
  { label: 'Under review', value: 'Reviewed' },
  { label: 'Approved', value: 'Approved' },
  { label: 'Funds released', value: 'Released' },
  { label: 'Returned', value: 'Returned' },
]

const PAGE_SIZE = 10

export const ReviewQueuePage = () => {
  const navigate = useNavigate()
  const [status, setStatus] = useState<string | undefined>('Submitted')
  const [page, setPage] = useState(1)
  const [searchTerm, setSearchTerm] = useState('')

  const query = useLoanApplications({ status, pageNumber: page, pageSize: PAGE_SIZE })
  const items = query.data?.items ?? []
  const totalCount = query.data?.pagination?.totalCount ?? items.length

  const columns: ColumnDef<LoanApplication>[] = [
    { key: 'applicantName', label: 'Applicant' },
    {
      key: 'amount',
      label: 'Amount',
      render: (loan) => <Money amount={loan.amount} compact />,
    },
    { key: 'termInMonths', label: 'Term', render: (loan) => `${loan.termInMonths} mo` },
    { key: 'purpose', label: 'Purpose' },
    { key: 'status', label: 'Status', render: (loan) => <LoanStatusPill status={loan.status} /> },
    { key: 'createdAt', label: 'Applied', render: (loan) => formatDate(loan.createdAt) },
  ]

  const actions: ActionDef<LoanApplication>[] = [
    {
      icon: <FiEye />,
      label: 'Review',
      onClick: (loan) => navigate(`/admin/loans/${loan.id}`),
    },
  ]

  const selectFilter = (value: string | undefined) => {
    setStatus(value)
    setPage(1)
  }

  return (
    <div>
      <PageHeader title="Review queue" subtitle="Applications waiting on the team." />

      <div className="mb-4 flex flex-wrap gap-2">
        {FILTERS.map((filter) => (
          <button
            key={filter.label}
            type="button"
            onClick={() => selectFilter(filter.value)}
            className={cn(
              'rounded-full px-3 py-1.5 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand',
              status === filter.value
                ? 'bg-brand text-white'
                : 'bg-surface text-brand ring-1 ring-line hover:text-ink',
            )}
          >
            {filter.label}
          </button>
        ))}
      </div>

      {query.isError ? (
        <ErrorState error={query.error} onRetry={query.refetch} />
      ) : (
        <DataTable<LoanApplication>
          data={items}
          columns={columns}
          actions={actions}
          searchTerm={searchTerm}
          setSearchTerm={setSearchTerm}
          searchPlaceholder="Search this page…"
          showAddButton={false}
          isLoading={query.isLoading}
          emptyMessage="No applications with this status right now."
          page={page}
          totalCount={totalCount}
          pageSize={PAGE_SIZE}
          onPageChange={setPage}
        />
      )}
    </div>
  )
}

export default ReviewQueuePage
