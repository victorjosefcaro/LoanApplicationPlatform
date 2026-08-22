import { ANNUAL_INTEREST_RATE } from '@/constants'

// Amortized monthly payment — indicative estimate only; the backend owns the real schedule.
export const estimateMonthlyPayment = (
  amount: number,
  termInMonths: number,
  annualRate: number = ANNUAL_INTEREST_RATE,
): number => {
  if (!amount || !termInMonths || amount <= 0 || termInMonths <= 0) return 0
  const monthlyRate = annualRate / 12
  if (monthlyRate === 0) return amount / termInMonths
  const factor = Math.pow(1 + monthlyRate, termInMonths)
  return (amount * monthlyRate * factor) / (factor - 1)
}

export const estimateTotalRepayment = (amount: number, termInMonths: number): number =>
  estimateMonthlyPayment(amount, termInMonths) * termInMonths

// Ratio 0..1 of how much has been repaid.
export const repaymentProgress = (paid: number, total: number): number => {
  if (!total || total <= 0) return 0
  return Math.min(1, Math.max(0, paid / total))
}
