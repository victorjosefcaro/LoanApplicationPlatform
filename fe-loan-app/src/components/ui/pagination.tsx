import * as React from 'react'
import { ChevronLeftIcon, ChevronRightIcon, MoreHorizontalIcon } from 'lucide-react'

import { type VariantProps } from 'class-variance-authority'

import { cn } from '@/lib/utils'
import { buttonVariants } from './button'
import { Tooltip } from './tooltip'

const Pagination = ({ className, ...props }: React.ComponentProps<'nav'>) => (
  <nav
    role="navigation"
    aria-label="pagination"
    data-slot="pagination"
    className={cn('mx-auto flex w-full justify-center', className)}
    {...props}
  />
)

const PaginationContent = ({ className, ...props }: React.ComponentProps<'ul'>) => (
  <ul
    data-slot="pagination-content"
    className={cn('flex flex-row items-center gap-1', className)}
    {...props}
  />
)

const PaginationItem = ({ ...props }: React.ComponentProps<'li'>) => (
  <li data-slot="pagination-item" {...props} />
)

type PaginationLinkProps = {
  isActive?: boolean
  size?: VariantProps<typeof buttonVariants>['size']
} & React.ComponentProps<'a'>

const PaginationLink = ({ className, isActive, size = 'icon', ...props }: PaginationLinkProps) => (
  <a
    aria-current={isActive ? 'page' : undefined}
    data-slot="pagination-link"
    data-active={isActive}
    className={cn(buttonVariants({ variant: isActive ? 'outline' : 'ghost', size }), className)}
    {...props}
  />
)

const PaginationPrevious = ({
  className,
  ...props
}: React.ComponentProps<typeof PaginationLink>) => (
  <Tooltip content="Go to previous page">
    <PaginationLink
      aria-label="Go to previous page"
      size="default"
      className={cn('gap-1 px-2.5', className)}
      {...props}
    >
      <ChevronLeftIcon />
      <span className="hidden sm:block">Previous</span>
    </PaginationLink>
  </Tooltip>
)

const PaginationNext = ({ className, ...props }: React.ComponentProps<typeof PaginationLink>) => (
  <Tooltip content="Go to next page">
    <PaginationLink
      aria-label="Go to next page"
      size="default"
      className={cn('gap-1 px-2.5', className)}
      {...props}
    >
      <span className="hidden sm:block">Next</span>
      <ChevronRightIcon />
    </PaginationLink>
  </Tooltip>
)

const PaginationEllipsis = ({ className, ...props }: React.ComponentProps<'span'>) => (
  <span
    aria-hidden
    data-slot="pagination-ellipsis"
    className={cn('flex size-8 items-center justify-center', className)}
    {...props}
  >
    <MoreHorizontalIcon className="size-4" />
    <span className="sr-only">More pages</span>
  </span>
)

export {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationPrevious,
  PaginationNext,
  PaginationEllipsis,
}
