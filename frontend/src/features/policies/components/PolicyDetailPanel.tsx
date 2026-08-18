import { useState } from 'react';
import {
  ShieldAlert,
  AlertCircle,
  Edit2,
  Trash2,
  Lock,
  UserCheck,
  Sliders,
  MinusCircle,
  PlusCircle,
  Eye,
  BookOpen
} from 'lucide-react';
import { Switch } from '@/components/ui/Switch';
import { formatId } from '@/utils/formatId';
import { classNames } from '@/utils/classNames';
import type { PolicyDetailDto } from '@/api/policiesApi';
import { usePolicyAttributes } from '../hooks/usePolicyAttributes';
import { RowFilterTreeView } from './RowFilterTreeView';
import { PolicyGuideModal } from './PolicyGuideModal';
import { parseRowFilterExpression } from '../utils/rowFilterTree';
import { GUIDE_HEADINGS } from '../content/policyGuideContent';

interface PolicyDetailPanelProps {
  policy: PolicyDetailDto | null;
  isLoadingDetail: boolean;
  detailError: string | null;
  onOpenEdit: () => void;
  onDelete: () => void;
  onToggleActive: (val: boolean) => Promise<void>;
  onAddCondition: (group: number, attribute: string, operator: string, value: string) => Promise<void>;
  onRemoveCondition: (condId: string) => void;
  onAddFieldRestriction: (fieldName: string, type: string, pattern: string) => Promise<void>;
  onRemoveFieldRestriction: (restId: string) => void;
  canUpdate?: boolean;
  canDelete?: boolean;
}

