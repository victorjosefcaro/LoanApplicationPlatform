export const paymentKeys = {
  all: ['payments'] as const,
  lists: () => [...paymentKeys.all, 'list'] as const,
  list: (loanApplicationId: number) => [...paymentKeys.lists(), loanApplicationId] as const,
}
