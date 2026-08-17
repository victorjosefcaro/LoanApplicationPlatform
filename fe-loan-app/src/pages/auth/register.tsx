import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useLogin, useRegister } from '@/api/auth/auth.mutations'
import { useAuth } from '@/auth/auth-context'
import InputField from '@/components/shared/components/input-field'
import { Button } from '@/components/ui/button'
import { Spinner, InlineError } from '@/components/states'
import { AuthLayout } from './auth-layout'

export const RegisterPage = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')

  const register = useRegister()
  const login = useLogin()
  const { signIn } = useAuth()
  const navigate = useNavigate()

  const mismatch = confirm.length > 0 && password !== confirm
  const pending = register.isPending || login.isPending

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    if (mismatch) return
    const creds = { username: username.trim(), password }
    register.mutate(creds, {
      onSuccess: () => {
        // Registration returns no token; sign in with the same creds.
        login.mutate(creds, {
          onSuccess: (token) => {
            signIn(token, creds.username)
            navigate('/', { replace: true })
          },
        })
      },
    })
  }

  return (
    <AuthLayout
      title="Create your account"
      subtitle="Apply for a loan and track every step."
      footer={
        <span>
          Already have an account?{' '}
          <Link to="/login" className="font-semibold text-brand hover:underline">
            Sign in
          </Link>
        </span>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        <InputField
          label="Username"
          size="xl"
          value={username}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
          isRequired
          placeholder="you@example.com"
        />
        <InputField
          label="Password"
          fieldType="password"
          size="xl"
          value={password}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
          isRequired
          placeholder="At least 8 characters"
        />
        <InputField
          label="Confirm password"
          fieldType="password"
          size="xl"
          value={confirm}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setConfirm(e.target.value)}
          isRequired
          placeholder="Re-enter your password"
          errorMessage={mismatch ? 'Passwords don’t match yet.' : undefined}
        />

        {register.isError && <InlineError error={register.error} />}
        {login.isError && <InlineError error={login.error} />}

        <Button
          type="submit"
          size="xl"
          className="w-full"
          disabled={pending || !username.trim() || !password || mismatch}
        >
          {pending ? <Spinner className="size-4" /> : 'Create account'}
        </Button>
      </form>
    </AuthLayout>
  )
}

export default RegisterPage
