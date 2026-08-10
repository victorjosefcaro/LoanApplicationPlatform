// ─── Shared API types (global ambient) ──────────────────────────────────────
type ApiBodyResponse<T> = {
  data?: T
  error?: { type: string; message?: string }
  errors?: Record<string, string[]>
  title?: string
}

type ApiResponse<T> = {
  data: T
  status: number
  statusText: string
  headers: Record<string, string>
}

// Standard shape for endpoints that only return a status message (e.g. the
// 204 No Content mutations: update, cancel, review, approve, release, ...).
type MessageResponse = {
  message: string
}

// Serialized value of the `X-Pagination` response header returned by paged
// endpoints (e.g. GET /loanapplications, GET /treasury/transactions).
type PaginationMetadata = {
  totalCount: number
  pageSize: number
  currentPage: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// Paged endpoints return the items as the JSON body and the pagination
// metadata in the `X-Pagination` header; this stitches them back together.
type PagedResult<T> = {
  items: T[]
  pagination: PaginationMetadata | null
}
