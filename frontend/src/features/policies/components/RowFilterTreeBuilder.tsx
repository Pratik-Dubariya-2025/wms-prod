import { Plus, Trash2 } from 'lucide-react';
import { classNames } from '@/utils/classNames';
import { usePolicyAttributes } from '../hooks/usePolicyAttributes';
import {
  REPORTING_TEAM_BINDING,
  ROW_FILTER_OPERATORS,
  SUBORDINATES_BINDING,
  buildUserAttributeBinding,
  createEmptyClause,
  createEmptyGroup,
  extractUserAttributeBinding,
  isReportingTeamBinding,
  isRowFilterGroup,
  isSubordinatesBinding,
  type RowFilterClause,
  type RowFilterNode,
  type RowFilterOperator,
} from '../utils/rowFilterTree';

interface RowFilterTreeBuilderProps {
  value: RowFilterNode;
  onChange: (node: RowFilterNode) => void;
  /** Module CODE (not id) of the currently selected resource module, used to fetch attribute suggestions. */
  moduleCode?: string;
}

interface SharedAttrProps {
  userAttributes: string[];
  resourceAttributes: string[];
  moduleSupported: boolean;
  moduleSelected: boolean;
}

export function RowFilterTreeBuilder({ value, onChange, moduleCode }: RowFilterTreeBuilderProps) {
  const { data: attributes, isFetching } = usePolicyAttributes(moduleCode);
  const userAttributes = attributes?.userAttributes ?? [];
  const resourceAttributes = attributes?.resourceAttributes ?? [];
  const moduleSupported = attributes?.moduleSupported ?? false;

  return (
    <div className="space-y-2">
      {!moduleCode ? (
        <p className="text-[11px] text-wms-muted italic">Select a resource module above to enable field/attribute suggestions.</p>
      ) : isFetching ? (
        <div className="h-8 bg-wms-hover rounded-lg animate-pulse" />
      ) : null}
      <RowFilterNodeEditor
        node={value}
        onChange={onChange}
        onRemove={null}
        depth={0}
        userAttributes={userAttributes}
        resourceAttributes={resourceAttributes}
        moduleSupported={moduleSupported}
        moduleSelected={!!moduleCode}
      />
    </div>
  );
}

interface RowFilterNodeEditorProps extends SharedAttrProps {
  node: RowFilterNode;
  onChange: (node: RowFilterNode) => void;
  onRemove: (() => void) | null;
  depth: number;
}

