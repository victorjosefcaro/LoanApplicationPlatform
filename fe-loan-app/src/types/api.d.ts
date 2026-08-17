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

// Endpoints that only return a status message.
type MessageResponse = {
  message: string
}

// Value of the `X-Pagination` response header on paged endpoints.
type PaginationMetadata = {
  totalCount: number
  pageSize: number
  currentPage: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// Items (JSON body) stitched together with the X-Pagination metadata.
type PagedResult<T> = {
  items: T[]
  pagination: PaginationMetadata | null
}
