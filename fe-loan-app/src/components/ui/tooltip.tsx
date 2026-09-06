import { type ReactElement, type ReactNode } from 'react'
import { Tooltip as TooltipPrimitive } from '@base-ui/react'

import { cn } from '@/lib/utils'

/** Shared hover delay/grouping for every tooltip in the app. Mount once at the root. */
const TooltipProvider = TooltipPrimitive.Provider

type TooltipProps = {
  /** The hover/focus text. When empty, the trigger renders with no tooltip attached. */
  content?: ReactNode
  /** The element the tooltip is attached to (e.g. an icon button). */
  children: ReactElement
  side?: TooltipPrimitive.Positioner.Props['side']
  sideOffset?: TooltipPrimitive.Positioner.Props['sideOffset']
  align?: TooltipPrimitive.Positioner.Props['align']
}

/**
 * A small, styled tooltip wrapping the Base UI primitive — mirrors the popover.tsx
 * wrapper so all tooltips share one look and behavior. Usage:
 *   <Tooltip content="Review"><Button size="icon"><FiEye /></Button></Tooltip>
 */
function Tooltip({
  content,
  children,
  side = 'top',
  sideOffset = 6,
  align = 'center',
}: TooltipProps) {
  if (content === undefined || content === null || content === '') return children

  return (
    <TooltipPrimitive.Root>
      <TooltipPrimitive.Trigger render={children} />
      <TooltipPrimitive.Portal>
        <TooltipPrimitive.Positioner
          side={side}
          sideOffset={sideOffset}
          align={align}
          className="isolate z-50"
        >
          <TooltipPrimitive.Popup
            data-slot="tooltip-content"
            className={cn(
              'origin-(--transform-origin) rounded-sm border bg-popover px-2 py-1 text-xs text-popover-foreground shadow-md outline-none',
              'data-open:animate-in data-open:fade-in-0 data-open:zoom-in-95 data-closed:animate-out data-closed:fade-out-0 data-closed:zoom-out-95',
            )}
          >
            {content}
          </TooltipPrimitive.Popup>
        </TooltipPrimitive.Positioner>
      </TooltipPrimitive.Portal>
    </TooltipPrimitive.Root>
  )
}

export { Tooltip, TooltipProvider }
