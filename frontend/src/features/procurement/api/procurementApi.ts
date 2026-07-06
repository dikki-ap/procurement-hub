import { apiClient } from '@/shared/lib/axios';

export type PRStatus       = 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Cancelled';
export type RFQStatus      = 'Draft' | 'Open' | 'Closed' | 'Cancelled';
export type RFQVendorStatus = 'Invited' | 'Declined' | 'Submitted';

export interface PRItemDto {
  id: string;
  materialId: string | null;
  itemDescription: string;
  quantity: number;
  unitOfMeasureId: string | null;
  unitLabel: string | null;
  estimatedUnitPrice: number;
  lineTotal: number;
  notes: string | null;
}

export interface PRListDto {
  id: string;
  prNumber: string;
  title: string;
  department: string;
  requiredDate: string;
  status: PRStatus;
  totalEstimatedValue: number;
  itemCount: number;
  createdAt: string;
}

export interface PRDto extends PRListDto {
  description: string | null;
  deliveryLocation: string | null;
  notes: string | null;
  requestedById: string;
  updatedAt: string;
  items: PRItemDto[];
}

export interface RFQItemDto {
  id: string;
  prItemId: string | null;
  materialId: string | null;
  itemDescription: string;
  quantity: number;
  unitOfMeasureId: string | null;
  unitLabel: string | null;
}

export interface RFQVendorDto {
  id: string;
  vendorId: string;
  invitedAt: string;
  status: RFQVendorStatus;
  declinedReason: string | null;
}

export interface RFQListDto {
  id: string;
  rfqNumber: string;
  title: string;
  purchaseRequisitionId: string | null;
  bidDeadline: string;
  status: RFQStatus;
  itemCount: number;
  vendorCount: number;
  createdAt: string;
}

export interface RFQDto extends RFQListDto {
  deliveryDate: string | null;
  notes: string | null;
  terms: string | null;
  updatedAt: string;
  items: RFQItemDto[];
  vendors: RFQVendorDto[];
}

export interface CreatePRItemRequest {
  materialId?: string;
  itemDescription: string;
  quantity: number;
  unitOfMeasureId?: string;
  unitLabel?: string;
  estimatedUnitPrice: number;
  notes?: string;
}

export interface CreatePRRequest {
  companyId: string;
  title: string;
  description?: string;
  department: string;
  deliveryLocation?: string;
  requiredDate: string;
  notes?: string;
  items: CreatePRItemRequest[];
}

export interface CreateRFQItemRequest {
  prItemId?: string;
  materialId?: string;
  itemDescription: string;
  quantity: number;
  unitOfMeasureId?: string;
  unitLabel?: string;
}

export interface CreateRFQRequest {
  companyId: string;
  title: string;
  purchaseRequisitionId?: string;
  bidDeadline: string;
  deliveryDate?: string;
  notes?: string;
  terms?: string;
  items: CreateRFQItemRequest[];
}

// ── PR endpoints ──────────────────────────────────────────────────────────────

export const procurementApi = {
  listPRs: (companyId: string) =>
    apiClient.get<PRListDto[]>('/purchase-requisitions', { params: { companyId } }),

  getPR: (id: string) =>
    apiClient.get<PRDto>(`/purchase-requisitions/${id}`),

  createPR: (data: CreatePRRequest) =>
    apiClient.post<string>('/purchase-requisitions', data),

  submitPR: (id: string) =>
    apiClient.post(`/purchase-requisitions/${id}/submit`),

  cancelPR: (id: string) =>
    apiClient.post(`/purchase-requisitions/${id}/cancel`),

  // ── RFQ endpoints ─────────────────────────────────────────────────────────

  listRFQs: (companyId: string) =>
    apiClient.get<RFQListDto[]>('/rfqs', { params: { companyId } }),

  getRFQ: (id: string) =>
    apiClient.get<RFQDto>(`/rfqs/${id}`),

  createRFQ: (data: CreateRFQRequest) =>
    apiClient.post<string>('/rfqs', data),

  inviteVendors: (rfqId: string, vendorIds: string[]) =>
    apiClient.post(`/rfqs/${rfqId}/invite-vendors`, vendorIds),

  openRFQ: (id: string) =>
    apiClient.post(`/rfqs/${id}/open`),

  closeRFQ: (id: string) =>
    apiClient.post(`/rfqs/${id}/close`),
};
