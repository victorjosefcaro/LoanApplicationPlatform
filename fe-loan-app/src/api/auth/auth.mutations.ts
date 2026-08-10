import { useMutation } from '@tanstack/react-query'
import { STORAGE_KEYS } from '@/constants'
import { login, register, registerAdminUser } from './auth.api'

export const useLogin = () =>
  useMutation({
    mutationFn: login,
    onSuccess: (token) => {
      try {
        localStorage.setItem(STORAGE_KEYS.AUTH.TOKEN, token)
      } catch {
        // Ignore storage failures; the token is still returned to the caller.
      }
    },
  })

export const useRegister = () => useMutation({ mutationFn: register })

export const useRegisterAdminUser = () => useMutation({ mutationFn: registerAdminUser })
