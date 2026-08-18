/**
 * Shared types + helpers for the recursive AND/OR row-filter expression tree
 * used by `AccessPolicy.RowFilterExpression`. The backend stores/accepts this
 * as a raw JSON string; these helpers parse it into a typed tree for the
 * builder/viewer UI and serialize it back to the wire shape.
 *
 * Wire shape (case-insensitive, duck-typed on the backend):
 *   Clause: { field, operator, value, valueType? }
 *   Group:  { logic: 'AND' | 'OR', children: RowFilterNode[] }
 * A node with a non-empty `children` array is treated as a group by the
 * backend; everything else is treated as a clause. The legacy single-clause
 * shape (PascalCase keys, no `logic`/`children`) is still accepted and
 * normalizes to a bare clause (no wrapping group).
 */

export type RowFilterOperator =
  | 'eq'
  | 'neq'
  | 'gt'
  | 'lt'
  | 'gte'
  | 'lte'
  | 'contains'
  | 'in'
  | 'not_in';

export type RowFilterValueType = 'static' | 'dynamic';

export interface RowFilterClause {
  field: string;
  operator: RowFilterOperator;
  value: string;
  valueType?: RowFilterValueType;
}

export interface RowFilterGroup {
  logic: 'AND' | 'OR';
  children: RowFilterNode[];
}

export type RowFilterNode = RowFilterClause | RowFilterGroup;

export const ROW_FILTER_OPERATORS: { value: RowFilterOperator; label: string }[] = [
  { value: 'eq', label: 'Equals (=)' },
  { value: 'neq', label: 'Not Equals (≠)' },
  { value: 'contains', label: 'Contains' },
  { value: 'in', label: 'In (comma-separated)' },
  { value: 'not_in', label: 'Not In' },
  { value: 'gt', label: 'Greater Than (>)' },
  { value: 'lt', label: 'Less Than (<)' },
  { value: 'gte', label: 'Greater or Equal (≥)' },
  { value: 'lte', label: 'Less or Equal (≤)' },
];

const OPERATOR_SYMBOLS: Record<RowFilterOperator, string> = {
  eq: '=',
  neq: '≠',
  gt: '>',
  lt: '<',
  gte: '≥',
  lte: '≤',
  contains: 'CONTAINS',
  in: 'IN',
  not_in: 'NOT IN',
};

/** Reserved dynamic binding resolving server-side to the caller's org-chart subordinates (walks ManagerId). Only valid with `in`/`not_in`. */
export const SUBORDINATES_BINDING = '{user.SubordinateIds}';

export function isSubordinatesBinding(value: string): boolean {
  return value === SUBORDINATES_BINDING;
}

/**
 * Reserved dynamic binding resolving server-side to the caller's day-to-day reporting team (walks
 * ReportingOfficerId instead of ManagerId). Use this instead of SUBORDINATES_BINDING when ManagerId
 * is a flat/org-wide field in your data and ReportingOfficerId carries the real tiered hierarchy.
 * Only valid with `in`/`not_in`.
 */
export const REPORTING_TEAM_BINDING = '{user.ReportingTeamIds}';

export function isReportingTeamBinding(value: string): boolean {
  return value === REPORTING_TEAM_BINDING;
}

const USER_ATTR_BINDING_RE = /^\{user\.(.+)\}$/;

export function extractUserAttributeBinding(value: string): string | null {
  const match = USER_ATTR_BINDING_RE.exec(value);
  return match ? match[1] : null;
}

export function buildUserAttributeBinding(attribute: string): string {
  return `{user.${attribute}}`;
}

/** Discriminates a node as a group. Based on presence of a `children` array (any length) so freshly-created, still-empty groups remain editable as groups. */
export function isRowFilterGroup(node: RowFilterNode): node is RowFilterGroup {
  return !!node && typeof node === 'object' && Array.isArray((node as RowFilterGroup).children);
}

export function createEmptyClause(): RowFilterClause {
  return { field: '', operator: 'eq', value: '', valueType: 'static' };
}

