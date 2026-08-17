export const STORAGE_KEYS = {
  AUTH: {
    TOKEN: 'lap.auth.token',
    ROLE: 'lap.auth.role',
    USER_ID: 'lap.auth.userId',
    USERNAME: 'lap.auth.username',
  },
} as const

export const ROLES = {
  APPLICANT: 'Applicant',
  ADMIN: 'Admin',
  REVIEWER: 'Reviewer',
  APPROVER: 'Approver',
} as const

// Back office roles; applicants are everyone else.
export const STAFF_ROLES: string[] = [ROLES.ADMIN, ROLES.REVIEWER, ROLES.APPROVER]

export const isStaffRole = (role: string | null | undefined): boolean =>
  !!role && STAFF_ROLES.includes(role)

export type Tone = 'info' | 'progress' | 'success' | 'warning' | 'danger' | 'muted'

export const LOAN_STATUS_META: Record<string, { label: string; tone: Tone }> = {
  Submitted: { label: 'Submitted', tone: 'info' },
  Returned: { label: 'Returned', tone: 'warning' },
  Reviewed: { label: 'Under review', tone: 'progress' },
  Approved: { label: 'Approved', tone: 'success' },
  Rejected: { label: 'Rejected', tone: 'danger' },
  Released: { label: 'Funds released', tone: 'success' },
  Cancelled: { label: 'Cancelled', tone: 'muted' },
  Completed: { label: 'Completed', tone: 'success' },
}

export const PAYMENT_STATUS_META: Record<string, { label: string; tone: Tone }> = {
  Pending: { label: 'Pending', tone: 'muted' },
  PaymentSubmitted: { label: 'Submitted', tone: 'info' },
  PartiallyPaid: { label: 'Partially paid', tone: 'progress' },
  Paid: { label: 'Paid', tone: 'success' },
}

export const loanStatusMeta = (status: string): { label: string; tone: Tone } =>
  LOAN_STATUS_META[status] ?? { label: status, tone: 'muted' }

export const paymentStatusMeta = (status: string): { label: string; tone: Tone } =>
  PAYMENT_STATUS_META[status] ?? { label: status, tone: 'muted' }

// Applicant can edit/cancel only before review.
export const EDITABLE_LOAN_STATUSES: string[] = ['Submitted', 'Returned']

export const isEditableLoan = (status: string): boolean => EDITABLE_LOAN_STATUSES.includes(status)

export const ANNUAL_INTEREST_RATE = 0.12

const TERM_MONTHS = [6, 12, 18, 24, 36, 48, 60]

const yearsLabel = (months: number): string => {
  const years = months / 12
  const value = Number.isInteger(years) ? years : years.toFixed(1)
  return `${value} ${years === 1 ? 'yr' : 'yrs'}`
}

export const TERM_OPTIONS: { label: string; value: string }[] = TERM_MONTHS.map((months) => ({
  label: `${months} months (${yearsLabel(months)})`,
  value: String(months),
}))

export const PURPOSE_OPTIONS: { label: string; value: string }[] = [
  { label: 'Home improvement', value: 'Home improvement' },
  { label: 'Business', value: 'Business' },
  { label: 'Education', value: 'Education' },
  { label: 'Medical', value: 'Medical' },
  { label: 'Debt consolidation', value: 'Debt consolidation' },
  { label: 'Personal', value: 'Personal' },
  { label: 'Other', value: 'Other' },
]
