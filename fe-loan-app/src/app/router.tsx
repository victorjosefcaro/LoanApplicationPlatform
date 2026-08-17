import { type ReactNode } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { STAFF_ROLES, ROLES } from '@/constants'
import { RequireAuth, RedirectIfSignedIn } from '@/auth/require-auth'
import { AppShell } from '@/components/layout/app-shell'

import { LoginPage } from '@/pages/auth/login'
import { RegisterPage } from '@/pages/auth/register'
import { AdminRegisterPage } from '@/pages/auth/admin-register'

import { DashboardPage } from '@/pages/applicant/dashboard'
import { LoansPage } from '@/pages/applicant/loans'
import { LoanNewPage } from '@/pages/applicant/loan-new'
import { LoanDetailPage } from '@/pages/applicant/loan-detail'
import { LoanEditPage } from '@/pages/applicant/loan-edit'
import { LoanPayPage } from '@/pages/applicant/loan-pay'

import { ReviewQueuePage } from '@/pages/admin/review-queue'
import { AdminLoanReviewPage } from '@/pages/admin/loan-review'
import { AdminLoanPaymentsPage } from '@/pages/admin/loan-payments'
import { TreasuryPage } from '@/pages/admin/treasury'

import { ComponentsGalleryPage } from '@/pages/dev/components-gallery'

const applicantOnly = (element: ReactNode) => (
  <RequireAuth roles={[ROLES.APPLICANT]}>{element}</RequireAuth>
)
const staffOnly = (element: ReactNode) => (
  <RequireAuth roles={STAFF_ROLES}>{element}</RequireAuth>
)

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

  // Applicant area
  {
    element: applicantOnly(<AppShell />),
    children: [
      { path: '/', element: <DashboardPage /> },
      { path: '/loans', element: <LoansPage /> },
      { path: '/loans/new', element: <LoanNewPage /> },
      { path: '/loans/:id', element: <LoanDetailPage /> },
      { path: '/loans/:id/edit', element: <LoanEditPage /> },
      { path: '/loans/:id/pay', element: <LoanPayPage /> },
    ],
  },

  // Staff (admin / reviewer / approver) area
  {
    element: staffOnly(<AppShell />),
    children: [
      { path: '/admin', element: <ReviewQueuePage /> },
      { path: '/admin/register', element: <AdminRegisterPage /> },
      { path: '/admin/loans/:id', element: <AdminLoanReviewPage /> },
      { path: '/admin/loans/:id/payments', element: <AdminLoanPaymentsPage /> },
      { path: '/admin/treasury', element: <TreasuryPage /> },
    ],
  },

  { path: '*', element: <Navigate to="/" replace /> },
])
