type WithResponse = {
  response?: { status?: number; data?: unknown }
}

// Turn any thrown value into a user-facing message.
export const getErrorMessage = (
  error: unknown,
  fallback = 'Something broke on our end. Try again in a moment.',
): string => {
  if (typeof error === 'string' && error.trim()) return error

  const withResponse = error as WithResponse
  const status = withResponse?.response?.status
  const data = withResponse?.response?.data

  // Prefer a specific, server-provided message when present.
  if (typeof data === 'string' && data.trim()) return data
  if (data && typeof data === 'object') {
    const detail = (data as Record<string, unknown>).detail
    if (typeof detail === 'string' && detail.trim()) return detail
  }

  // Map auth status codes to friendly guidance. The backend returns bare
  // ProblemDetails (e.g. title "Unauthorized"/"Forbidden") that aren't
  // user-facing, so fall back to these before using that boilerplate.
  if (status === 401) return 'Authentication required. Please log in again.'
  if (status === 403) return 'You do not have permission to access this module.'

  if (data && typeof data === 'object') {
    const record = data as Record<string, unknown>
    const detail = record.title ?? record.message
    if (typeof detail === 'string' && detail.trim()) return detail
  }

  if (error instanceof Error && error.message.trim()) return error.message
  return fallback
}

export const getErrorStatus = (error: unknown): number | null => {
  const status = (error as WithResponse)?.response?.status
  return typeof status === 'number' ? status : null
}