export function PolicyDetailPanel({
  policy,
  isLoadingDetail,
  detailError,
  onOpenEdit,
  onDelete,
  onToggleActive,
  onAddCondition,
  onRemoveCondition,
  onAddFieldRestriction,
  onRemoveFieldRestriction,
  canUpdate = false,
  canDelete = false
}: PolicyDetailPanelProps) {
  // Policy Guide state (shared modal, opened with a specific section depending on entry point)
  const [guideSection, setGuideSection] = useState<string | null>(null);

  // Condition Form State
  const [newCondGroup, setNewCondGroup] = useState(1);
  const [newCondAttr, setNewCondAttr] = useState('');
  const [newCondOp, setNewCondOp] = useState('eq');
  const [newCondVal, setNewCondVal] = useState('');
  const [isAddingCond, setIsAddingCond] = useState(false);

  // Field Restriction Form State
  const [newFieldRestName, setNewFieldRestName] = useState('');
  const [newFieldRestType, setNewFieldRestType] = useState('Hide');
  const [newFieldRestPattern, setNewFieldRestPattern] = useState('');
  const [isAddingRest, setIsAddingRest] = useState(false);

  // Attribute suggestions for this policy's module (used by the Condition "Left Attribute" datalist)
  const { data: policyAttributes } = usePolicyAttributes(policy?.moduleCode);
  const attributeDatalistOptions = [
    ...(policyAttributes?.userAttributes ?? []).map((attr) => `user.${attr}`),
    ...(policyAttributes?.resourceAttributes ?? []).map((attr) => `resource.${attr}`),
  ];

  const handleAddConditionSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCondAttr.trim() || !newCondVal.trim()) return;
    setIsAddingCond(true);
    try {
      await onAddCondition(newCondGroup, newCondAttr.trim(), newCondOp, newCondVal.trim());
      setNewCondAttr('');
      setNewCondVal('');
    } finally {
      setIsAddingCond(false);
    }
  };

  const handleAddFieldRestSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newFieldRestName.trim()) return;
    setIsAddingRest(true);
    try {
      await onAddFieldRestriction(newFieldRestName.trim(), newFieldRestType, newFieldRestPattern.trim());
      setNewFieldRestName('');
      setNewFieldRestPattern('');
    } finally {
      setIsAddingRest(false);
    }
  };

  if (detailError) {
    return (
      <div className="glass-card rounded-xl p-8 text-center text-wms-danger">
        <AlertCircle className="h-10 w-10 mx-auto text-wms-danger mb-3 opacity-80" />
        <p className="text-sm font-semibold">{detailError}</p>
      </div>
    );
  }

  if (isLoadingDetail) {
    return (
      <div className="glass-card rounded-xl p-8 space-y-4">
        <div className="h-12 bg-wms-hover rounded-lg animate-pulse" />
        <div className="h-32 bg-wms-hover rounded-lg animate-pulse" />
        <div className="h-32 bg-wms-hover rounded-lg animate-pulse" />
      </div>
    );
  }

  if (!policy) {
    return (
      <div className="glass-card rounded-xl p-16 text-center text-wms-muted">
        <ShieldAlert className="h-10 w-10 mx-auto text-wms-muted mb-3 opacity-50" />
        Select an access policy from the left list to manage details.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Detail Overview Card */}
      <div className="glass-card rounded-xl p-6 space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-wms-border">
          <div>
            <h2 className="text-xl font-bold text-wms-text flex items-center gap-2 flex-wrap">
              {policy.name}
              <span className={classNames(
                'text-xs uppercase font-bold tracking-wider px-2.5 py-1 rounded border',
                policy.effect === 'Allow'
                  ? 'bg-wms-success/15 text-wms-success border-wms-success/20'
                  : 'bg-wms-danger/15 text-wms-danger border-wms-danger/20'
              )}>
                {policy.effect} Effect
              </span>
            </h2>
            <p className="text-sm text-wms-secondary mt-1">{policy.description || 'No description provided.'}</p>
            <p className="text-xs text-wms-muted mt-2 font-mono">ID: {formatId(policy.id)}</p>
          </div>
          {(canUpdate || canDelete) && (
            <div className="flex items-center gap-2 self-start sm:self-center">
              {canUpdate && (
                <button
                  onClick={onOpenEdit}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-wms-hover border border-wms-border hover:bg-wms-hover text-xs font-semibold text-wms-text transition cursor-pointer"
                >
                  <Edit2 className="h-3.5 w-3.5" />
                  Edit
                </button>
              )}
              {canDelete && (
                <button
                  onClick={onDelete}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-wms-danger/10 border border-wms-danger/20 hover:bg-wms-danger hover:text-white text-xs font-semibold text-wms-danger transition cursor-pointer"
                >
                  <Trash2 className="h-3.5 w-3.5" />
                  Delete
                </button>
              )}
            </div>
          )}
        </div>

        {/* Metadata details grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div className="space-y-1">
            <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Module & Action</span>
            <div className="text-wms-text font-medium flex items-center gap-1.5">
              <Lock className="h-4 w-4 text-wms-indigo" />
              {policy.moduleName} ({policy.moduleCode})
              <code className="text-xs bg-wms-indigo/10 text-wms-indigo px-1.5 py-0.5 rounded font-mono font-medium">{policy.action}</code>
            </div>
          </div>

          <div className="space-y-1">
            <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Target Assignment</span>
            <div className="text-wms-text font-medium flex items-center gap-1.5">
              <UserCheck className="h-4 w-4 text-wms-indigo" />
              {policy.userName ? (
                <span>User: <strong className="text-wms-text">{policy.userName}</strong></span>
              ) : (
                <span>Role: <strong className="text-wms-text">{policy.roleName}</strong></span>
              )}
            </div>
          </div>

          <div className="space-y-1">
            <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Evaluation Priority</span>
            <div className="text-wms-text font-medium">
              Priority weight: <strong className="text-wms-indigo">{policy.priority}</strong> (higher priority evaluated first)
            </div>
          </div>

          <div className="space-y-1">
            <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Policy Status</span>
            <div className="flex items-center gap-3 mt-1">
              {canUpdate ? (
                <>
                  <Switch
                    checked={policy.isActive}
                    onChange={onToggleActive}
                    ariaLabel="Toggle policy active status"
                  />
                  <span className="text-sm font-semibold text-wms-text">
                    {policy.isActive ? 'Active / Enabled' : 'Disabled'}
                  </span>
                </>
              ) : (
                <span className={classNames(
                  'px-2 py-0.5 rounded font-medium text-xs',
                  policy.isActive ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
                )}>
                  {policy.isActive ? 'Active' : 'Disabled'}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Row Filter Expression */}
        <div className="pt-4 border-t border-wms-border space-y-2">
          <div className="flex items-center justify-between">
            <h3 className="text-xs font-semibold text-wms-muted uppercase tracking-wider flex items-center gap-1.5">
              <Sliders className="h-4 w-4 text-wms-indigo" />
              Row-Level Data Filter (JSON Expression)
            </h3>
            <button
              type="button"
              onClick={() => setGuideSection(GUIDE_HEADINGS.rowFilter)}
              className="inline-flex items-center gap-1 text-[11px] font-semibold text-wms-indigo hover:underline cursor-pointer"
            >
              <BookOpen className="h-3 w-3" />
              Guide
            </button>
          </div>
          <RowFilterTreeView node={parseRowFilterExpression(policy.rowFilterExpression)} rawExpression={policy.rowFilterExpression} />
        </div>
      </div>

      {/* Conditions Card */}
      <div className="glass-card rounded-xl p-6 space-y-4">
        <div className="flex items-center justify-between pb-3 border-b border-wms-border">
          <h3 className="text-sm font-bold text-wms-text uppercase tracking-wider flex items-center gap-2">
            <Sliders className="h-4.5 w-4.5 text-wms-indigo" />
            Policy Conditions (AND/OR Groups)
          </h3>
          <button
            type="button"
            onClick={() => setGuideSection(GUIDE_HEADINGS.conditions)}
            className="inline-flex items-center gap-1 text-[11px] font-semibold text-wms-indigo hover:underline cursor-pointer"
          >
            <BookOpen className="h-3 w-3" />
            Guide
          </button>
        </div>

        {/* Conditions Table */}
        <div className="space-y-3">
          {policy.conditions.length === 0 ? (
            <p className="text-xs text-wms-muted italic py-2">No custom dynamic conditions defined. Policy applies unconditionally based on target role/user.</p>
          ) : (
            <div className="border border-wms-border rounded-lg overflow-x-auto">
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-wms-hover border-b border-wms-border text-wms-secondary font-semibold">
                    <th className="p-3 whitespace-nowrap">Group (OR)</th>
                    <th className="p-3 whitespace-nowrap">Attribute (Left)</th>
                    <th className="p-3 whitespace-nowrap">Operator</th>
                    <th className="p-3 whitespace-nowrap">Value (Right)</th>
                    {canUpdate && <th className="p-3 text-right whitespace-nowrap">Action</th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-wms-border text-wms-text">
                  {policy.conditions.map((cond) => (
                    <tr key={cond.id} className="hover:bg-wms-hover/40 transition">
                      <td className="p-3 whitespace-nowrap">
                        <span className="bg-wms-indigo/10 text-wms-indigo px-2 py-0.5 rounded font-bold">
                          Group {cond.conditionGroup}
                        </span>
                      </td>
                      <td className="p-3 font-mono min-w-[120px] break-all">{cond.attribute}</td>
                      <td className="p-3 font-semibold text-wms-indigo whitespace-nowrap">{cond.operator}</td>
                      <td className="p-3 font-mono break-all whitespace-normal min-w-[180px] max-w-xs md:max-w-md">{cond.value}</td>
                      {canUpdate && (
                        <td className="p-3 text-right whitespace-nowrap">
                          <button
                            type="button"
                            onClick={() => onRemoveCondition(cond.id)}
                            className="text-wms-danger hover:text-red-500 font-semibold cursor-pointer"
                            title="Delete condition"
                          >
                            <MinusCircle className="h-4 w-4 inline" />
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Add Condition Inline Form */}
          {canUpdate && (
            <form onSubmit={handleAddConditionSubmit} className="grid grid-cols-1 sm:grid-cols-5 gap-3 pt-3 border-t border-wms-border items-end">
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Group</label>
                <input
                  type="number"
                  min="1"
                  value={newCondGroup}
                  onChange={(e) => setNewCondGroup(Number(e.target.value))}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
                />
              </div>
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Left Attribute</label>
                <input
                  type="text"
                  placeholder="e.g. user.DepartmentId"
                  value={newCondAttr}
                  onChange={(e) => setNewCondAttr(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
                  list="policy-condition-attribute-suggestions"
                  required
                />
                <datalist id="policy-condition-attribute-suggestions">
                  {attributeDatalistOptions.map((attr) => (
                    <option key={attr} value={attr} />
                  ))}
                </datalist>
              </div>
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Operator</label>
                <select
                  value={newCondOp}
                  onChange={(e) => setNewCondOp(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none cursor-pointer"
                >
                  <option value="eq">Equals (eq)</option>
                  <option value="neq">Not Equals (neq)</option>
                  <option value="contains">Contains</option>
                  <option value="in">In (comma-separated)</option>
                  <option value="not_in">Not In</option>
                  <option value="gt">Greater Than (gt)</option>
                  <option value="lt">Less Than (lt)</option>
                  <option value="gte">Greater or Equal (gte)</option>
                  <option value="lte">Less or Equal (lte)</option>
                </select>
              </div>
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Right Value</label>
                <input
                  type="text"
                  placeholder="e.g. resource.DepartmentId"
                  value={newCondVal}
                  onChange={(e) => setNewCondVal(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
                  required
                />
              </div>
              <button
                type="submit"
                disabled={isAddingCond}
                className="w-full inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-lg bg-wms-indigo text-white text-xs font-semibold hover:bg-indigo-700 transition cursor-pointer disabled:opacity-50"
              >
                <PlusCircle className="h-3.5 w-3.5" />
                Add Rule
              </button>
            </form>
          )}
        </div>
      </div>

      {/* Column Restrictions Card */}
      <div className="glass-card rounded-xl p-6 space-y-4">
        <div className="flex items-center justify-between pb-3 border-b border-wms-border">
          <h3 className="text-sm font-bold text-wms-text uppercase tracking-wider flex items-center gap-2">
            <Eye className="h-4.5 w-4.5 text-wms-indigo" />
            Column-Level Field Restrictions
          </h3>
          <button
            type="button"
            onClick={() => setGuideSection(GUIDE_HEADINGS.fieldRestrictions)}
            className="inline-flex items-center gap-1 text-[11px] font-semibold text-wms-indigo hover:underline cursor-pointer"
          >
            <BookOpen className="h-3 w-3" />
            Guide
          </button>
        </div>

        {/* Field Restrictions Table */}
        <div className="space-y-3">
          {policy.fieldRestrictions.length === 0 ? (
            <p className="text-xs text-wms-muted italic py-2">No column-level or field-level restrictions defined. User sees all entity properties.</p>
          ) : (
            <div className="border border-wms-border rounded-lg overflow-x-auto">
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-wms-hover border-b border-wms-border text-wms-secondary font-semibold">
                    <th className="p-3 whitespace-nowrap">Field / Column Name</th>
                    <th className="p-3 whitespace-nowrap">Restriction Type</th>
                    <th className="p-3 whitespace-nowrap">Mask Pattern</th>
                    {canUpdate && <th className="p-3 text-right whitespace-nowrap">Action</th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-wms-border text-wms-text">
                  {policy.fieldRestrictions.map((rest) => (
                    <tr key={rest.id} className="hover:bg-wms-hover/40 transition">
                      <td className="p-3 font-semibold font-mono min-w-[120px] break-all">{rest.fieldName}</td>
                      <td className="p-3 whitespace-nowrap">
                        <span className={classNames(
                           'px-2 py-0.5 rounded font-medium',
                           rest.restrictionType === 'Hide' && 'bg-red-500/10 text-red-400',
                           rest.restrictionType === 'Mask' && 'bg-amber-500/10 text-amber-400',
                           rest.restrictionType === 'ReadOnly' && 'bg-blue-500/10 text-blue-400'
                         )}>
                          {rest.restrictionType}
                        </span>
                      </td>
                      <td className="p-3 font-mono text-wms-muted min-w-[100px] break-all">{rest.maskPattern || 'N/A'}</td>
                      {canUpdate && (
                        <td className="p-3 text-right whitespace-nowrap">
                          <button
                            type="button"
                            onClick={() => onRemoveFieldRestriction(rest.id)}
                            className="text-wms-danger hover:text-red-500 font-semibold cursor-pointer"
                            title="Delete restriction"
                          >
                            <MinusCircle className="h-4 w-4 inline" />
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Add Field Restriction Form */}
          {canUpdate && (
            <form onSubmit={handleAddFieldRestSubmit} className="grid grid-cols-1 sm:grid-cols-4 gap-3 pt-3 border-t border-wms-border items-end">
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Field Name</label>
                <input
                  type="text"
                  placeholder="e.g. Salary"
                  value={newFieldRestName}
                  onChange={(e) => setNewFieldRestName(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none"
                  required
                />
              </div>
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Type</label>
                <select
                  value={newFieldRestType}
                  onChange={(e) => setNewFieldRestType(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none cursor-pointer"
                >
                  <option value="Hide">Hide (Remove completely)</option>
                  <option value="Mask">Mask (Partial obfuscation)</option>
                  <option value="ReadOnly">Read-Only (Lock field)</option>
                </select>
              </div>
              <div className="space-y-1">
                <label className="text-[10px] font-bold text-wms-muted uppercase">Mask Pattern (Optional)</label>
                <input
                  type="text"
                  placeholder="e.g. **** or ##-##"
                  disabled={newFieldRestType !== 'Mask'}
                  value={newFieldRestPattern}
                  onChange={(e) => setNewFieldRestPattern(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text focus:border-wms-indigo outline-none disabled:opacity-50"
                />
              </div>
              <button
                type="submit"
                disabled={isAddingRest}
                className="w-full inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-lg bg-wms-indigo text-white text-xs font-semibold hover:bg-indigo-700 transition cursor-pointer disabled:opacity-50"
              >
                <PlusCircle className="h-3.5 w-3.5" />
                Add Lock
              </button>
            </form>
          )}
        </div>
      </div>

      <PolicyGuideModal
        isOpen={guideSection !== null}
        onClose={() => setGuideSection(null)}
        initialSectionHeading={guideSection ?? undefined}
      />
    </div>
  );
}
