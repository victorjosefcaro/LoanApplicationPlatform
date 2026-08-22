// Shared audit fields, mixed into persisted entity types (`SomeEntity & AuditFields`).
type AuditFields = {
  createdByUserId?: string | number | null
  updatedByUserId?: string | number | null
  createdUserName?: string | null
  updatedUserName?: string | null
  createdDate?: string | null
  updatedDate?: string | null
  isActive?: boolean
}
