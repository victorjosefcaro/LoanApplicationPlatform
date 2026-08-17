import { useMemo, useState, type ChangeEvent, type FormEvent, type ReactNode } from 'react'
import type { LoanApplicationCreateRequest } from '@/api/loan-applications/loan-applications.types'
import { TERM_OPTIONS, PURPOSE_OPTIONS } from '@/constants'
import { estimateMonthlyPayment, estimateTotalRepayment } from '@/utils/finance'
import InputField from '@/components/shared/components/input-field'
import { Button } from '@/components/ui/button'
import { Money } from '@/components/money/money'
import { Spinner, InlineError } from '@/components/states'
import { getErrorMessage } from '@/utils/errors'
import DataMap from '@/utils/data-map'

export type LoanFormValues = {
  applicantName: string
  amount: string
  termInMonths: string
  monthlyIncome: string
  purpose: string
}

const EMPTY: LoanFormValues = {
  applicantName: '',
  amount: '',
  termInMonths: '12',
  monthlyIncome: '',
  purpose: '',
}

type LoanFormProps = {
  initial?: Partial<LoanFormValues>
  submitLabel: string
  pending?: boolean
  error?: unknown
  onSubmit: (payload: LoanApplicationCreateRequest) => void
  onCancel?: () => void
}

export const LoanForm = ({
  initial,
  submitLabel,
  pending,
  error,
  onSubmit,
  onCancel,
}: LoanFormProps) => {
  const [values, setValues] = useState<LoanFormValues>({ ...EMPTY, ...initial })

  const set =
    (key: keyof LoanFormValues) =>
    (input: string | ChangeEvent<HTMLInputElement>): void => {
      const value = typeof input === 'string' ? input : input.target.value
      setValues((prev) => ({ ...prev, [key]: value }))
    }

  const amount = Number(values.amount)
  const term = Number(values.termInMonths)
  const income = Number(values.monthlyIncome)

  const monthly = useMemo(() => estimateMonthlyPayment(amount, term), [amount, term])
  const total = useMemo(() => estimateTotalRepayment(amount, term), [amount, term])

  const summary: { label: string; value: ReactNode }[] = [
    { label: 'Total repayment', value: <Money amount={total > 0 ? total : 0} /> },
    { label: 'Over', value: term > 0 ? `${term} months` : '—' },
  ]

  const fields: {
    key: keyof LoanFormValues
    label: string
    fieldType?: 'text' | 'number' | 'select'
    placeholder?: string
    options?: { label: string; value: string }[]
    isReadOnly?: boolean
  }[] = [
    {
      key: 'applicantName',
      label: 'Full name',
      placeholder: 'As it appears on your ID',
      isReadOnly: true,
    },
    { key: 'amount', label: 'Amount (₱)', fieldType: 'number', placeholder: '250000' },
    { key: 'termInMonths', label: 'Term', fieldType: 'select', options: TERM_OPTIONS },
    {
      key: 'monthlyIncome',
      label: 'Monthly income (₱)',
      fieldType: 'number',
      placeholder: '60000',
    },
    {
      key: 'purpose',
      label: 'Purpose',
      fieldType: 'select',
      options: PURPOSE_OPTIONS,
      placeholder: "What's this loan for?",
    },
  ]

  const message = error ? getErrorMessage(error) : ''
  const incomeError = message.toLowerCase().includes('income')
  const fieldErrors: Partial<Record<keyof LoanFormValues, string>> = incomeError
    ? { monthlyIncome: message }
    : {}

  const nameFromAccount = fields.find((field) => field.key === 'applicantName')?.isReadOnly
  if (nameFromAccount && values.applicantName.trim().length === 0) {
    fieldErrors.applicantName =
      "We couldn't read your account name. Please refresh or sign in again."
  }

  // Loan amount must exceed one month's income.
  const amountBelowIncome = amount > 0 && income > 0 && amount < income
  const amountEqualsIncome = amount > 0 && income > 0 && amount === income
  if (amountBelowIncome) {
    fieldErrors.amount = 'Loan amount must be greater than your monthly income.'
  } else if (amountEqualsIncome) {
    fieldErrors.amount = 'Loan amount cannot be equal to your monthly income.'
  }

  const valid =
    values.applicantName.trim().length > 0 &&
    amount > 0 &&
    term > 0 &&
    income > 0 &&
    !amountBelowIncome &&
    !amountEqualsIncome &&
    values.purpose.trim().length > 0

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    if (!valid) return
    onSubmit({
      applicantName: values.applicantName.trim(),
      amount,
      termInMonths: term,
      monthlyIncome: income,
      purpose: values.purpose.trim(),
    })
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-5 lg:grid-cols-[1fr_20rem]" noValidate>
      <div className="space-y-4">
        <DataMap
          data={fields}
          render={(field) => (
            <InputField
              key={field.key}
              id={field.key}
              name={field.key}
              label={field.label}
              fieldType={field.fieldType}
              value={values[field.key]}
              onChange={set(field.key)}
              onValueChange={set(field.key)}
              options={field.options}
              readOnly={field.isReadOnly}
              placeholder={field.placeholder}
              isRequired
              errorMessage={fieldErrors[field.key]}
            />
          )}
        />

        {error && !incomeError ? <InlineError error={error} /> : null}

        <div className="flex gap-3 pt-1">
          <Button type="submit" disabled={pending || !valid}>
            {pending ? <Spinner className="size-4" /> : submitLabel}
          </Button>
          {onCancel && (
            <Button type="button" variant="ghost" onClick={onCancel} disabled={pending}>
              Cancel
            </Button>
          )}
        </div>
      </div>

      <aside className="h-fit rounded-xl border border-line bg-brand-tint/50 p-5">
        <p className="text-sm font-bold text-brand-deep">Estimated monthly payment</p>
        <p className="mt-1 font-heading text-3xl font-extrabold text-ink">
          <Money amount={monthly > 0 ? monthly : 0} />
        </p>
        <dl className="mt-4 space-y-2 text-sm">
          <DataMap
            data={summary}
            render={({ label, value }) => (
              <div key={label} className="flex justify-between text-black">
                <dt className="">{label}</dt>
                <dd className="font-medium">{value}</dd>
              </div>
            )}
          />
        </dl>
        <p className="mt-4 text-xs text-black">
          An estimate at{' '}
          <span className="text-brand font-extrabold">{Math.round(0.12 * 100)}% p.a</span>. Your
          final schedule is set when the loan is approved.
        </p>
      </aside>
    </form>
  )
}

export default LoanForm
