/** True when an API error is a 404. */
export const isNotFoundError = (error: unknown): boolean =>
  (error as { response?: { status?: number } } | null)?.response?.status === 404

// Throw from a route (`throw notFound()`) so the layout's errorElement renders
// the full-page Not Found, escaping the AppShell (no sidebar/topbar).
export const notFound = (): Response => new Response('Not Found', { status: 404 })
