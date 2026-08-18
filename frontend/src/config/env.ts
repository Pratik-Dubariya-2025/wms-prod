// Typed wrapper over import.meta.env for safe access to env variables

export const env = {
  /** Base URL for the backend API */
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL as string || '/api',

  /** Application name */
  APP_NAME: import.meta.env.VITE_APP_NAME as string || 'WMS',

  /** Current environment mode */
  MODE: import.meta.env.MODE as string,

  /** Is development mode */
  IS_DEV: import.meta.env.DEV as boolean,

  /** Is production mode */
  IS_PROD: import.meta.env.PROD as boolean,
} as const;
