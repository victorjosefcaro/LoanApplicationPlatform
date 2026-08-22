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
  const data = withResponse?.response?.data
  if (typeof data === 'string' && data.trim()) return data
  if (data && typeof data === 'object') {
    const record = data as Record<string, unknown>
    const detail = record.detail ?? record.title ?? record.message
    if (typeof detail === 'string' && detail.trim()) return detail
  }

  if (error instanceof Error && error.message.trim()) return error.message
  return fallback
}

export const getErrorStatus = (error: unknown): number | null => {
  const status = (error as WithResponse)?.response?.status
  return typeof status === 'number' ? status : null
}
