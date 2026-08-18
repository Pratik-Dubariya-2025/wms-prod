export type LeadStage = 'NewLead' | 'Qualified' | 'Proposal' | 'Negotiation' | 'ClosedWon' | 'ClosedLost';

export interface Lead {
  id: string;
  companyName: string;
  contactName: string;
  contactEmail?: string;
  contactPhone?: string;
  stage: LeadStage;
  estimatedValue?: number;
  ownerId: string;
  ownerName: string;
  region?: string;
  source?: string;
  closedAt?: string;
  createdAt: string;
}

export interface CreateLeadCommand {
  companyName: string;
  contactName: string;
  contactEmail?: string;
  contactPhone?: string;
  estimatedValue?: number;
  region?: string;
  source?: string;
}

export interface UpdateLeadStageCommand {
  stage: LeadStage;
}
