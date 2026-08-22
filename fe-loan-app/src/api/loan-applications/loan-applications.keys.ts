import type { LoanApplicationListParams } from './loan-applications.types'

export const loanApplicationKeys = {
  all: ['loan-applications'] as const,
  lists: () => [...loanApplicationKeys.all, 'list'] as const,
  list: (params?: LoanApplicationListParams) =>
    [...loanApplicationKeys.lists(), params ?? {}] as const,
  details: () => [...loanApplicationKeys.all, 'detail'] as const,
  detail: (id: number) => [...loanApplicationKeys.details(), id] as const,
  history: (id: number) => [...loanApplicationKeys.detail(id), 'history'] as const,
}
