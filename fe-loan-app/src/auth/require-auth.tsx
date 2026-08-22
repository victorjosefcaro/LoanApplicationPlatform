import { type ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth, homePathForRole } from './auth-context'
import { UnauthorizedPage } from '@/pages/unauthorized'

type RequireAuthProps = {
  /** If set, the user's role must be one of these. Otherwise any signed-in user. */
  roles?: string[]
  children: ReactNode
}

/** Route guard: bounces signed-out users to /login and wrong-role users home. */
export const RequireAuth = ({ roles, children }: RequireAuthProps) => {
  const { user } = useAuth()
  const location = useLocation()

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (roles && !roles.includes(user.role)) {
    return <UnauthorizedPage />
  }

  return <>{children}</>
}

/** For /login and /register: send already-signed-in users to their home. */
export const RedirectIfSignedIn = ({ children }: { children: ReactNode }) => {
  const { user } = useAuth()
  if (user) return <Navigate to={homePathForRole(user.role)} replace />
  return <>{children}</>
}
