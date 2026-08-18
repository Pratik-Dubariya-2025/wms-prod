import * as z from 'zod';
import type { RowFilterNode } from '../utils/rowFilterTree';

const rowFilterOperatorSchema = z.enum(['eq', 'neq', 'gt', 'lt', 'gte', 'lte', 'contains', 'in', 'not_in']);

const rowFilterClauseSchema = z.object({
  field: z.string(),
  operator: rowFilterOperatorSchema,
  value: z.string(),
  valueType: z.enum(['static', 'dynamic']).optional(),
});

// Recursive tree: a node is either a clause (leaf) or a group with `logic` + `children`.
// Kept permissive (no non-empty refinements) so the form stays valid while the user is
// still composing the tree; incomplete clauses/groups are pruned on submit via
// `serializeRowFilterNode` instead of blocking the form.
export const rowFilterNodeSchema: z.ZodType<RowFilterNode, RowFilterNode> = z.lazy(() =>
  z.union([
    z.object({
      logic: z.enum(['AND', 'OR']),
      children: z.array(rowFilterNodeSchema),
    }),
    rowFilterClauseSchema,
  ])
);

export const createPolicySchema = z.object({
  name: z.string().min(1, 'Policy name is required').max(200, 'Max 200 characters'),
  description: z.string().max(1000, 'Max 1000 characters').optional().or(z.literal('')),
  targetType: z.enum(['Role', 'User']),
  targetId: z.string().min(1, 'Please select a target role or user'),
  moduleId: z.string().min(1, 'Resource module is required'),
  action: z.string().min(1, 'Allowed action is required'),
  effect: z.enum(['Allow', 'Deny']),
  priority: z
    .number({ error: 'Priority must be a number' })
    .min(1, 'Priority must be at least 1')
    .max(1000, 'Priority cannot exceed 1000'),
  rowFilterExpression: rowFilterNodeSchema.nullable().optional(),
});

export const editPolicySchema = createPolicySchema.extend({
  isActive: z.boolean(),
});

export type CreatePolicyFormValues = z.infer<typeof createPolicySchema>;
export type EditPolicyFormValues = z.infer<typeof editPolicySchema>;
