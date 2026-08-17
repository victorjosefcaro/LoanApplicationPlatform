import { Link } from 'react-router-dom'
import { useAuth, homePathForRole } from '@/auth/auth-context'
import { Logo } from '@/components/brand/logo'
import { Button } from '@/components/ui/button'

export const UnauthorizedPage = () => {
  const { user } = useAuth()
  const homePath = user ? homePathForRole(user.role) : '/login'

  return (
    <div className="flex min-h-dvh flex-col items-center justify-center bg-bg px-4 py-10 text-center">
      <Logo img="/assets/logo.png" name="Loanly" size="lg" className="mb-8 w-auto!" />

      <p className="font-heading text-7xl font-extrabold text-brand sm:text-8xl">401</p>
      <h1 className="mt-4 font-heading text-4xl font-extrabold text-ink sm:text-5xl">
        Unauthorized
      </h1>
      <p className="mt-3 max-w-md text-lg text-black/70">
        You don’t have permission to access this page.
      </p>

      <Button render={<Link to={homePath} />} size="xl" className="mt-8">
        {user ? 'Back to home' : 'Go to sign in'}
      </Button>
    </div>
  )
}

export default UnauthorizedPage
