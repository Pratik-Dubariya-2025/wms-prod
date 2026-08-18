/**
 * Formats a number as Indian Rupee currency.
 */
export function formatCurrency(
  amount: number | null | undefined,
  currency: string = 'INR',
  locale: string = 'en-IN',
): string {
  if (amount == null) return '—';

  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}
