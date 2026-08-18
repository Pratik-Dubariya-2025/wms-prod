import * as z from 'zod';

export const createRoleSchema = z.object({
  name: z.string().min(1, 'Role name is required').max(50, 'Max 50 characters'),
  code: z
    .string()
    .min(1, 'Role code is required')
    .max(50, 'Max 50 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Can only contain uppercase letters, numbers, and underscores'),
  priority: z
    .number({ error: 'Priority must be a number' })
    .min(1, 'Priority must be at least 1')
    .max(1000, 'Priority cannot exceed 1000'),
  description: z.string().max(256, 'Max 256 characters').optional().or(z.literal('')),
});

export type CreateRoleFormValues = z.infer<typeof createRoleSchema>;
