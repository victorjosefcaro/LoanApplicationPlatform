import { useState, type ChangeEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useLogin, useRegister } from '@/api/auth/auth.mutations'
import { useAuth } from '@/auth/auth-context'
import InputField from '@/components/shared/components/input-field'
import { Button } from '@/components/ui/button'
import { Spinner, InlineError } from '@/components/states'
import { checkPassword } from '@/utils/password'
import { AuthLayout } from './auth-layout'

export const RegisterPage = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  const register = useRegister()
  const login = useLogin()
  const { signIn } = useAuth()
  const navigate = useNavigate()

  const mismatch = confirmPassword.length > 0 && password !== confirmPassword
  const passwordCheck = checkPassword(password)
  const pending = register.isPending || login.isPending

  const handleSubmit = (e: ChangeEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!username.trim() || !passwordCheck.isValid) return
    if (!confirmPassword || password !== confirmPassword) return

    const creds = { username: username.trim(), password }
    register.mutate(creds, {
      onSuccess: () => {
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

        {password.length > 0 && (
          <ul className="grid grid-cols-1 gap-1 sm:grid-cols-2">
            {passwordCheck.results.map((rule) => (
              <li
                key={rule.id}
                className={`flex items-center gap-2 text-sm ${
                  rule.passed ? 'text-green-600' : 'text-muted-foreground'
                }`}
              >
                <span aria-hidden="true">{rule.passed ? '✓' : '○'}</span>
                {rule.label}
              </li>
            ))}
          </ul>
        )}

        <InputField
          label="Confirm password"
          fieldType="password"
          size="xl"
          value={confirmPassword}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setConfirmPassword(e.target.value)}
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
          disabled={
            pending || !username.trim() || !passwordCheck.isValid || !confirmPassword || mismatch
          }
        >
          {pending ? <Spinner className="size-4" /> : 'Create account'}
        </Button>
      </form>
    </AuthLayout>
  )
}

export default RegisterPage
