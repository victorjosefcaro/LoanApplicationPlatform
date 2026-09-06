import {
  Card as CardUI,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { ReactNode } from 'react'

type ICard = {
  title?: string
  subtitle?: ReactNode
  action?: ReactNode
  content?: ReactNode
  contentClassName?: string
}

const Card = ({ title, subtitle, action, content, contentClassName }: ICard) => (
  <CardUI>
    {title && (
      <CardHeader className={action ? 'flex flex-col items-start' : undefined}>
        <CardTitle>{title}</CardTitle>
        {subtitle && <CardDescription>{subtitle}</CardDescription>}
        <span> {action}</span>
      </CardHeader>
    )}
    <CardContent className={contentClassName}>{content}</CardContent>
  </CardUI>
)

export default Card
