// ─── Shared audit fields (global ambient) ────────────────────────────────────
// Mixed into persisted entity types, mirroring the reference `src/types` usage
// where record shapes are declared as `SomeEntity & AuditFields`.

type AuditFields = {
  createdByUserId?: string | number | null
  updatedByUserId?: string | number | null
  createdUserName?: string | null
  updatedUserName?: string | null
  createdDate?: string | null
  updatedDate?: string | null
  isActive?: boolean
}
