export type TreasuryBalance = {
  balance: number
}

export type TreasuryTransaction = {
  id: number
  transactionDate: string
  amount: number
  type: string
  referenceId?: number | null
} & AuditFields

export type DepositRequest = {
  amount: number
}

export type TreasuryTransactionListParams = {
  pageNumber?: number
  pageSize?: number
  status?: string
}
