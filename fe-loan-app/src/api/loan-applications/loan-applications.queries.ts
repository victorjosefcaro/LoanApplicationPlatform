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

export const useLoanApplication = (id: number) =>
  useQuery({
    queryKey: loanApplicationKeys.detail(id),
    queryFn: () => getLoanApplication(id),
    enabled: Number.isFinite(id),
  })

export const useLoanApplicationHistory = (id: number) =>
  useQuery({
    queryKey: loanApplicationKeys.history(id),
    queryFn: () => getLoanApplicationHistory(id),
    enabled: Number.isFinite(id),
  })
