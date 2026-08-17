import { type ReactNode, useEffect, useMemo, useState } from 'react'

import useIsMobile from '@/hooks/use-is-mobile'
import { compareValues } from '@/utils/compare-values'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'

import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

import {
  FiArrowDown,
  FiArrowUp,
  FiFilter,
  FiMoreHorizontal,
  FiPlus,
  FiSearch,
} from 'react-icons/fi'

export type ColumnDef<T> = {
  key: keyof T | string
  label: string
  sortable?: boolean
  filterable?: boolean
  /** Desktop column width, e.g. '160px' or '20%'. Omit to auto-size. */
  width?: string
  /** Width used instead of `width` below the mobile breakpoint. */
  mobileWidth?: string
  render?: (item: T) => ReactNode
  className?: string
}

export type ActionDef<T> = {
  icon?: ReactNode
  label?: string
  onClick: (item: T) => void
  variant?: 'default' | 'destructive' | 'ghost' | 'outline' | 'secondary' | 'link'
  disabled?: (item: T) => boolean
  title?: string
  show?: (item: T) => boolean
  className?: string
}

export type ToolbarButton = {
  label: ReactNode
  onClick: () => void
  variant?: 'default' | 'destructive' | 'ghost' | 'outline' | 'secondary' | 'link'
  disabled?: boolean
  title?: string
}

type SortDirection = 'asc' | 'desc'

const MAX_INLINE_BUTTONS = 3
const DEFAULT_PAGE_SIZE = 10
const DEFAULT_COL_MIN_PX = 120

const toPx = (value: string) => {
  const v = value.trim()
  if (v.endsWith('px')) return parseFloat(v) || DEFAULT_COL_MIN_PX
  if (v.endsWith('rem')) return (parseFloat(v) || 0) * 16
  return DEFAULT_COL_MIN_PX
}

// Page list like [1, 'ellipsis', 4, 5, 6, 'ellipsis', 20] — keeps the pager a fixed width.
const getPageList = (current: number, total: number): (number | 'ellipsis')[] => {
  const delta = 1
  const pages: (number | 'ellipsis')[] = []
  const left = Math.max(2, current - delta)
  const right = Math.min(total - 1, current + delta)

  pages.push(1)
  if (left > 2) pages.push('ellipsis')
  for (let i = left; i <= right; i++) pages.push(i)
  if (right < total - 1) pages.push('ellipsis')
  if (total > 1) pages.push(total)

  return pages
}

type ShadcnDataTableProps<T> = {
  data: T[]
  columns: ColumnDef<T>[]

  searchTerm: string
  setSearchTerm: (term: string) => void
  searchPlaceholder?: string
  /** Which fields the search box matches against. Defaults to all columns' keys. */
  searchFields?: (keyof T)[]

  isLoading?: boolean
  emptyMessage?: ReactNode

  showAddButton?: boolean
  onAdd?: () => void
  addButtonLabel?: string

  /** Extra toolbar buttons, rendered after the built-in add button. */
  buttons?: ToolbarButton[]
  /** Title of the collapse modal shown when there are more than 3 buttons. */
  buttonsModalTitle?: ReactNode
  /** Label of the trigger that opens the collapse modal. */
  buttonsModalTriggerLabel?: ReactNode

  actions?: ActionDef<T>[]

  className?: string

  /** Rows per page in client (uncontrolled) mode. Default 10. */
  pageSize?: number
  page?: number
  totalCount?: number
  onPageChange?: (page: number) => void
}

