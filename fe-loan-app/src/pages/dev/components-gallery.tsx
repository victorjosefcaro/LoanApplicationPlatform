import { useState, type ChangeEvent, type ReactNode } from 'react'
import { Logo } from '@/components/brand/logo'
import { Button } from '@/components/ui/button'
import Card from '@/components/shared/components/card'
import { Money } from '@/components/money/money'
import { LoanStatusPill, PaymentStatusPill } from '@/components/status-pill'
import { LoanLifecycle } from '@/components/loan-lifecycle/loan-lifecycle'
import { ProgressBar } from '@/components/progress-bar'
import { EmptyState, ErrorState, Spinner } from '@/components/states'
import InputField from '@/components/shared/components/input-field'

const Section = ({ title, children }: { title: string; children: ReactNode }) => (
  <Card title={title} contentClassName="space-y-4" content={children} />
)

const LOAN_STATUSES = [
  'Submitted',
  'Returned',
  'Reviewed',
  'Approved',
  'Rejected',
  'Released',
  'Cancelled',
  'Completed',
]
const PAYMENT_STATUSES = ['Pending', 'PaymentSubmitted', 'PartiallyPaid', 'Paid']
const LIFECYCLE_STATES = ['Submitted', 'Reviewed', 'Approved', 'Released', 'Returned', 'Rejected']

export const ComponentsGalleryPage = () => {
  const [text, setText] = useState('')
  const [choice, setChoice] = useState('')

  return (
    <div className="mx-auto max-w-5xl space-y-6 p-6">
      <div className="flex items-center justify-between">
        <Logo img="/assets/logo.png" name="Loanly" size="xl" />
        <span className="rounded-md bg-brand-tint px-3 py-1 text-sm font-medium text-brand-deep">
          Component gallery
        </span>
      </div>

      <Section title="Buttons">
        <div className="flex flex-wrap gap-3">
          <Button>Primary</Button>
          <Button variant="gold">Gold</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="destructive">Danger</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="link">Link</Button>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Button size="sm">Small</Button>
          <Button>Default</Button>
          <Button size="lg">Large</Button>
          <Button disabled>Disabled</Button>
        </div>
      </Section>

      <Section title="Money">
        <div className="flex flex-wrap gap-6 text-lg">
          <Money amount={250000} />
          <Money amount={11458.33} />
          <Money amount={5000} signed />
          <Money amount={-1200} />
          <Money amount={null} />
        </div>
      </Section>

      <Section title="Status pills">
        <div className="flex flex-wrap gap-2">
          {LOAN_STATUSES.map((s) => (
            <LoanStatusPill key={s} status={s} />
          ))}
        </div>
        <div className="flex flex-wrap gap-2">
          {PAYMENT_STATUSES.map((s) => (
            <PaymentStatusPill key={s} status={s} />
          ))}
        </div>
      </Section>

      <Section title="LoanLifecycle">
        {LIFECYCLE_STATES.map((s) => (
          <div key={s} className="rounded-xl border border-line p-4">
            <p className="mb-3 text-xs font-medium text-brand uppercase">{s}</p>
            <LoanLifecycle status={s} />
          </div>
        ))}
      </Section>

      <Section title="Progress">
        <ProgressBar value={0.3} />
        <ProgressBar value={0.7} tone="gold" />
      </Section>

      <Section title="Inputs">
        <div className="grid gap-4 sm:grid-cols-2">
          <InputField
            label="Text"
            value={text}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setText(e.target.value)}
            placeholder="Type here"
          />
          <InputField label="Password" fieldType="password" value="" onChange={() => undefined} />
          <InputField
            label="Select"
            fieldType="select"
            value={choice}
            onValueChange={setChoice}
            options={[
              { label: 'One', value: '1' },
              { label: 'Two', value: '2' },
            ]}
            placeholder="Pick one"
          />
          <InputField label="Number" fieldType="number" value="" onChange={() => undefined} />
        </div>
      </Section>

      <Section title="States">
        <div className="flex items-center gap-3">
          <Spinner className="text-brand" /> <span className="text-sm text-brand">Loading…</span>
        </div>
        <EmptyState title="Nothing here yet" message="Empty states invite the next action." />
        <ErrorState error={new Error('Treasury balance is temporarily unavailable.')} />
      </Section>
    </div>
  )
}

export default ComponentsGalleryPage
