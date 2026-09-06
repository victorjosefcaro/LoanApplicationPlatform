import { useQuery } from '@tanstack/react-query'
import { getUsers } from './users.api'
import { userKeys } from './users.keys'
import type { UserListParams } from './users.types'

export const useUsers = (params?: UserListParams) =>
  useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => getUsers(params),
  })
