export type LoginRequest = {
  username: string
  password: string
  tenantId?: number
}

// Applicant self-registration uses the same payload shape as login.
export type RegisterRequest = LoginRequest

export type AdminRegistrationRequest = {
  username: string
  password: string
  role: string
  tenantId?: number
}

// POST /authentication/login responds with a bare JWT string.
export type LoginResponse = string
