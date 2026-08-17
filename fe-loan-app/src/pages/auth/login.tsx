import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useLogin } from '@/api/auth/auth.mutations'
import { useAuth, homePathForRole } from '@/auth/auth-context'
import InputField from '@/components/shared/components/input-field'
import { Button } from '@/components/ui/button'
import { Spinner, InlineError } from '@/components/states'
import { AuthLayout } from './auth-layout'

export const LoginPage = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const login = useLogin()
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    login.mutate(
      { username: username.trim(), password },
      {
        onSuccess: (token) => {
          const user = signIn(token, username.trim())
          navigate(from ?? homePathForRole(user?.role), { replace: true })
        },
      },
    )
  }

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to pick up where you left off."
      footer={
        <span>
          New to Loanly?{' '}
          <Link to="/register" className="font-semibold text-brand hover:underline">
            Create an account
          </Link>
        </span>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        <InputField
          label="Username"
          value={username}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
          isRequired
          size='xl'
          placeholder="johndoe"
        />
        <InputField
          label="Password"
          fieldType="password"
          value={password}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
          isRequired
          size='xl'
          placeholder="Your password"
        />

        {login.isError && <InlineError error={login.error} />}

        <Button
          type="submit"
          size="xl"
          className="w-full"
          disabled={login.isPending || !username.trim() || !password}
        >
          {login.isPending ? <Spinner className="size-4" /> : 'Sign in'}
        </Button>
      </form>
    </AuthLayout>
  )
}

export default LoginPage
