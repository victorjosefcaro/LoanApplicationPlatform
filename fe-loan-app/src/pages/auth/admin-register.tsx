import { useState, type ChangeEvent, type FormEvent } from 'react'
import { FiCheckCircle } from 'react-icons/fi'
import { useRegisterAdminUser } from '@/api/auth/auth.mutations'
import { ROLES } from '@/constants'
import PageHeader from '@/components/page-header'
import InputField from '@/components/shared/components/input-field'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Spinner, InlineError } from '@/components/states'

const ROLE_OPTIONS = [
  { label: 'Admin', value: ROLES.ADMIN },
  { label: 'Reviewer', value: ROLES.REVIEWER },
  { label: 'Approver', value: ROLES.APPROVER },
]

export const AdminRegisterPage = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<string>(ROLES.REVIEWER)
  const [createdName, setCreatedName] = useState<string | null>(null)

  const registerAdmin = useRegisterAdminUser()

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const name = username.trim()
    registerAdmin.mutate(
      { username: name, password, role },
      {
        onSuccess: () => {
          setCreatedName(name)
          setUsername('')
          setPassword('')
          setRole(ROLES.REVIEWER)
        },
      },
    )
  }

  return (
    <div className="max-w-lg">
      <PageHeader
        title="Add a team member"
        subtitle="Create an account for a reviewer, approver, or admin."
      />

      {createdName && (
        <div className="mb-4 flex items-center gap-2 rounded-xl bg-brand-tint px-4 py-3 text-sm font-medium text-brand-deep">
          <FiCheckCircle className="size-5 shrink-0" />
          {createdName} can now sign in.
        </div>
      )}

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4" noValidate>
            <InputField
              label="Username"
              value={username}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
              isRequired
              placeholder="name@loanly.ph"
            />
            <InputField
              label="Temporary password"
              fieldType="password"
              value={password}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
              isRequired
              placeholder="At least 8 characters"
            />
            <InputField
              label="Role"
              fieldType="select"
              value={role}
              onValueChange={setRole}
              options={ROLE_OPTIONS}
              isRequired
            />

            {registerAdmin.isError && <InlineError error={registerAdmin.error} />}

            <Button
              type="submit"
              disabled={registerAdmin.isPending || !username.trim() || !password}
            >
              {registerAdmin.isPending ? <Spinner className="size-4" /> : 'Create account'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}

export default AdminRegisterPage
