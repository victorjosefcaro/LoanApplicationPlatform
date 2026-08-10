export const compareValues = (a: unknown, b: unknown): number => {
  if (a === b) return 0
  if (a === null || a === undefined) return -1
  if (b === null || b === undefined) return 1

  if (typeof a === 'number' && typeof b === 'number') return a - b

  return String(a).localeCompare(String(b), undefined, {
    numeric: true,
    sensitivity: 'base',
  })
}
