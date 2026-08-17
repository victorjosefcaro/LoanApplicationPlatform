import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import { STORAGE_KEYS, isStaffRole } from '@/constants'
import { decodeJwt, isTokenValid } from './jwt'

export type AuthUser = {
  userId: number | null
  role: string
  username: string
  tenantId: number | null
}

type AuthContextValue = {
  user: AuthUser | null
  isStaff: boolean
  signIn: (token: string, username: string) => AuthUser | null
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

const KEYS = STORAGE_KEYS.AUTH

const readStoredUser = (): AuthUser | null => {
  try {
    const token = localStorage.getItem(KEYS.TOKEN)
    if (!token) return null
    const claims = decodeJwt(token)
    if (!isTokenValid(claims) || !claims) {
      localStorage.removeItem(KEYS.TOKEN)
      return null
    }
    return {
      userId: claims.userId,
      role: claims.role as string,
      username: localStorage.getItem(KEYS.USERNAME) ?? '',
      tenantId: claims.tenantId,
    }
  } catch {
    return null
  }
}

const persist = (token: string, user: AuthUser): void => {
  try {
    localStorage.setItem(KEYS.TOKEN, token)
    localStorage.setItem(KEYS.ROLE, user.role)
    localStorage.setItem(KEYS.USERNAME, user.username)
    if (user.userId !== null) localStorage.setItem(KEYS.USER_ID, String(user.userId))
  } catch {
    // Storage can fail (private mode); the in-memory session still works.
  }
}

const clear = (): void => {
  try {
    Object.values(KEYS).forEach((key) => localStorage.removeItem(key))
  } catch {
    // ignore
  }
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<AuthUser | null>(readStoredUser)

  const signIn = useCallback((token: string, username: string): AuthUser | null => {
    const claims = decodeJwt(token)
    if (!isTokenValid(claims) || !claims) return null
    const next: AuthUser = {
      userId: claims.userId,
      role: claims.role as string,
      username,
      tenantId: claims.tenantId,
    }
    persist(token, next)
    setUser(next)
    return next
  }, [])

  const signOut = useCallback(() => {
    clear()
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({ user, isStaff: isStaffRole(user?.role), signIn, signOut }),
    [user, signIn, signOut],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within an AuthProvider')
  return context
}

/** Where a role lands after login / when hitting a route it can't access. */
export const homePathForRole = (role: string | null | undefined): string =>
  isStaffRole(role) ? '/admin' : '/'
