import { type ReactNode } from 'react'
import { Logo } from '@/components/brand/logo'

type AuthLayoutProps = {
  title: string
  subtitle?: string
  children: ReactNode
  footer?: ReactNode
}

export const AuthLayout = ({ title, subtitle, children, footer }: AuthLayoutProps) => (
  <div className="grid min-h-dvh lg:grid-cols-2">
    <div className="relative hidden flex-col justify-center bg-sidebar p-10 text-sidebar-foreground lg:flex">
      <div className="mx-auto flex w-full max-w-md flex-col items-start gap-5">
        <Logo img="/assets/logo.png" name="Loanly" size="xl" className="w-auto! justify-start!" />
        <h2 className="font-heading text-4xl leading-tight font-extrabold text-white">
          Loans made simple.
        </h2>
        <p className="max-w-sm text-sidebar-foreground/80">
          Apply, track, and repay in one place — with every step clear from submitted to funds
          released.
        </p>
        <p className="text-xs text-sidebar-foreground/50">© {new Date().getFullYear()} Loanly</p>
      </div>
    </div>

    <div className="flex items-center justify-center bg-bg px-4 py-10 sm:px-8">
      <div className="w-full max-w-xl">
        <div className="mb-8 lg:hidden">
          <Logo img="/assets/logo.png" name="Loanly" size="xl" />
        </div>
        <h1 className="font-heading text-5xl font-extrabold text-ink">{title}</h1>
        {subtitle && <p className="mt-1 text-xl text-black">{subtitle}</p>}
        <div className="mt-6">{children}</div>
        {footer && <div className="mt-6 text-xl text-black">{footer}</div>}
      </div>
    </div>
  </div>
)

export default AuthLayout
