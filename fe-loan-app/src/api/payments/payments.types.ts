export type PaymentSchedule = {
  id: number
  loanApplicationId: number
  dueDate: string
  amountDue: number
  amountPaid: number
  submittedAmount?: number | null
  status: string
} & AuditFields

export type PaymentRequest = {
  amount: number
}
