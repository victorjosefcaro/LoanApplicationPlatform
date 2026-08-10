import { useMutation, useQueryClient } from '@tanstack/react-query'
import { depositFunds } from './treasury.api'
import { treasuryKeys } from './treasury.keys'

export const useDepositFunds = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: depositFunds,
    onSuccess: () => qc.invalidateQueries({ queryKey: treasuryKeys.all }),
  })
}
