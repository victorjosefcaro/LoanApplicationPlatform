import { type ComponentType } from 'react'
import { NavLink } from 'react-router-dom'
import { FiDollarSign, FiFileText, FiHome, FiInbox, FiPlusCircle, FiUserPlus } from 'react-icons/fi'
import { cn } from '@/lib/utils'
import { Logo } from '@/components/brand/logo'
import { useAuth } from '@/auth/auth-context'

type NavItem = {
  to: string
  label: string
  icon: ComponentType<{ className?: string }>
  end?: boolean
}

const APPLICANT_NAV: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: FiHome, end: true },
  { to: '/loans', label: 'My loans', icon: FiFileText },
]

const STAFF_NAV: NavItem[] = [
  { to: '/admin', label: 'Review queue', icon: FiInbox, end: true },
  { to: '/admin/register', label: 'Add team member', icon: FiUserPlus },
  { to: '/admin/treasury', label: 'Treasury', icon: FiDollarSign },
]

export const Sidebar = ({ onNavigate }: { onNavigate?: () => void }) => {
  const { isStaff } = useAuth()
  const items = isStaff ? STAFF_NAV : APPLICANT_NAV

  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      <div className="flex h-16 w-full items-center px-3">
        <Logo img="/assets/logo.png" name="Loanly" size="md" className="w-auto justify-start" />
      </div>

      <nav className="flex-1 space-y-1 px-3 py-2" aria-label="Primary">
        {items.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            onClick={onNavigate}
            className={({ isActive }: { isActive: boolean }) =>
              cn(
                'flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gold',
                isActive
                  ? 'bg-sidebar-accent text-white'
                  : 'text-sidebar-foreground/80 hover:bg-sidebar-accent/60 hover:text-white',
              )
            }
          >
            <item.icon className="size-5 shrink-0" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="px-5 py-4 text-xs text-sidebar-foreground/50">Loans made simple.</div>
    </div>
  )
}

export default Sidebar
