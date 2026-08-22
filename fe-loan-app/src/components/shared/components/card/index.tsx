import { Card as CardUI, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { ReactNode } from 'react'

type ICard = {
  title?: string
  action?: ReactNode
  content?: ReactNode
  contentClassName?: string
}

const Card = ({ title, action, content, contentClassName }: ICard) => (
  <CardUI>
    {title && (
      <CardHeader className={action ? 'flex flex-col items-start' : undefined}>
        <CardTitle>{title}</CardTitle>
       <span> {action}</span>
      </CardHeader>
    )}
    <CardContent className={contentClassName}>{content}</CardContent>
  </CardUI>
)

export default Card
