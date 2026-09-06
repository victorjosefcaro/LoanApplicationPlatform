import { type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { STAFF_ROLES, ROLES } from '@/constants'
import { RequireAuth, RedirectIfSignedIn } from '@/auth/require-auth'
import { AppShell } from '@/components/layout/app-shell'

import { LoginPage } from '@/pages/auth/login'
import { RegisterPage } from '@/pages/auth/register'
import { AdminRegisterPage } from '@/pages/auth/admin-register'
import { UserManagementPage } from '@/pages/admin/user-management'

import { DashboardPage } from '@/pages/applicant/dashboard'
import { LoansPage } from '@/pages/applicant/loans'

import { ReviewQueuePage } from '@/pages/admin/review-queue'
import { AdminLoanReviewPage } from '@/pages/admin/loan-review'
import { AdminLoanPaymentsPage } from '@/pages/admin/loan-payments'
import { TreasuryPage } from '@/pages/admin/treasury'

import { ProfilePage } from '@/pages/profile/profile'

import { ComponentsGalleryPage } from '@/pages/dev/components-gallery'
import { NotFoundPage } from '@/pages/not-found'
import { RouteErrorBoundary } from '@/components/route-error-boundary'

const applicantOnly = (element: ReactNode) => (
  <RequireAuth roles={[ROLES.APPLICANT]}>{element}</RequireAuth>
)
const staffOnly = (element: ReactNode) => <RequireAuth roles={STAFF_ROLES}>{element}</RequireAuth>
const adminOnly = (element: ReactNode) => <RequireAuth roles={[ROLES.ADMIN]}>{element}</RequireAuth>

export const router = createBrowserRouter([
  {
    path: '/login',
    element: (
      <RedirectIfSignedIn>
        <LoginPage />
      </RedirectIfSignedIn>
    ),
  },
  {
    path: '/register',
    element: (
      <RedirectIfSignedIn>
        <RegisterPage />
      </RedirectIfSignedIn>
    ),
  },
  {
    path: '/dev/components',
    element: <ComponentsGalleryPage />,
  },

  {
    element: applicantOnly(<AppShell />),
    errorElement: <RouteErrorBoundary />,
    children: [
      { path: '/', element: <DashboardPage /> },
      { path: '/loans', element: <LoansPage /> },
    ],
  },

  {
    element: (
      <RequireAuth>
        <AppShell />
      </RequireAuth>
    ),
    errorElement: <RouteErrorBoundary />,
    children: [{ path: '/profile', element: <ProfilePage /> }],
  },

  {
    element: staffOnly(<AppShell />),
    errorElement: <RouteErrorBoundary />,
    children: [
      { path: '/admin', element: <ReviewQueuePage /> },
      { path: '/admin/register', element: adminOnly(<AdminRegisterPage />) },
      { path: '/admin/users', element: adminOnly(<UserManagementPage />) },
      { path: '/admin/loans/:id', element: <AdminLoanReviewPage /> },
      { path: '/admin/loans/:id/payments', element: <AdminLoanPaymentsPage /> },
      { path: '/admin/treasury', element: <TreasuryPage /> },
    ],
  },

  { path: '*', element: <NotFoundPage /> },
])
