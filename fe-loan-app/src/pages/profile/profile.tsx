import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import { FiCheckCircle } from 'react-icons/fi'
import { useAuth } from '@/auth/auth-context'
import { useProfile } from '@/hooks/profile/use-profile'
import PageHeader from '@/components/page-header'
import Card from '@/components/shared/components/card'
import InputField from '@/components/shared/components/input-field'
import { Button } from '@/components/ui/button'

export const ProfilePage = () => {
  const { user } = useAuth()
  const { fullName, setFullName } = useProfile()

  const [draft, setDraft] = useState(fullName)
  const [saved, setSaved] = useState(false)

  // Keep the draft aligned with the stored value (e.g. loaded from storage).
  useEffect(() => {
    setDraft(fullName)
  }, [fullName])

  const dirty = draft.trim() !== fullName.trim()

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    setFullName(draft.trim())
    setSaved(true)
  }

  return (
    <div className="max-w-lg">
      <PageHeader title="My profile" subtitle="Manage your personal details." />

      {saved && !dirty && (
        <div className="mb-4 flex items-center gap-2 rounded-xl bg-brand-tint px-4 py-3 text-sm font-medium text-brand-deep">
          <FiCheckCircle className="size-5 shrink-0" />
          Profile saved.
        </div>
      )}

      <Card
        content={
          <form onSubmit={handleSubmit} className="space-y-4" noValidate>
            <InputField
              label="Full name"
              value={draft}
              onChange={(e: ChangeEvent<HTMLInputElement>) => {
                setDraft(e.target.value)
                setSaved(false)
              }}
              placeholder="e.g. Juan Dela Cruz"
            />
            <InputField label="Username" value={user?.username ?? ''} readOnly />
            <InputField label="Role" value={user?.role ?? ''} readOnly />

            <Button type="submit" disabled={!dirty}>
              Save changes
            </Button>
          </form>
        }
      />
    </div>
  )
}

export default ProfilePage
