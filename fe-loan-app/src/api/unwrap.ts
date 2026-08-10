export const unwrap = <T>(res: ApiResponse<ApiBodyResponse<T>>): T => {
  const body = res?.data
  if (!body) return body as T

  if (body.errors && typeof body.errors === 'object') {
    const firstKey = Object.keys(body.errors)[0]
    const firstMessage = body.errors[firstKey]?.[0]
    throw new Error(firstMessage || body.title || 'Validation error')
  }

  if (body.error && body.error.type !== 'NONE') {
    const type = body.error.type || ''
    if (type === 'FORBIDDEN') {
      throw new Error(body.error.message || 'You do not have permission to access this module.')
    }
    if (type === 'UNAUTHORIZED') {
      throw new Error(body.error.message || 'Authentication required. Please log in again.')
    }
    throw new Error(body.error.message || 'API request failed')
  }

  return body.data !== undefined ? body.data : (res.data as T)
}

// Paged endpoints return the items as the JSON body and the pagination
// metadata in the `X-Pagination` response header.
export const parsePagination = (headers: Record<string, string>): PaginationMetadata | null => {
  const raw = headers['x-pagination']
  if (!raw) return null
  try {
    return JSON.parse(raw) as PaginationMetadata
  } catch {
    return null
  }
}

export const unwrapPaged = <T>(res: ApiResponse<T[]>): PagedResult<T> => ({
  items: res.data ?? [],
  pagination: parsePagination(res.headers),
})
