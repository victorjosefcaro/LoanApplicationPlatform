import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  approveLoanApplication,
  cancelLoanApplication,
  createLoanApplication,
  releaseLoanApplicationFunds,
  reviewLoanApplication,
  updateLoanApplication,
} from './loan-applications.api'
import { loanApplicationKeys } from './loan-applications.keys'
import type {
  ApproveRequest,
  LoanApplicationUpdateRequest,
  ReviewRequest,
} from './loan-applications.types'

export const useCreateLoanApplication = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: createLoanApplication,
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}

export const useUpdateLoanApplication = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; payload: LoanApplicationUpdateRequest }) =>
      updateLoanApplication(vars.id, vars.payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}

export const useCancelLoanApplication = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => cancelLoanApplication(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}

export const useReviewLoanApplication = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; payload: ReviewRequest }) =>
      reviewLoanApplication(vars.id, vars.payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}

export const useApproveLoanApplication = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; payload: ApproveRequest }) =>
      approveLoanApplication(vars.id, vars.payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}

export const useReleaseLoanApplicationFunds = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => releaseLoanApplicationFunds(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: loanApplicationKeys.all }),
  })
}
