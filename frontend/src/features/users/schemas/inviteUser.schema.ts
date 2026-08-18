import * as z from 'zod';

export const inviteUserSchema = z.object({
  employeeCode: z.string().min(1, 'Employee code is required').max(50, 'Max 50 characters'),
  firstName: z.string().min(1, 'First name is required').max(100, 'Max 100 characters'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Max 100 characters'),
  email: z.string().min(1, 'Email is required').email('Invalid email address').max(255, 'Max 255 characters'),
  username: z
    .string()
    .min(1, 'Username is required')
    .max(100, 'Max 100 characters')
    .regex(/^[a-zA-Z0-9._-]+$/, 'Can only contain letters, numbers, dots, hyphens, and underscores'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Password must contain at least one number')
    .regex(/[^a-zA-Z0-9]/, 'Password must contain at least one special character'),
  phoneNumber: z.string().max(20, 'Max 20 characters').optional().or(z.literal('')),
  departmentId: z.string().min(1, 'Department is required'),
  designationId: z.string().min(1, 'Designation is required'),
  roleIds: z.array(z.string()).min(1, 'At least one role is required'),
  managerId: z.string().optional().or(z.literal('')),
  reportingOfficerId: z.string().optional().or(z.literal('')),
});

export type InviteUserFormValues = z.infer<typeof inviteUserSchema>;
