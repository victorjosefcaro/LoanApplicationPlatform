import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { FiLogOut, FiMenu, FiUser } from 'react-icons/fi'
import { Button } from '@/components/ui/button'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { useAuth } from '@/auth/auth-context'
import { useProfile } from '@/hooks/profile/use-profile'
import { greeting, displayFirstName } from '@/utils/format'

export const Topbar = ({ onMenuClick }: { onMenuClick: () => void }) => {
  const { user, signOut } = useAuth()
  const { fullName } = useProfile()
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)

  const displayName = fullName.trim() || user?.username || 'Account'

  const goToProfile = () => {
    setMenuOpen(false)
    navigate('/profile')
  }

  const handleSignOut = () => {
    setMenuOpen(false)
    signOut()
    navigate('/login', { replace: true })
  }

  return (
    <header className="sticky top-0 z-20 flex h-16 items-center gap-3 border-b border-line bg-surface/80 px-4 backdrop-blur sm:px-6">
      <Button
        variant="ghost"
        size="icon"
        className="md:hidden"
        onClick={onMenuClick}
        aria-label="Open menu"
      >
        <FiMenu className="size-5" />
      </Button>

      <div className="min-w-0">
        <p className="truncate text-sm text-black">
          {greeting()},{' '}
          <span className="font-bold text-black">
            {displayFirstName(fullName.trim() || user?.username)}
          </span>
        </p>
      </div>

      <div className="ml-auto flex items-center">
        <Popover open={menuOpen} onOpenChange={setMenuOpen}>
          <PopoverTrigger
            className="flex items-center gap-3 rounded-sm p-1 pl-3 transition-colors hover:bg-black/5 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gold"
            aria-label="Open account menu"
          >
            <div className="hidden text-right sm:block">
              <p className="text-sm font-bold text-ink">{displayName}</p>
              <p className="text-xs text-black">{user?.role}</p>
            </div>
            <span
              className="grid size-9 place-items-center rounded-full bg-brand-tint font-heading text-sm font-bold text-brand-deep"
              aria-hidden="true"
            >
              {displayName.charAt(0).toUpperCase()}
            </span>
          </PopoverTrigger>
          <PopoverContent align="end" className="w-48 p-1">
            <Button
              variant="ghost"
              onClick={goToProfile}
              className="w-full justify-start gap-2 font-medium"
            >
              <FiUser className="size-4" />
              My profile
            </Button>
            <Button
              variant="ghost"
              onClick={handleSignOut}
              className="w-full justify-start gap-2 font-medium"
            >
              <FiLogOut className="size-4" />
              Sign out
            </Button>
          </PopoverContent>
        </Popover>
      </div>
    </header>
  )
}

export default Topbar
