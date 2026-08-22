import { cn } from '@/lib/utils'

type LogoSize = 'sm' | 'md' | 'lg' | 'xl'

const SIZE: Record<LogoSize, { img: string; name: string }> = {
  sm: { img: 'w-8 h-8', name: 'text-2xl' },
  md: { img: 'w-12 h-12', name: 'text-4xl' },
  lg: { img: 'w-20 h-20', name: 'text-6xl' },
  xl: { img: 'w-32 h-32', name: 'text-8xl' },
}

type ILogo = {
  size?: LogoSize
  className?: string
  img: string
  name: string
}

export const Logo = ({ size = 'md', className, img, name }: ILogo) => {
  const parseString = (value: string) => {
    if (!value || value === '') return ''
    return (value.trim().split('/').pop() ?? '').replace(/\.[^/.]+$/, '')
  }

  const { img: imgSize, name: nameSize } = SIZE[size]

  return (
    <div className={cn('flex w-full items-center justify-center gap-2', className)}>
      <img src={img} className={imgSize} alt={parseString(img)} />
      <span className={cn('font-heading font-extrabold', nameSize)}>{name}</span>
    </div>
  )
}

export default Logo
