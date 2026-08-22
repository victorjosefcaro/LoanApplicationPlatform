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

// The stage each status is actively at. Earlier stages render as done (an
// achieved milestone), so e.g. a Submitted application shows submission complete.
const CURRENT_INDEX: Record<string, number> = {
  Submitted: 1, // submission complete; awaiting review
  Returned: 0, // back with the applicant to revise & resubmit
  Reviewed: 1, // under review
  Approved: 3, // approved; awaiting fund release
  Released: 4, // every stage complete
  Completed: 4,
}

export const lifecycleView = (status: string): LifecycleView => {
  const terminal: Terminal | null =
    status === 'Rejected'
      ? { kind: 'rejected', label: 'Application rejected' }
      : status === 'Cancelled'
        ? { kind: 'cancelled', label: 'Application cancelled' }
        : null

  const current = CURRENT_INDEX[status] ?? 0

  const stages: LifecycleStage[] = STAGE_LABELS.map((label, index) => {
    if (terminal) return { label, state: 'upcoming' }
    if (index < current) return { label, state: 'done' }
    if (index === current) return { label, state: 'current' }
    return { label, state: 'upcoming' }
  })

  const note =
    status === 'Returned' ? 'Returned for changes — update the details and resubmit.' : null

  return { stages, terminal, note }
}
