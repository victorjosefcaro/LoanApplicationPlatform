import { useEffect, useState } from 'react'

const MOBILE_BREAKPOINT = 768

/**
 * Returns `true` while the viewport is narrower than `breakpoint` (px).
 * SSR-safe: falls back to `false` when `window` is unavailable.
 */
const useIsMobile = (breakpoint: number = MOBILE_BREAKPOINT): boolean => {
  const [isMobile, setIsMobile] = useState<boolean>(
    typeof window !== 'undefined' ? window.innerWidth < breakpoint : false,
  )

  useEffect(() => {
    const query = window.matchMedia(`(max-width: ${breakpoint - 1}px)`)
    const onChange = (): void => setIsMobile(window.innerWidth < breakpoint)

    onChange()
    query.addEventListener('change', onChange)
    return () => query.removeEventListener('change', onChange)
  }, [breakpoint])

  return isMobile
}

export default useIsMobile
