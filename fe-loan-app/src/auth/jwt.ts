// The .NET backend signs tokens with ClaimTypes.NameIdentifier / ClaimTypes.Role,
// which serialize to these schema-URI claim keys. There is no username claim,
// so the display name is captured from the login form separately.
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export type JwtClaims = {
  userId: number | null
  role: string | null
  tenantId: number | null
  exp: number | null
}

const base64UrlDecode = (segment: string): string => {
  const base64 = segment.replace(/-/g, '+').replace(/_/g, '/')
  const padLength = base64.length % 4 === 0 ? 0 : 4 - (base64.length % 4)
  return atob(base64 + '='.repeat(padLength))
}

const firstString = (value: unknown): string | null => {
  if (typeof value === 'string') return value
  if (Array.isArray(value) && typeof value[0] === 'string') return value[0]
  return null
}

const toNumber = (value: unknown): number | null => {
  const text = firstString(value)
  if (text === null) return null
  const parsed = Number(text)
  return Number.isFinite(parsed) ? parsed : null
}

export const decodeJwt = (token: string): JwtClaims | null => {
  const parts = token.split('.')
  if (parts.length < 2) return null

  try {
    const payload = JSON.parse(base64UrlDecode(parts[1])) as Record<string, unknown>
    return {
      userId: toNumber(payload[NAMEID_CLAIM] ?? payload.nameid ?? payload.sub),
      role: firstString(payload[ROLE_CLAIM] ?? payload.role),
      tenantId: toNumber(payload.tenant_id),
      exp: typeof payload.exp === 'number' ? payload.exp : null,
    }
  } catch {
    return null
  }
}

export const isTokenValid = (claims: JwtClaims | null): boolean => {
  if (!claims || !claims.role) return false
  if (claims.exp !== null && claims.exp * 1000 <= Date.now()) return false
  return true
}
