/**
 * Conditional class name concatenation utility.
 * Filters out falsy values and joins with spaces.
 *
 * Usage: classNames('base', condition && 'conditional', undefined, 'always')
 */
export function classNames(...classes: (string | false | null | undefined)[]): string {
  return classes.filter(Boolean).join(' ');
}
