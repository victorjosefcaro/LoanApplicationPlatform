import { useQuery } from '@tanstack/react-query'
import { getPaymentSchedules } from './payments.api'
import { paymentKeys } from './payments.keys'

export const usePaymentSchedules = (loanApplicationId: number, options?: { enabled?: boolean }) =>
  useQuery({
    queryKey: paymentKeys.list(loanApplicationId),
    queryFn: () => getPaymentSchedules(loanApplicationId),
    enabled: Number.isFinite(loanApplicationId) && (options?.enabled ?? true),
  })
