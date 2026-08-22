import { useState, type ChangeEvent, type FormEvent } from 'react'
import { useTreasuryBalance, useTreasuryTransactions } from '@/api/treasury/treasury.queries'
import { useDepositFunds } from '@/api/treasury/treasury.mutations'
import type { TreasuryTransaction } from '@/api/treasury/treasury.types'
import { ROLES } from '@/constants'
import { useAuth } from '@/auth/auth-context'
import { formatDateTime } from '@/utils/format'
import PageHeader from '@/components/page-header'
import Card from '@/components/shared/components/card'
import { Button } from '@/components/ui/button'
import InputField from '@/components/shared/components/input-field'
import { Money } from '@/components/money/money'
import { DataTable, type ColumnDef } from '@/components/shared'
import { LoadingState, ErrorState, InlineError } from '@/components/states'

const PAGE_SIZE = 10

const DepositForm = () => {
  const [amount, setAmount] = useState('')
  const deposit = useDepositFunds()

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const value = Number(amount)
    if (!(value > 0)) return
    deposit.mutate({ amount: value }, { onSuccess: () => setAmount('') })
  }

  return (
    <Card
      title="Add funds"
      content={
        <form onSubmit={handleSubmit} className="space-y-3" noValidate>
          <InputField
            label="Deposit amount (₱)"
            fieldType="number"
            value={amount}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setAmount(e.target.value)}
            isRequired
            placeholder="100000"
          />
          {deposit.isError && <InlineError error={deposit.error} />}
          {deposit.isSuccess && (
            <p className="text-sm font-medium text-brand">Funds added to Treasury.</p>
          )}
          <Button
            type="submit"
            variant="gold"
            disabled={deposit.isPending || !(Number(amount) > 0)}
          >
            {deposit.isPending ? 'Depositing…' : 'Deposit'}
          </Button>
        </form>
      }
    />
  )
}

const TransactionsLedger = () => {
  const [page, setPage] = useState(1)
  const [searchTerm, setSearchTerm] = useState('')
  const query = useTreasuryTransactions({ pageNumber: page, pageSize: PAGE_SIZE })

  const items = query.data?.items ?? []
  const totalCount = query.data?.pagination?.totalCount ?? items.length

  const columns: ColumnDef<TreasuryTransaction>[] = [
    { key: 'transactionDate', label: 'Date', render: (t) => formatDateTime(t.transactionDate) },
    { key: 'type', label: 'Type' },
    {
      key: 'referenceId',
      label: 'Reference',
      render: (t) => (t.referenceId ? `#${t.referenceId}` : '—'),
    },
    {
      key: 'amount',
      label: 'Amount',
      render: (t) => (
        <Money amount={t.amount} signed className={t.amount < 0 ? 'text-coral' : 'text-brand'} />
      ),
    },
  ]

  if (query.isError) return <ErrorState error={query.error} onRetry={query.refetch} />

  return (
    <DataTable<TreasuryTransaction>
      data={items}
      columns={columns}
      searchTerm={searchTerm}
      setSearchTerm={setSearchTerm}
      searchPlaceholder="Search this page…"
      showAddButton={false}
      isLoading={query.isLoading}
      emptyMessage="No transactions yet."
      page={page}
      totalCount={totalCount}
      pageSize={PAGE_SIZE}
      onPageChange={setPage}
    />
  )
}

export const TreasuryPage = () => {
  const { user } = useAuth()
  const isAdmin = user?.role === ROLES.ADMIN
  const balanceQuery = useTreasuryBalance()

  return (
    <div>
      <PageHeader title="Treasury" subtitle="The pool funds are released from." />

      <div className="grid gap-5 lg:grid-cols-[1fr_20rem]">
        <div className="space-y-5">
          <Card
            title="Available balance"
            content={
              <>
                {balanceQuery.isLoading && <LoadingState label="Loading balance…" />}
                {balanceQuery.isError && (
                  <ErrorState error={balanceQuery.error} onRetry={balanceQuery.refetch} />
                )}
                {balanceQuery.data && (
                  <p className="font-heading text-4xl font-extrabold text-ink">
                    <Money amount={balanceQuery.data.balance} />
                  </p>
                )}
              </>
            }
          />

          {isAdmin ? (
            <Card title="Transactions" content={<TransactionsLedger />} />
          ) : (
            <p className="text-sm text-brand">
              Only admins can deposit funds or view the transaction ledger.
            </p>
          )}
        </div>

        {isAdmin && <DepositForm />}
      </div>
    </div>
  )
}

export default TreasuryPage
