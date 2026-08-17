import { isRouteErrorResponse, useRouteError } from 'react-router-dom'
import { NotFoundPage } from '@/pages/not-found'
import { ErrorState } from '@/components/states'

// errorElement for the layout routes: renders full-viewport (no AppShell) when
// a child route throws. 404s show Not Found; anything else a full-page error.
export const RouteErrorBoundary = () => {
  const error = useRouteError()

  // A 404 arrives either as a raw Response (our notFound()) or a router ErrorResponse.
  const isNotFound =
    (error instanceof Response && error.status === 404) ||
    (isRouteErrorResponse(error) && error.status === 404)

  if (isNotFound) {
    return <NotFoundPage />
  }

  return (
    <div className="flex min-h-dvh items-center justify-center bg-bg px-4 py-10">
      <ErrorState error={error} className="max-w-md" />
    </div>
  )
}

export default RouteErrorBoundary
