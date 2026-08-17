import { type ReactNode } from 'react'
import { cn } from '@/lib/utils'

type PageHeaderProps = {
  title: string
  subtitle?: ReactNode
  actions?: ReactNode
  className?: string
}

export const PageHeader = ({ title, subtitle, actions, className }: PageHeaderProps) => (
  <div className={cn('mb-6 flex flex-wrap items-end justify-between gap-4', className)}>
    <div className="min-w-0 text-black">
      <h1 className="font-heading text-2xl font-extrabold sm:text-3xl">{title}</h1>
      {subtitle && <p className="mt-1 text-sm">{subtitle}</p>}
    </div>
    {actions && <div className="flex shrink-0 items-center gap-2">{actions}</div>}
  </div>
)

export default PageHeader
