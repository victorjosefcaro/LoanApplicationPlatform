import { useState, type ChangeEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { FiEdit2, FiKey, FiUserCheck, FiUserPlus, FiUserX } from 'react-icons/fi'
import { useUsers } from '@/api/users/users.queries'
import {
  useResetUserPassword,
  useSetUserActive,
  useUpdateUserRole,
} from '@/api/users/users.mutations'
import type { User } from '@/api/users/users.types'
import { STAFF_ROLES } from '@/constants'
import { useAuth } from '@/auth/auth-context'
import PageHeader from '@/components/page-header'
import { DataTable, type ColumnDef, type ActionDef } from '@/components/shared'
import ModalDialog from '@/components/shared/components/modal-dialog'
import ConfirmDialog from '@/components/shared/components/confirm-dialog'
import InputField from '@/components/shared/components/input-field'
import { Badge } from '@/components/ui/badge'
import { InlineError, ErrorState } from '@/components/states'

const ROLE_OPTIONS = STAFF_ROLES.map((role) => ({ label: role, value: role }))

const PAGE_SIZE = 10

export const UserManagementPage = () => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const currentUserId = user?.userId ?? null

  const [page, setPage] = useState(1)
  const [searchTerm, setSearchTerm] = useState('')

  const [editing, setEditing] = useState<User | null>(null)
  const [roleDraft, setRoleDraft] = useState('')
  const [resetting, setResetting] = useState<User | null>(null)
  const [passwordDraft, setPasswordDraft] = useState('')
  const [deactivating, setDeactivating] = useState<User | null>(null)

  const query = useUsers({ pageNumber: page, pageSize: PAGE_SIZE, search: searchTerm || undefined })
  const items = query.data?.items ?? []
  const totalCount = query.data?.pagination?.totalCount ?? items.length

  const updateRole = useUpdateUserRole()
  const resetPassword = useResetUserPassword()
  const setActive = useSetUserActive()

  const handleSearch = (term: string) => {
    setSearchTerm(term)
    setPage(1)
  }

  const openEdit = (member: User) => {
    setRoleDraft(member.role)
    setEditing(member)
  }

  const saveRole = () => {
    if (!editing) return
    updateRole.mutate(
      { id: editing.id, payload: { role: roleDraft } },
      { onSuccess: () => setEditing(null) },
    )
  }

  const openReset = (member: User) => {
    setPasswordDraft('')
    setResetting(member)
  }

  const saveReset = () => {
    if (!resetting) return
    resetPassword.mutate(
      { id: resetting.id, payload: { password: passwordDraft } },
      { onSuccess: () => setResetting(null) },
    )
  }

  const columns: ColumnDef<User>[] = [
    { key: 'username', label: 'Username' },
    { key: 'role', label: 'Role' },
    {
      key: 'isActive',
      label: 'Status',
      render: (member) =>
        member.isActive ? (
          <Badge variant="secondary">Active</Badge>
        ) : (
          <Badge variant="destructive">Inactive</Badge>
        ),
    },
  ]

  const isSelf = (member: User) => currentUserId !== null && member.id === currentUserId

  const actions: ActionDef<User>[] = [
    {
      icon: <FiEdit2 />,
      label: 'Edit role',
      variant: 'ghost',
      onClick: openEdit,
      show: (member) => !isSelf(member),
    },
    {
      icon: <FiKey />,
      label: 'Reset password',
      variant: 'ghost',
      onClick: openReset,
    },
    {
      icon: <FiUserX />,
      label: 'Deactivate',
      variant: 'destructive',
      onClick: setDeactivating,
      show: (member) => member.isActive && !isSelf(member),
    },
    {
      icon: <FiUserCheck />,
      label: 'Activate',
      variant: 'outline',
      onClick: (member) => setActive.mutate({ id: member.id, isActive: true }),
      show: (member) => !member.isActive,
    },
  ]

  return (
    <div>
      <PageHeader title="Team members" subtitle="Manage the people who can access the platform." />

      {query.isError ? (
        <ErrorState error={query.error} onRetry={query.refetch} />
      ) : (
        <DataTable<User>
          data={items}
          columns={columns}
          actions={actions}
          searchTerm={searchTerm}
          setSearchTerm={handleSearch}
          searchPlaceholder="Search by username…"
          showAddButton={false}
          buttons={[
            {
              label: (
                <span className="inline-flex items-center gap-2">
                  <FiUserPlus className="size-4" /> Add team member
                </span>
              ),
              onClick: () => navigate('/admin/register'),
            },
          ]}
          isLoading={query.isLoading}
          emptyMessage="No team members yet."
          page={page}
          totalCount={totalCount}
          pageSize={PAGE_SIZE}
          onPageChange={setPage}
        />
      )}

      {/* Edit role */}
      <ModalDialog
        open={editing !== null}
        onOpenChange={(next) => !next && setEditing(null)}
        title="Edit role"
        desc={editing ? `Change the role for ${editing.username}.` : undefined}
        size="sm"
        actionButton={[
          {
            type: 'button',
            variant: 'ghost',
            value: 'Cancel',
            onClick: () => setEditing(null),
            disabled: updateRole.isPending,
          },
          {
            type: 'button',
            variant: 'default',
            value: updateRole.isPending ? 'Saving…' : 'Save role',
            onClick: saveRole,
            disabled: updateRole.isPending || !roleDraft,
          },
        ]}
      >
        <InputField
          label="Role"
          fieldType="select"
          value={roleDraft}
          onValueChange={setRoleDraft}
          options={ROLE_OPTIONS}
          isRequired
        />
        {updateRole.isError && <InlineError error={updateRole.error} />}
      </ModalDialog>

      {/* Reset password */}
      <ModalDialog
        open={resetting !== null}
        onOpenChange={(next) => !next && setResetting(null)}
        title="Reset password"
        desc={resetting ? `Set a new temporary password for ${resetting.username}.` : undefined}
        size="sm"
        actionButton={[
          {
            type: 'button',
            variant: 'ghost',
            value: 'Cancel',
            onClick: () => setResetting(null),
            disabled: resetPassword.isPending,
          },
          {
            type: 'button',
            variant: 'default',
            value: resetPassword.isPending ? 'Saving…' : 'Reset password',
            onClick: saveReset,
            disabled: resetPassword.isPending || !passwordDraft,
          },
        ]}
      >
        <InputField
          label="New temporary password"
          fieldType="password"
          value={passwordDraft}
          onChange={(e: ChangeEvent<HTMLInputElement>) => setPasswordDraft(e.target.value)}
          isRequired
          placeholder="At least 8 characters"
        />
        {resetPassword.isError && <InlineError error={resetPassword.error} />}
      </ModalDialog>

      {/* Deactivate confirmation */}
      <ConfirmDialog
        open={deactivating !== null}
        onOpenChange={(next) => !next && setDeactivating(null)}
        title="Deactivate this member?"
        description={
          deactivating
            ? `${deactivating.username} will be blocked from signing in until reactivated.`
            : undefined
        }
        confirmLabel="Deactivate"
        pendingLabel="Deactivating…"
        confirmVariant="destructive"
        pending={setActive.isPending}
        error={setActive.isError ? setActive.error : undefined}
        onConfirm={() =>
          deactivating &&
          setActive.mutate(
            { id: deactivating.id, isActive: false },
            { onSuccess: () => setDeactivating(null) },
          )
        }
      />
    </div>
  )
}

export default UserManagementPage
