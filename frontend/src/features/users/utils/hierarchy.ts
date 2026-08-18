import type { InviteMeta } from '../types/user.types';

/**
 * Mirrors the tier logic in sp_InviteUser so the form shows/requires the right
 * fields without hardcoded level numbers:
 *   - top tier   = department head (manager)   -> no manager / RO / team
 *   - lead tier  = team lead (top - 1)         -> manager required, team auto-created
 *   - below lead = developer (< top - 1)       -> manager + reporting officer required
 */
export interface HierarchyTier {
  designationLevel: number;
  deptTopLevel: number;
  isTopTier: boolean;
  isLeadTier: boolean;
  isBelowLead: boolean;
  needsManager: boolean;
  needsReportingOfficer: boolean;
}

export function resolveHierarchyTier(
  meta: InviteMeta | undefined,
  departmentId: string,
  designationId: string,
): HierarchyTier {
  const deptDesignations = (meta?.designations ?? []).filter((d) => d.departmentId === departmentId);
  const deptTopLevel = deptDesignations.reduce((max, d) => Math.max(max, d.level), 0);
  const designationLevel = deptDesignations.find((d) => d.id === designationId)?.level ?? 0;

  const isTopTier = designationLevel > 0 && designationLevel >= deptTopLevel;
  const isLeadTier = designationLevel > 0 && !isTopTier && designationLevel === deptTopLevel - 1;
  const isBelowLead = designationLevel > 0 && !isTopTier && designationLevel < deptTopLevel - 1;

  return {
    designationLevel,
    deptTopLevel,
    isTopTier,
    isLeadTier,
    isBelowLead,
    needsManager: isLeadTier || isBelowLead,
    needsReportingOfficer: isBelowLead,
  };
}
