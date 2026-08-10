export type LoanStatus =
  | 'Submitted'
  | 'Returned'
  | 'Reviewed'
  | 'Approved'
  | 'Rejected'
  | 'Released'
  | 'Cancelled'
  | 'Completed'

export type LoanApplication = {
  id: number
  applicantId: number
  applicantName: string
  amount: number
  termInMonths: number
  monthlyIncome: number
  purpose: string
  status: string
  remarks?: string | null
  createdAt: string
} & AuditFields

export type LoanApplicationStatusHistory = {
  id: number
  loanApplicationId: number
  previousStatus?: string | null
  newStatus: string
  remarks?: string | null
  changedByUserId: number
  changedByUsername?: string | null
  changedAt: string
} & AuditFields

export type LoanApplicationListParams = {
  pageNumber?: number
  pageSize?: number
  status?: LoanStatus | string
}

export type LoanApplicationCreateRequest = {
  applicantName: string
  amount: number
  termInMonths: number
  monthlyIncome: number
  purpose: string
}

export type LoanApplicationUpdateRequest = LoanApplicationCreateRequest

export type ReviewRequest = {
  status: 'Returned' | 'Reviewed' | 'Rejected'
  remarks?: string
}

export type ApproveRequest = {
  status: 'Approved' | 'Rejected' | 'Returned'
  remarks?: string
}
