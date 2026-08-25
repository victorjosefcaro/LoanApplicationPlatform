import { useQuery } from '@tanstack/react-query'
import {
  getLoanApplication,
  getLoanApplicationHistory,
  getLoanApplications,
} from './loan-applications.api'
import { loanApplicationKeys } from './loan-applications.keys'
import type { LoanApplicationListParams } from './loan-applications.types'

export const useLoanApplications = (params?: LoanApplicationListParams) =>
  useQuery({
    queryKey: loanApplicationKeys.list(params),
    queryFn: () => getLoanApplications(params),
  })

export const useLoanApplication = (id: number, options?: { enabled?: boolean }) =>
  useQuery({
    queryKey: loanApplicationKeys.detail(id),
    queryFn: () => getLoanApplication(id),
    enabled: Number.isFinite(id) && (options?.enabled ?? true),
  })

export const useLoanApplicationHistory = (id: number, options?: { enabled?: boolean }) =>
  useQuery({
    queryKey: loanApplicationKeys.history(id),
    queryFn: () => getLoanApplicationHistory(id),
    enabled: Number.isFinite(id) && (options?.enabled ?? true),
  })
