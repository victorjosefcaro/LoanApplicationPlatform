import { FiChevronLeft, FiChevronRight } from 'react-icons/fi'
import { Button } from '@/components/ui/button'
import { Tooltip } from '@/components/ui/tooltip'
import { cn } from '@/lib/utils'

type PagerProps = {
  index: number
  count: number
  onChange: (index: number) => void
  label?: string
  className?: string
}

export const Pager = ({ index, count, onChange, label = 'item', className }: PagerProps) => {
  if (count <= 1) return null
  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Tooltip content={`Previous ${label}`}>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label={`Previous ${label}`}
          disabled={index <= 0}
          onClick={() => onChange(index - 1)}
        >
          <FiChevronLeft />
        </Button>
      </Tooltip>
      <span className="text-sm tabular-nums text-brand">
        {index + 1} / {count}
      </span>
      <Tooltip content={`Next ${label}`}>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label={`Next ${label}`}
          disabled={index >= count - 1}
          onClick={() => onChange(index + 1)}
        >
          <FiChevronRight />
        </Button>
      </Tooltip>
    </div>
  )
}

export default Pager