const ShadcnDataTable = <T,>({
  data,
  columns,
  searchTerm,
  setSearchTerm,
  searchPlaceholder = 'Search...',
  searchFields,
  isLoading = false,
  emptyMessage,
  showAddButton = true,
  onAdd,
  addButtonLabel = 'Create',
  buttons,
  buttonsModalTitle = 'More Options',
  buttonsModalTriggerLabel = 'More Options',
  actions,
  className = '',
  pageSize = DEFAULT_PAGE_SIZE,
  page,
  totalCount,
  onPageChange,
}: ShadcnDataTableProps<T>) => {
  const isMobile = useIsMobile()
  const isControlled = page !== undefined && totalCount !== undefined && !!onPageChange

  const rows = useMemo(() => (Array.isArray(data) ? data : []), [data])

  // Sort / filter state (client mode only)
  const [sortField, setSortField] = useState<keyof T | undefined>(undefined)
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc')
  const [filters, setFilters] = useState<Record<string, Set<string> | null>>({})

  const toggleSort = (key: keyof T) => {
    if (sortField !== key) {
      setSortField(key)
      setSortDirection('asc')
    } else if (sortDirection === 'asc') {
      setSortDirection('desc')
    } else {
      setSortField(undefined)
    }
  }

  const distinctValues = useMemo(() => {
    const map: Record<string, string[]> = {}
    if (isControlled) return map

    for (const column of columns) {
      if (column.filterable && column.key) {
        const values = new Set<string>()
        for (const row of rows) {
          values.add(String((row as Record<string, unknown>)[column.key as string] ?? ''))
        }
        map[column.key as string] = Array.from(values).sort((a, b) => compareValues(a, b))
      }
    }
    return map
  }, [columns, rows, isControlled])

  const setColumnFilter = (key: keyof T, next: Set<string> | null) =>
    setFilters((prev) => ({ ...prev, [key as string]: next }))

  // Search (client mode only; controlled mode assumes the caller filtered server-side).
  const searchedData = useMemo(() => {
    if (isControlled) return rows

    const term = searchTerm.trim().toLowerCase()
    if (!term) return rows

    const keys =
      searchFields && searchFields.length > 0
        ? searchFields
        : columns.map((c) => c.key as keyof T).filter(Boolean)

    return rows.filter((row) =>
      keys.some((key) =>
        String((row as Record<string, unknown>)[key as string] ?? '')
          .toLowerCase()
          .includes(term),
      ),
    )
  }, [rows, searchTerm, searchFields, columns, isControlled])

  const filteredData = useMemo(() => {
    if (isControlled) return searchedData

    const active = Object.entries(filters).filter(([, set]) => set !== null && set.size > 0) as [
      string,
      Set<string>,
    ][]
    if (active.length === 0) return searchedData

    return searchedData.filter((row) =>
      active.every(([key, set]) => set.has(String((row as Record<string, unknown>)[key] ?? ''))),
    )
  }, [searchedData, filters, isControlled])

  const sortedData = useMemo(() => {
    if (isControlled || !sortField) return filteredData

    const next = [...filteredData].sort((a, b) => compareValues(a[sortField], b[sortField]))
    return sortDirection === 'desc' ? next.reverse() : next
  }, [filteredData, sortField, sortDirection, isControlled])

  const [internalPage, setInternalPage] = useState(1)
  const currentPage = isControlled ? (page as number) : internalPage
  const effectiveTotal = isControlled ? (totalCount as number) : sortedData.length
  const totalPages = Math.max(1, Math.ceil(effectiveTotal / pageSize))

  useEffect(() => {
    if (!isControlled && internalPage > totalPages) setInternalPage(totalPages)
  }, [internalPage, totalPages, isControlled])

  // Reset to page 1 when the client-mode result set changes.
  useEffect(() => {
    if (!isControlled) setInternalPage(1)
  }, [searchTerm, filters, sortField, sortDirection, isControlled])

  const pageRows = useMemo(() => {
    if (isControlled) return rows
    const start = (currentPage - 1) * pageSize
    return sortedData.slice(start, start + pageSize)
  }, [isControlled, rows, sortedData, currentPage, pageSize])

  const goToPage = (next: number) => {
    const clamped = Math.min(Math.max(1, next), totalPages)
    if (isControlled) {
      onPageChange?.(clamped)
    } else {
      setInternalPage(clamped)
    }
  }

  const rangeStart = effectiveTotal === 0 ? 0 : (currentPage - 1) * pageSize + 1
  const rangeEnd = Math.min(currentPage * pageSize, effectiveTotal)

  const toolbarButtons: ToolbarButton[] = [
    ...(showAddButton && onAdd
      ? [
          {
            label: (
              <>
                <FiPlus className="mr-1.5 text-base" />
                {addButtonLabel}
              </>
            ),
            onClick: onAdd,
          },
        ]
      : []),
    ...(buttons ?? []),
  ]

  const collapseButtons = toolbarButtons.length > MAX_INLINE_BUTTONS
  const [buttonsModalOpen, setButtonsModalOpen] = useState(false)

  const colWidth = (col: ColumnDef<T>) =>
    isMobile && col.mobileWidth ? col.mobileWidth : col.width

  const tableMinWidth = useMemo(() => {
    const sum = columns.reduce((total, col) => {
      const w = colWidth(col)
      return total + (w ? toPx(w) : DEFAULT_COL_MIN_PX)
    }, 0)
    return `${sum + (actions ? 140 : 0)}px`
  }, [columns, isMobile, actions])

  return (
    <div className={`rounded-xl border bg-white p-4 shadow-sm ${className}`}>
      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative w-full sm:max-w-xs">
          <FiSearch className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <Input
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder={searchPlaceholder}
            className="h-10 rounded-lg pl-9"
          />
        </div>

        {!collapseButtons && toolbarButtons.length > 0 && (
          <div className="flex flex-wrap gap-2">
            {toolbarButtons.map((btn, i) => (
              <Button
                key={i}
                variant={btn.variant}
                disabled={btn.disabled}
                title={btn.title}
                onClick={btn.onClick}
              >
                {btn.label}
              </Button>
            ))}
          </div>
        )}

        {collapseButtons && (
          <Button variant="outline" onClick={() => setButtonsModalOpen(true)}>
            <FiMoreHorizontal className="mr-1.5" />
            {buttonsModalTriggerLabel}
          </Button>
        )}
      </div>

      {collapseButtons && (
        <Dialog open={buttonsModalOpen} onOpenChange={setButtonsModalOpen}>
          <DialogContent className="max-w-sm">
            <DialogHeader>
              <DialogTitle>{buttonsModalTitle}</DialogTitle>
            </DialogHeader>

            <div className="flex flex-col gap-2">
              {toolbarButtons.map((btn, i) => (
                <Button
                  key={i}
                  variant={btn.variant}
                  disabled={btn.disabled}
                  title={btn.title}
                  onClick={() => {
                    btn.onClick()
                    setButtonsModalOpen(false)
                  }}
                >
                  {btn.label}
                </Button>
              ))}
            </div>
          </DialogContent>
        </Dialog>
      )}

      <div className="w-full overflow-x-auto">
        <Table style={{ minWidth: tableMinWidth }}>
          <TableHeader>
            <TableRow>
              {columns.map((col) => {
                const canSort = !isControlled && col.sortable && !!col.key
                const canFilter = !isControlled && col.filterable && !!col.key
                const isActive = !!col.key && sortField === col.key
                const width = colWidth(col)

                return (
                  <TableHead
                    key={String(col.key)}
                    style={width ? { width } : undefined}
                    className="whitespace-nowrap"
                  >
                    <div className="flex items-center gap-1.5">
                      {canSort ? (
                        <button
                          type="button"
                          className="flex items-center gap-1 font-medium hover:text-gray-900"
                          onClick={() => toggleSort(col.key as keyof T)}
                        >
                          <span>{col.label}</span>
                          {isActive &&
                            (sortDirection === 'asc' ? (
                              <FiArrowUp className="text-xs" />
                            ) : (
                              <FiArrowDown className="text-xs" />
                            ))}
                        </button>
                      ) : (
                        <span>{col.label}</span>
                      )}

                      {canFilter && (
                        <Popover>
                          <PopoverTrigger
                            className={`rounded p-0.5 hover:bg-gray-100 ${
                              filters[col.key as string]?.size ? 'text-blue-600' : 'text-gray-400'
                            }`}
                            title={`Filter ${col.label}`}
                          >
                            <FiFilter className="text-xs" />
                          </PopoverTrigger>

                          <PopoverContent align="start" className="w-56 p-2">
                            <div className="mb-1 flex items-center justify-between px-1">
                              <span className="text-xs font-medium text-gray-500">
                                Filter {col.label}
                              </span>

                              <button
                                type="button"
                                className="text-xs font-medium text-blue-600 hover:underline"
                                onClick={() => setColumnFilter(col.key as keyof T, null)}
                              >
                                Clear
                              </button>
                            </div>

                            <div className="max-h-56 space-y-1 overflow-y-auto">
                              {(distinctValues[col.key as string] ?? []).map((value) => {
                                const selected = filters[col.key as string]
                                const checked = selected ? selected.has(value) : false

                                return (
                                  <label
                                    key={value}
                                    className="flex items-center gap-2 rounded px-1 py-1 text-sm hover:bg-gray-50"
                                  >
                                    <Checkbox
                                      checked={checked}
                                      onCheckedChange={(next) => {
                                        const current =
                                          filters[col.key as string] ?? new Set<string>()
                                        const updated = new Set(current)
                                        if (next) {
                                          updated.add(value)
                                        } else {
                                          updated.delete(value)
                                        }
                                        setColumnFilter(
                                          col.key as keyof T,
                                          updated.size > 0 ? updated : null,
                                        )
                                      }}
                                    />
                                    <span>{value || '(empty)'}</span>
                                  </label>
                                )
                              })}
                            </div>
                          </PopoverContent>
                        </Popover>
                      )}
                    </div>
                  </TableHead>
                )
              })}

              {actions && <TableHead className="text-right">Actions</TableHead>}
            </TableRow>
          </TableHeader>

          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length + (actions ? 1 : 0)}
                  className="h-32 text-center text-sm text-gray-500"
                >
                  Loading data…
                </TableCell>
              </TableRow>
            ) : pageRows.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length + (actions ? 1 : 0)}
                  className="h-32 text-center text-sm text-gray-500"
                >
                  {emptyMessage ??
                    (searchTerm ? 'No records match your search.' : 'No records found.')}
                </TableCell>
              </TableRow>
            ) : (
              pageRows.map((item, rowIndex) => (
                <TableRow key={rowIndex}>
                  {columns.map((col) => (
                    <TableCell key={String(col.key)} className={col.className}>
                      {col.render
                        ? col.render(item)
                        : String((item as Record<string, unknown>)[col.key as string] ?? '—')}
                    </TableCell>
                  ))}

                  {actions && (
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1.5">
                        {actions
                          .filter((a) => !a.show || a.show(item))
                          .map((action, i) => {
                            const disabled = action.disabled ? action.disabled(item) : false

                            return (
                              <Button
                                key={i}
                                variant={action.variant ?? 'default'}
                                size="icon"
                                disabled={disabled}
                                title={action.title ?? action.label ?? ''}
                                className={action.className}
                                onClick={() => action.onClick(item)}
                              >
                                {action.icon ?? action.label}
                              </Button>
                            )
                          })}
                      </div>
                    </TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {!isLoading && effectiveTotal > pageSize && (
        <div className="mt-4 flex flex-col items-center justify-between gap-3 sm:flex-row">
          <p className="text-sm text-gray-500">
            Showing {rangeStart}–{rangeEnd} of {effectiveTotal}
          </p>

          <Pagination className="mx-0 w-auto">
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  href="#"
                  aria-disabled={currentPage <= 1}
                  className={currentPage <= 1 ? 'pointer-events-none opacity-50' : ''}
                  onClick={(e) => {
                    e.preventDefault()
                    if (currentPage > 1) goToPage(currentPage - 1)
                  }}
                />
              </PaginationItem>

              {getPageList(currentPage, totalPages).map((p, i) =>
                p === 'ellipsis' ? (
                  <PaginationItem key={`ellipsis-${i}`}>
                    <PaginationEllipsis />
                  </PaginationItem>
                ) : (
                  <PaginationItem key={p}>
                    <PaginationLink
                      href="#"
                      isActive={p === currentPage}
                      onClick={(e) => {
                        e.preventDefault()
                        goToPage(p)
                      }}
                    >
                      {p}
                    </PaginationLink>
                  </PaginationItem>
                ),
              )}

              <PaginationItem>
                <PaginationNext
                  href="#"
                  aria-disabled={currentPage >= totalPages}
                  className={currentPage >= totalPages ? 'pointer-events-none opacity-50' : ''}
                  onClick={(e) => {
                    e.preventDefault()
                    if (currentPage < totalPages) goToPage(currentPage + 1)
                  }}
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </div>
      )}
    </div>
  )
}

export default ShadcnDataTable

// Migration note: mirrors the shared DataTable prop shape so pages can swap
// `DataTable` -> `ShadcnDataTable`. Only shadcn Button variants are supported,
// and it renders a plain <table> (check mobile horizontal scroll).
