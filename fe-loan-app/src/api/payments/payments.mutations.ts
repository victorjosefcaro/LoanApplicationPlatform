import { useMutation, useQueryClient } from '@tanstack/react-query'
import { postPayment, submitPayment } from './payments.api'
import { paymentKeys } from './payments.keys'
import type { PaymentRequest } from './payments.types'

export const useSubmitPayment = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: {
      loanApplicationId: number
      scheduleId: number
      payload?: PaymentRequest
    }) => submitPayment(vars.loanApplicationId, vars.scheduleId, vars.payload),
    onSuccess: (_data, vars) =>
      qc.invalidateQueries({
        queryKey: paymentKeys.list(vars.loanApplicationId),
      }),
  })
}

export const usePostPayment = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: {
      loanApplicationId: number
      scheduleId: number
      payload: PaymentRequest
    }) => postPayment(vars.loanApplicationId, vars.scheduleId, vars.payload),
    onSuccess: (_data, vars) =>
      qc.invalidateQueries({
        queryKey: paymentKeys.list(vars.loanApplicationId),
      }),
  })
}
