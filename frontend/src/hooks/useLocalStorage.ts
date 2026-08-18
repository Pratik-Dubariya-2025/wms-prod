import { useState, useEffect } from 'react';

/**
 * Hook that syncs a value with localStorage.
 * Falls back to initialValue if nothing stored or parsing fails.
 */
export function useLocalStorage<T>(key: string, initialValue: T) {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? (JSON.parse(item) as T) : initialValue;
    } catch {
      return initialValue;
    }
  });

  useEffect(() => {
    try {
      window.localStorage.setItem(key, JSON.stringify(storedValue));
    } catch {
      // Silently fail on storage quota errors
    }
  }, [key, storedValue]);

  return [storedValue, setStoredValue] as const;
}
