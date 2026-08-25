import { Dialog, DialogContent } from '@/components/ui/dialog'
import { DialogTitle } from '@base-ui/react'
import { type ReactNode } from 'react'
import DataMap from '@/utils/data-map'
import { Button } from '@/components/ui/button'

type IActionButton = {
  type: 'button' | 'submit'
  variant?:
    'link' | 'default' | 'transparent' | 'outline' | 'secondary' | 'ghost' | 'destructive' | null
  onClick?: () => void
  disabled?: boolean
  className?: string
  value?: ReactNode | string
}

type IModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full'

type IModalDialog = {
  open: boolean
  onOpenChange?: (open: boolean) => void
  onClose?: () => void
  children: ReactNode
  title?: string
  desc?: string | ReactNode
  hasActionButton?: boolean
  actionButton?: IActionButton[]
  size?: IModalSize
}

const sizeClass: Record<IModalSize, string> = {
  sm: 'sm:max-w-md',
  md: 'sm:max-w-2xl',
  lg: 'sm:max-w-4xl',
  xl: 'sm:max-w-6xl',
  full: 'sm:max-w-[95vw]',
}

const ModalDialog = ({
  open,
  onOpenChange,
  onClose,
  title,
  desc,
  children,
  actionButton,
  size = 'md',
}: IModalDialog) => {
  const handleOpenChange = (nextOpen: boolean) => {
    onOpenChange?.(nextOpen)
    if (!nextOpen) onClose?.()
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className={`flex max-h-[85vh] w-full flex-col gap-0 overflow-hidden p-0 ${sizeClass[size]}`}
      >
        {title && (
          <div className="flex shrink-0 flex-col">
            <div className="border-b bg-white px-5 py-5 sm:px-8 sm:py-6">
              <h2 className="text-xl font-bold text-gray-900 sm:text-2xl">{title}</h2>
              {desc && <p className="mt-1.5 text-sm text-gray-500 sm:mt-2 sm:text-base">{desc}</p>}
            </div>
          </div>
        )}
        <div className="min-h-0 flex-1 space-y-5 overflow-y-auto bg-gray-50 px-5 py-5 sm:space-y-6 sm:px-8 sm:py-6">
          {children}
        </div>

        {actionButton && (
          <div className="flex shrink-0 flex-col-reverse gap-3 border-t bg-white px-5 py-4 sm:flex-row sm:justify-end sm:px-8 sm:py-5">
            {actionButton && (
              <DataMap
                data={actionButton}
                render={(b: IActionButton) => (
                  <Button
                    type={b.type}
                    variant={b.variant}
                    onClick={b.onClick}
                    disabled={b.disabled}
                    className={b.className}
                  >
                    {b.value}
                  </Button>
                )}
              />
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

export default ModalDialog
