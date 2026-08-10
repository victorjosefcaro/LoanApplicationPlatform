import type { TreasuryTransactionListParams } from './treasury.types'

export const treasuryKeys = {
  all: ['treasury'] as const,
  balance: () => [...treasuryKeys.all, 'balance'] as const,
  transactionLists: () => [...treasuryKeys.all, 'transactions'] as const,
  transactionList: (params?: TreasuryTransactionListParams) =>
    [...treasuryKeys.transactionLists(), params ?? {}] as const,
}
