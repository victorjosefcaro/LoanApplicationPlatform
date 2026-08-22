import { apiClient } from '@/api/client'
import { unwrap, unwrapPaged } from '@/api/unwrap'
import type {
  ApproveRequest,
  LoanApplication,
  LoanApplicationCreateRequest,
  LoanApplicationListParams,
  LoanApplicationStatusHistory,
  LoanApplicationUpdateRequest,
  ReviewRequest,
} from './loan-applications.types'

const LOAN_APPLICATIONS_API = '/loanapplications'

export const getLoanApplications = async (
  params?: LoanApplicationListParams,
): Promise<PagedResult<LoanApplication>> => {
  const response = await apiClient.get<LoanApplication[]>(LOAN_APPLICATIONS_API, { params })
  return unwrapPaged(response)
}

export const getLoanApplication = async (id: number): Promise<LoanApplication> => {
  const response = await apiClient.get<ApiBodyResponse<LoanApplication>>(
    `${LOAN_APPLICATIONS_API}/${id}`,
  )
  return unwrap(response)
}

export const getLoanApplicationHistory = async (
  id: number,
): Promise<LoanApplicationStatusHistory[]> => {
  const response = await apiClient.get<ApiBodyResponse<LoanApplicationStatusHistory[]>>(
    `${LOAN_APPLICATIONS_API}/${id}/history`,
  )
  return unwrap(response)
}

export const createLoanApplication = async (
  payload: LoanApplicationCreateRequest,
): Promise<LoanApplication> => {
  const response = await apiClient.post<ApiBodyResponse<LoanApplication>>(
    LOAN_APPLICATIONS_API,
    payload,
  )
  return unwrap(response)
}

export const updateLoanApplication = async (
  id: number,
  payload: LoanApplicationUpdateRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.put<ApiBodyResponse<MessageResponse>>(
    `${LOAN_APPLICATIONS_API}/${id}`,
    payload,
  )
  return unwrap(response)
}

export const cancelLoanApplication = async (id: number): Promise<MessageResponse> => {
  const response = await apiClient.patch<ApiBodyResponse<MessageResponse>>(
    `${LOAN_APPLICATIONS_API}/${id}/cancel`,
  )
  return unwrap(response)
}

export const reviewLoanApplication = async (
  id: number,
  payload: ReviewRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.patch<ApiBodyResponse<MessageResponse>>(
    `${LOAN_APPLICATIONS_API}/${id}/review`,
    payload,
  )
  return unwrap(response)
}

export const approveLoanApplication = async (
  id: number,
  payload: ApproveRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.patch<ApiBodyResponse<MessageResponse>>(
    `${LOAN_APPLICATIONS_API}/${id}/approve`,
    payload,
  )
  return unwrap(response)
}

export const releaseLoanApplicationFunds = async (id: number): Promise<MessageResponse> => {
  const response = await apiClient.post<ApiBodyResponse<MessageResponse>>(
    `${LOAN_APPLICATIONS_API}/${id}/release`,
  )
  return unwrap(response)
}
