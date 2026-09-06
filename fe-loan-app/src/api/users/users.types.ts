export type User = {
  id: number
  username: string
  role: string
  isActive: boolean
}

export type UserListParams = {
  pageNumber?: number
  pageSize?: number
  search?: string
}

export type UpdateUserRoleRequest = {
  role: string
}

export type ResetUserPasswordRequest = {
  password: string
}
