import { useCallback, useEffect, useState } from 'react'
import { STORAGE_KEYS } from '@/constants'
import { useAuth, type AuthUser } from '@/auth/auth-context'

/** Stable per-user key so each account keeps its own full name in localStorage. */
const keyFor = (user: AuthUser | null): string | null => {
  if (!user) return null
  const id = user.userId !== null ? String(user.userId) : user.username
  if (!id) return null
  return `${STORAGE_KEYS.PROFILE_FULL_NAME_PREFIX}${id}`
}

const read = (key: string | null): string => {
  if (!key) return ''
  try {
    return localStorage.getItem(key) ?? ''
  } catch {
    return ''
  }
}

type UseProfile = {
  fullName: string
  setFullName: (value: string) => void
}

export const useProfile = (): UseProfile => {
  const { user } = useAuth()
  const key = keyFor(user)

  const [fullName, setFullNameState] = useState<string>(() => read(key))

  // Re-sync when the signed-in user changes.
  useEffect(() => {
    setFullNameState(read(key))
  }, [key])

  // Keep in sync across tabs/windows.
  useEffect(() => {
    if (!key) return
    const onStorage = (event: StorageEvent) => {
      if (event.key === key) setFullNameState(event.newValue ?? '')
    }
    window.addEventListener('storage', onStorage)
    return () => window.removeEventListener('storage', onStorage)
  }, [key])

  const setFullName = useCallback(
    (value: string) => {
      setFullNameState(value)
      if (!key) return
      try {
        if (value.trim()) localStorage.setItem(key, value)
        else localStorage.removeItem(key)
      } catch {
        // Storage can fail (private mode); in-memory value still applies.
      }
    },
    [key],
  )

  return { fullName, setFullName }
}

export default useProfile
