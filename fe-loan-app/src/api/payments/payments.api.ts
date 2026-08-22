import { apiClient } from '@/api/client'
import { unwrap } from '@/api/unwrap'
import type { PaymentRequest, PaymentSchedule } from './payments.types'

const paymentsPath = (loanApplicationId: number): string =>
  `/loanapplications/${loanApplicationId}/payments`

export const getPaymentSchedules = async (
  loanApplicationId: number,
): Promise<PaymentSchedule[]> => {
  const response = await apiClient.get<ApiBodyResponse<PaymentSchedule[]>>(
    paymentsPath(loanApplicationId),
  )
  return unwrap(response)
}

// Applicant-initiated payment; amount defaults to the scheduled amount due when omitted.
export const submitPayment = async (
  loanApplicationId: number,
  scheduleId: number,
  payload?: PaymentRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.post<ApiBodyResponse<MessageResponse>>(
    `${paymentsPath(loanApplicationId)}/${scheduleId}/submit`,
    payload,
  )
  return unwrap(response)
}

// Admin-initiated posting against a schedule.
export const postPayment = async (
  loanApplicationId: number,
  scheduleId: number,
  payload: PaymentRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.post<ApiBodyResponse<MessageResponse>>(
    `${paymentsPath(loanApplicationId)}/${scheduleId}/post`,
    payload,
  )
  return unwrap(response)
}
