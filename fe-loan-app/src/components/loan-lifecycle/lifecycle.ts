export type NodeState = 'done' | 'current' | 'upcoming'

export type LifecycleStage = {
  label: string
  state: NodeState
}

export type Terminal = {
  kind: 'rejected' | 'cancelled'
  label: string
}

export type LifecycleView = {
  stages: LifecycleStage[]
  terminal: Terminal | null
  /** Short nudge shown near the rail, e.g. for returned applications. */
  note: string | null
}

const STAGE_LABELS = ['Submitted', 'Under review', 'Approved', 'Funds released'] as const

// Index into STAGE_LABELS each status has reached.
const REACHED_INDEX: Record<string, number> = {
  Submitted: 0,
  Returned: 0,
  Reviewed: 1,
  Approved: 2,
  Released: 3,
  Completed: 3,
}

const FULLY_DONE = new Set(['Released', 'Completed'])

export const lifecycleView = (status: string): LifecycleView => {
  const terminal: Terminal | null =
    status === 'Rejected'
      ? { kind: 'rejected', label: 'Application rejected' }
      : status === 'Cancelled'
        ? { kind: 'cancelled', label: 'Application cancelled' }
        : null

  const reached = REACHED_INDEX[status] ?? 0
  const allDone = FULLY_DONE.has(status)

  const stages: LifecycleStage[] = STAGE_LABELS.map((label, index) => {
    if (terminal) return { label, state: 'upcoming' }
    if (allDone) return { label, state: 'done' }
    if (index < reached) return { label, state: 'done' }
    if (index === reached) return { label, state: 'current' }
    return { label, state: 'upcoming' }
  })

  const note =
    status === 'Returned' ? 'Returned for changes — update the details and resubmit.' : null

  return { stages, terminal, note }
}