export function createEmptyGroup(logic: 'AND' | 'OR' = 'AND'): RowFilterGroup {
  return { logic, children: [createEmptyClause()] };
}

function toLowerKeyMap(raw: Record<string, unknown>): Record<string, unknown> {
  const map: Record<string, unknown> = {};
  for (const key of Object.keys(raw)) {
    map[key.toLowerCase()] = raw[key];
  }
  return map;
}

function normalizeRawNode(raw: unknown): RowFilterNode | null {
  if (!raw || typeof raw !== 'object') return null;
  const map = toLowerKeyMap(raw as Record<string, unknown>);
  const rawChildren = map['children'];

  // Backend rule: a non-empty children array makes this a group.
  if (Array.isArray(rawChildren) && rawChildren.length > 0) {
    const logicRaw = String(map['logic'] ?? 'AND').toUpperCase();
    const logic: 'AND' | 'OR' = logicRaw === 'OR' ? 'OR' : 'AND';
    const children = rawChildren
      .map((child) => normalizeRawNode(child))
      .filter((n): n is RowFilterNode => n !== null);
    return { logic, children };
  }

  const field = map['field'];
  const operator = map['operator'];
  const value = map['value'];
  if (field === undefined && operator === undefined && value === undefined) return null;

  const valueTypeRaw = String(map['valuetype'] ?? 'static').toLowerCase();
  const valueType: RowFilterValueType = valueTypeRaw === 'dynamic' ? 'dynamic' : 'static';

  return {
    field: field !== undefined && field !== null ? String(field) : '',
    operator: (operator !== undefined && operator !== null ? String(operator) : 'eq') as RowFilterOperator,
    value: value !== undefined && value !== null ? String(value) : '',
    valueType,
  };
}

/** Safely parses `AccessPolicy.RowFilterExpression` into a typed tree, normalizing legacy flat single-clause JSON. Returns null on empty/invalid input. */
export function parseRowFilterExpression(expression?: string | null): RowFilterNode | null {
  if (!expression || !expression.trim()) return null;
  try {
    const raw = JSON.parse(expression);
    return normalizeRawNode(raw);
  } catch {
    return null;
  }
}

function pruneEmpty(node: RowFilterNode): RowFilterNode | null {
  if (isRowFilterGroup(node)) {
    const children = node.children
      .map((child) => pruneEmpty(child))
      .filter((n): n is RowFilterNode => n !== null);
    if (children.length === 0) return null;
    return { logic: node.logic, children };
  }
  if (!node.field.trim() || !node.value.trim()) return null;
  const clause: RowFilterClause = { field: node.field.trim(), operator: node.operator, value: node.value };
  if (node.valueType && node.valueType !== 'static') clause.valueType = node.valueType;
  return clause;
}

/** Serializes the tree to the JSON string stored in `rowFilterExpression`, pruning incomplete clauses/groups. Returns undefined when nothing meaningful remains (i.e. no filter configured). */
export function serializeRowFilterNode(node: RowFilterNode | null | undefined): string | undefined {
  if (!node) return undefined;
  const pruned = pruneEmpty(node);
  if (!pruned) return undefined;
  return JSON.stringify(pruned);
}

export function describeClauseValue(clause: RowFilterClause): string {
  if (clause.valueType === 'dynamic') {
    if (isSubordinatesBinding(clause.value)) return 'My Subordinates';
    if (isReportingTeamBinding(clause.value)) return 'My Reporting Team';
    const attr = extractUserAttributeBinding(clause.value);
    if (attr) return `My ${attr}`;
    return clause.value || '(empty)';
  }
  if (!clause.value) return '(empty)';
  if (/^-?\d+(\.\d+)?$/.test(clause.value)) return clause.value;
  return `"${clause.value}"`;
}

/** Human-readable single-line description of a clause, e.g. `OwnerId IN My Subordinates`. */
export function describeClause(clause: RowFilterClause): string {
  const symbol = OPERATOR_SYMBOLS[clause.operator] ?? clause.operator;
  return `${clause.field || '(no field)'} ${symbol} ${describeClauseValue(clause)}`;
}
