import { useQuery } from '@tanstack/react-query'
import { getTreasuryBalance, getTreasuryTransactions } from './treasury.api'
import { treasuryKeys } from './treasury.keys'
import type { TreasuryTransactionListParams } from './treasury.types'

export const useTreasuryBalance = () =>
  useQuery({
    queryKey: treasuryKeys.balance(),
    queryFn: getTreasuryBalance,
  })

export const useTreasuryTransactions = (params?: TreasuryTransactionListParams) =>
  useQuery({
    queryKey: treasuryKeys.transactionList(params),
    queryFn: () => getTreasuryTransactions(params),
  })
