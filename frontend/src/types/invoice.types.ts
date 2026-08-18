export type InvoiceStatus = 'Draft' | 'Sent' | 'Paid' | 'Overdue' | 'Cancelled';

export interface Invoice {
  id: string;
  invoiceNumber: string;
  leadId: string;
  companyName: string;
  contactName: string;
  amount: number;
  status: InvoiceStatus;
  issuedDate: string;
  dueDate: string;
  createdAt: string;
}

export interface CreateInvoiceCommand {
  leadId: string;
  amount?: number;
  dueDays?: number;
}
