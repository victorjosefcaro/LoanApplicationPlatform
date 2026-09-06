import { type ReactNode } from 'react'
import ModalDialog from '@/components/shared/components/modal-dialog'
import { InlineError } from '@/components/states'

type ConfirmDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  /** Optional body copy shown above/instead of the error. */
  body?: ReactNode
  confirmLabel: string
  /** Label shown on the confirm button while pending. Defaults to confirmLabel. */
  pendingLabel?: string
  cancelLabel?: string
  confirmVariant?: 'default' | 'destructive'
  pending?: boolean
  /** When set, renders an inline error inside the dialog instead of dismissing it. */
  error?: unknown
  onConfirm: () => void
}

/**
 * A small reusable confirmation dialog for guarding critical actions.
 * Wraps ModalDialog and keeps itself open while pending or on error so the
 * user can see progress/failure without the action silently going through.
 */
const ConfirmDialog = ({
  open,
  onOpenChange,
  title,
  description,
  body,
  confirmLabel,
  pendingLabel,
  cancelLabel = 'Cancel',
  confirmVariant = 'default',
  pending = false,
  error,
  onConfirm,
}: ConfirmDialogProps) => (
  <ModalDialog
    open={open}
    onOpenChange={onOpenChange}
    title={title}
    desc={description}
    size="sm"
    actionButton={[
      {
        type: 'button',
        variant: 'ghost',
        value: cancelLabel,
        onClick: () => onOpenChange(false),
        disabled: pending,
      },
      {
        type: 'button',
        variant: confirmVariant,
        value: pending ? (pendingLabel ?? confirmLabel) : confirmLabel,
        onClick: onConfirm,
        disabled: pending,
      },
    ]}
  >
    {error ? <InlineError error={error} /> : body ? body : null}
  </ModalDialog>
)

export default ConfirmDialog
