import { type ReactNode } from 'react'
import type { PaymentSchedule } from '@/api/payments/payments.types'
import { formatDate } from '@/utils/format'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Money } from '@/components/money/money'
import { PaymentStatusPill } from '@/components/status-pill'

type ScheduleTableProps = {
  schedules: PaymentSchedule[]
  /** Optional trailing action cell per row (e.g. Pay / Post buttons). */
  renderAction?: (schedule: PaymentSchedule) => ReactNode
}

export const ScheduleTable = ({ schedules, renderAction }: ScheduleTableProps) => {
  const ordered = [...schedules].sort(
    (a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime(),
  )

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead className="w-12">#</TableHead>
          <TableHead>Due date</TableHead>
          <TableHead className="text-right">Amount due</TableHead>
          <TableHead className="text-right">Paid</TableHead>
          <TableHead>Status</TableHead>
          {renderAction && <TableHead className="text-right">Action</TableHead>}
        </TableRow>
      </TableHeader>
      <TableBody>
        {ordered.map((schedule, index) => (
          <TableRow key={schedule.id}>
            <TableCell className="text-brand">{index + 1}</TableCell>
            <TableCell>{formatDate(schedule.dueDate)}</TableCell>
            <TableCell className="text-right">
              <Money amount={schedule.amountDue} />
            </TableCell>
            <TableCell className="text-right text-brand">
              <Money amount={schedule.amountPaid} />
            </TableCell>
            <TableCell>
              <PaymentStatusPill status={schedule.status} />
            </TableCell>
            {renderAction && <TableCell className="text-right">{renderAction(schedule)}</TableCell>}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

export default ScheduleTable
