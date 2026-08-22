import { apiClient } from '@/api/client'
import { unwrap, unwrapPaged } from '@/api/unwrap'
import type {
  DepositRequest,
  TreasuryBalance,
  TreasuryTransaction,
  TreasuryTransactionListParams,
} from './treasury.types'

const TREASURY_API = '/treasury'

export const getTreasuryBalance = async (): Promise<TreasuryBalance> => {
  const response = await apiClient.get<ApiBodyResponse<TreasuryBalance>>(`${TREASURY_API}/balance`)
  return unwrap(response)
}

export const depositFunds = async (payload: DepositRequest): Promise<TreasuryBalance> => {
  const response = await apiClient.post<ApiBodyResponse<TreasuryBalance>>(
    `${TREASURY_API}/deposit`,
    payload,
  )
  return unwrap(response)
}

export const getTreasuryTransactions = async (
  params?: TreasuryTransactionListParams,
): Promise<PagedResult<TreasuryTransaction>> => {
  const response = await apiClient.get<TreasuryTransaction[]>(`${TREASURY_API}/transactions`, {
    params,
  })
  return unwrapPaged(response)
}
