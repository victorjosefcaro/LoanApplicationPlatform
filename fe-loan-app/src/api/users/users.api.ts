import { apiClient } from '@/api/client'
import { unwrap, unwrapPaged } from '@/api/unwrap'
import type {
  ResetUserPasswordRequest,
  UpdateUserRoleRequest,
  User,
  UserListParams,
} from './users.types'

const USERS_API = '/users'

export const getUsers = async (params?: UserListParams): Promise<PagedResult<User>> => {
  const response = await apiClient.get<User[]>(USERS_API, { params })
  return unwrapPaged(response)
}

export const updateUserRole = async (
  id: number,
  payload: UpdateUserRoleRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.put<ApiBodyResponse<MessageResponse>>(
    `${USERS_API}/${id}/role`,
    payload,
  )
  return unwrap(response)
}

export const setUserActive = async (id: number, isActive: boolean): Promise<MessageResponse> => {
  const response = await apiClient.patch<ApiBodyResponse<MessageResponse>>(
    `${USERS_API}/${id}/status`,
    { isActive },
  )
  return unwrap(response)
}

export const resetUserPassword = async (
  id: number,
  payload: ResetUserPasswordRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.put<ApiBodyResponse<MessageResponse>>(
    `${USERS_API}/${id}/password`,
    payload,
  )
  return unwrap(response)
}
