import { useMutation, useQueryClient } from '@tanstack/react-query'
import { resetUserPassword, setUserActive, updateUserRole } from './users.api'
import { userKeys } from './users.keys'
import type { ResetUserPasswordRequest, UpdateUserRoleRequest } from './users.types'

export const useUpdateUserRole = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; payload: UpdateUserRoleRequest }) =>
      updateUserRole(vars.id, vars.payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  })
}

export const useSetUserActive = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; isActive: boolean }) => setUserActive(vars.id, vars.isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  })
}

export const useResetUserPassword = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: number; payload: ResetUserPasswordRequest }) =>
      resetUserPassword(vars.id, vars.payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  })
}
