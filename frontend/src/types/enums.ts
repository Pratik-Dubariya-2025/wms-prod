// Placeholder for application-wide enums
// Add feature-specific enums here as backend features are implemented

export const EntityStatus = {
  Active: 'Active',
  Inactive: 'Inactive',
} as const;

export type EntityStatus = typeof EntityStatus[keyof typeof EntityStatus];
