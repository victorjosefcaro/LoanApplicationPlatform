export type PasswordRule = {
  id: string
  label: string
  test: (value: string) => boolean
}

// Rules enforced when creating an account.
export const PASSWORD_RULES: PasswordRule[] = [
  { id: 'length', label: 'At least 8 characters', test: (v) => v.length >= 8 },
  { id: 'uppercase', label: 'At least 1 uppercase letter', test: (v) => /[A-Z]/.test(v) },
  { id: 'lowercase', label: 'At least 1 lowercase letter', test: (v) => /[a-z]/.test(v) },
  { id: 'number', label: 'At least 1 number', test: (v) => /[0-9]/.test(v) },
  {
    id: 'special',
    label: 'At least 1 special character',
    test: (v) => /[^A-Za-z0-9\s]/.test(v),
  },
  { id: 'no-space', label: 'No spaces', test: (v) => v.length > 0 && !/\s/.test(v) },
]

export type PasswordCheck = {
  isValid: boolean
  results: { id: string; label: string; passed: boolean }[]
}

export const checkPassword = (value: string): PasswordCheck => {
  const results = PASSWORD_RULES.map((rule) => ({
    id: rule.id,
    label: rule.label,
    passed: rule.test(value),
  }))

  return { isValid: results.every((r) => r.passed), results }
}

export const isValidPassword = (value: string): boolean =>
  PASSWORD_RULES.every((rule) => rule.test(value))
