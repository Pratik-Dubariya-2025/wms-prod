/**
 * Formats a long GUID/UUID or ID into a clean, shortened string for UI display.
 * Example: "a1000000-0000-0000-0000-000000000001" -> "#a1000000"
 */
export function formatId(id?: string | null, length: number = 8): string {
  if (!id) return '';
  if (id.includes('-')) {
    const firstPart = id.split('-')[0];
    return `#${firstPart}`;
  }
  if (id.length > length) {
    return `#${id.slice(0, length)}`;
  }
  return `#${id}`;
}