function RowFilterNodeEditor({ node, onChange, onRemove, depth, ...attrProps }: RowFilterNodeEditorProps) {
  if (isRowFilterGroup(node)) {
    const updateLogic = (logic: 'AND' | 'OR') => onChange({ ...node, logic });
    const updateChild = (idx: number, child: RowFilterNode) => {
      const children = node.children.slice();
      children[idx] = child;
      onChange({ ...node, children });
    };
    const removeChild = (idx: number) => {
      onChange({ ...node, children: node.children.filter((_, i) => i !== idx) });
    };
    const addClause = () => onChange({ ...node, children: [...node.children, createEmptyClause()] });
    const addGroup = () => onChange({ ...node, children: [...node.children, createEmptyGroup()] });

    return (
      <div
        className={classNames(
          'rounded-lg border p-3 space-y-3',
          depth === 0 ? 'border-wms-border bg-wms-hover/30' : 'border-wms-border bg-wms-hover/50'
        )}
      >
        <div className="flex items-center justify-between gap-2">
          <div className="inline-flex rounded-lg border border-wms-border overflow-hidden">
            <button
              type="button"
              onClick={() => updateLogic('AND')}
              className={classNames(
                'px-3 py-1 text-[11px] font-bold uppercase tracking-wider transition cursor-pointer',
                node.logic === 'AND' ? 'bg-wms-indigo text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              AND
            </button>
            <button
              type="button"
              onClick={() => updateLogic('OR')}
              className={classNames(
                'px-3 py-1 text-[11px] font-bold uppercase tracking-wider transition cursor-pointer',
                node.logic === 'OR' ? 'bg-wms-purple text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              OR
            </button>
          </div>
          {onRemove && (
            <button
              type="button"
              onClick={onRemove}
              title="Remove group"
              className="text-wms-danger hover:text-red-500 cursor-pointer"
            >
              <Trash2 className="h-3.5 w-3.5" />
            </button>
          )}
        </div>

        <div className="space-y-2 pl-3 border-l-2 border-wms-border">
          {node.children.length === 0 ? (
            <p className="text-[11px] text-wms-muted italic">Empty group - add a clause or nested group.</p>
          ) : (
            node.children.map((child, idx) => (
              <RowFilterNodeEditor
                key={idx}
                node={child}
                onChange={(c) => updateChild(idx, c)}
                onRemove={() => removeChild(idx)}
                depth={depth + 1}
                {...attrProps}
              />
            ))
          )}
        </div>

        <div className="flex gap-2">
          <button
            type="button"
            onClick={addClause}
            className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-wms-hover border border-wms-border text-[11px] font-semibold text-wms-secondary hover:text-wms-text hover:border-wms-indigo/40 transition cursor-pointer"
          >
            <Plus className="h-3 w-3" /> Clause
          </button>
          <button
            type="button"
            onClick={addGroup}
            className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-wms-hover border border-wms-border text-[11px] font-semibold text-wms-secondary hover:text-wms-text hover:border-wms-indigo/40 transition cursor-pointer"
          >
            <Plus className="h-3 w-3" /> Group
          </button>
        </div>
      </div>
    );
  }

  return <RowFilterClauseEditor clause={node} onChange={onChange} onRemove={onRemove} {...attrProps} />;
}

type ValueMode = 'static' | 'attribute' | 'subordinates' | 'reportingTeam';

function deriveValueMode(clause: RowFilterClause): ValueMode {
  if (clause.valueType === 'dynamic') {
    if (isSubordinatesBinding(clause.value)) return 'subordinates';
    if (isReportingTeamBinding(clause.value)) return 'reportingTeam';
    return 'attribute';
  }
  return 'static';
}

interface RowFilterClauseEditorProps extends SharedAttrProps {
  clause: RowFilterClause;
  onChange: (node: RowFilterNode) => void;
  onRemove: (() => void) | null;
}

function RowFilterClauseEditor({
  clause,
  onChange,
  onRemove,
  userAttributes,
  resourceAttributes,
  moduleSupported,
  moduleSelected,
}: RowFilterClauseEditorProps) {
  const mode = deriveValueMode(clause);
  const canUseHierarchyBinding = clause.operator === 'in' || clause.operator === 'not_in';
  const patch = (partial: Partial<RowFilterClause>) => onChange({ ...clause, ...partial });

  const handleOperatorChange = (operator: RowFilterOperator) => {
    if ((mode === 'subordinates' || mode === 'reportingTeam') && operator !== 'in' && operator !== 'not_in') {
      // Hierarchy bindings are only valid with in/not_in - fall back to static.
      patch({ operator, value: '', valueType: 'static' });
    } else {
      patch({ operator });
    }
  };

  const handleModeChange = (nextMode: ValueMode) => {
    if (nextMode === 'static') {
      patch({ value: '', valueType: 'static' });
    } else if (nextMode === 'attribute') {
      const firstAttr = userAttributes[0] ?? '';
      patch({ value: firstAttr ? buildUserAttributeBinding(firstAttr) : '', valueType: 'dynamic' });
    } else if (nextMode === 'subordinates') {
      patch({ value: SUBORDINATES_BINDING, valueType: 'dynamic' });
    } else {
      patch({ value: REPORTING_TEAM_BINDING, valueType: 'dynamic' });
    }
  };

  const useFieldSelect = moduleSelected && moduleSupported && resourceAttributes.length > 0;

  return (
    <div className="flex flex-wrap items-start gap-2 p-2.5 rounded-lg bg-wms-hover border border-wms-border">
      {/* Field */}
      <div className="space-y-1 min-w-35">
        <label className="text-[9px] font-bold text-wms-muted uppercase">Field</label>
        {useFieldSelect ? (
          <select
            value={clause.field}
            onChange={(e) => patch({ field: e.target.value })}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-2.5 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none cursor-pointer"
          >
            <option value="">-- Field --</option>
            {resourceAttributes.map((attr) => (
              <option key={attr} value={attr}>{attr}</option>
            ))}
          </select>
        ) : (
          <>
            <input
              type="text"
              placeholder="e.g. OwnerId"
              value={clause.field}
              onChange={(e) => patch({ field: e.target.value })}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-2.5 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
            />
            {moduleSelected && !moduleSupported && (
              <span className="text-[9px] text-amber-400 block leading-snug">This module isn't attribute-validated - check spelling carefully.</span>
            )}
          </>
        )}
      </div>

      {/* Operator */}
      <div className="space-y-1 min-w-32.5">
        <label className="text-[9px] font-bold text-wms-muted uppercase">Operator</label>
        <select
          value={clause.operator}
          onChange={(e) => handleOperatorChange(e.target.value as RowFilterOperator)}
          className="w-full rounded-lg bg-wms-hover border border-wms-border px-2.5 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none cursor-pointer"
        >
          {ROW_FILTER_OPERATORS.map((op) => (
            <option key={op.value} value={op.value}>{op.label}</option>
          ))}
        </select>
      </div>

      {/* Value source */}
      <div className="space-y-1 flex-1 min-w-55">
        <label className="text-[9px] font-bold text-wms-muted uppercase">Value</label>
        <div className="flex flex-wrap items-center gap-1.5">
          <div className="inline-flex rounded-lg border border-wms-border overflow-hidden shrink-0">
            <button
              type="button"
              onClick={() => handleModeChange('static')}
              className={classNames(
                'px-2 py-1 text-[10px] font-semibold transition cursor-pointer',
                mode === 'static' ? 'bg-wms-indigo text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              Static
            </button>
            <button
              type="button"
              onClick={() => handleModeChange('attribute')}
              className={classNames(
                'px-2 py-1 text-[10px] font-semibold transition cursor-pointer',
                mode === 'attribute' ? 'bg-wms-indigo text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              My Attribute
            </button>
            <button
              type="button"
              disabled={!canUseHierarchyBinding}
              onClick={() => handleModeChange('subordinates')}
              title={!canUseHierarchyBinding ? 'Only available with In / Not In operators' : 'Walks the Manager chain (ManagerId)'}
              className={classNames(
                'px-2 py-1 text-[10px] font-semibold transition',
                canUseHierarchyBinding ? 'cursor-pointer' : 'cursor-not-allowed opacity-40',
                mode === 'subordinates' ? 'bg-wms-indigo text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              My Subordinates
            </button>
            <button
              type="button"
              disabled={!canUseHierarchyBinding}
              onClick={() => handleModeChange('reportingTeam')}
              title={!canUseHierarchyBinding ? 'Only available with In / Not In operators' : 'Walks the Reporting Officer chain (ReportingOfficerId) - use this instead when Manager is a flat, org-wide field in your data'}
              className={classNames(
                'px-2 py-1 text-[10px] font-semibold transition',
                canUseHierarchyBinding ? 'cursor-pointer' : 'cursor-not-allowed opacity-40',
                mode === 'reportingTeam' ? 'bg-wms-indigo text-white' : 'bg-wms-hover text-wms-secondary hover:text-wms-text'
              )}
            >
              My Reporting Team
            </button>
          </div>

          {mode === 'static' && (
            <input
              type="text"
              placeholder="Literal value..."
              value={clause.value}
              onChange={(e) => patch({ value: e.target.value, valueType: 'static' })}
              className="flex-1 min-w-30 rounded-lg bg-wms-hover border border-wms-border px-2.5 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
            />
          )}
          {mode === 'attribute' && (
            <select
              value={extractUserAttributeBinding(clause.value) ?? ''}
              onChange={(e) => patch({ value: buildUserAttributeBinding(e.target.value), valueType: 'dynamic' })}
              className="flex-1 min-w-30 rounded-lg bg-wms-hover border border-wms-border px-2.5 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none cursor-pointer"
            >
              <option value="">-- My Attribute --</option>
              {userAttributes.map((attr) => (
                <option key={attr} value={attr}>{attr}</option>
              ))}
            </select>
          )}
          {mode === 'subordinates' && (
            <span className="text-xs text-wms-indigo font-medium bg-wms-indigo/10 px-2.5 py-1.5 rounded-lg border border-wms-indigo/20">
              Current user's org-chart subordinates (via Manager)
            </span>
          )}
          {mode === 'reportingTeam' && (
            <span className="text-xs text-wms-indigo font-medium bg-wms-indigo/10 px-2.5 py-1.5 rounded-lg border border-wms-indigo/20">
              Current user's day-to-day reporting team (via Reporting Officer)
            </span>
          )}
        </div>
      </div>

      {onRemove && (
        <button
          type="button"
          onClick={onRemove}
          title="Remove clause"
          className="text-wms-danger hover:text-red-500 cursor-pointer mt-5"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      )}
    </div>
  );
}
