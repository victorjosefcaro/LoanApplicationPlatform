// Static tenant list for the login selector.
// Mirrors the tenants seeded in the backend (LoanApplicationPlatformContext.cs).
// Keep in sync manually until a GET /tenants endpoint exists.
export const TENANT_OPTIONS = [
  { label: 'NanoCap', value: '1' },
  { label: 'SmartLend', value: '2' },
]
