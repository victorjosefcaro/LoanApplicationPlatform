import { apiClient } from '@/api/client'
import { unwrap } from '@/api/unwrap'
import type {
  AdminRegistrationRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from './auth.types'

const AUTH_API = '/authentication'

export const login = async (payload: LoginRequest): Promise<LoginResponse> => {
  const response = await apiClient.post<ApiBodyResponse<LoginResponse>>(
    `${AUTH_API}/login`,
    payload,
  )
  return unwrap(response)
}

export const register = async (payload: RegisterRequest): Promise<MessageResponse> => {
  const response = await apiClient.post<ApiBodyResponse<MessageResponse>>(
    `${AUTH_API}/register`,
    payload,
  )
  return unwrap(response)
}

export const registerAdminUser = async (
  payload: AdminRegistrationRequest,
): Promise<MessageResponse> => {
  const response = await apiClient.post<ApiBodyResponse<MessageResponse>>(
    `${AUTH_API}/admin/register`,
    payload,
  )
  return unwrap(response)
}
