import { apiClient } from '@/shared/lib/axios';

export type PRStatus         = 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Cancelled';
export type RFQStatus        = 'Draft' | 'Open' | 'Closed' | 'Cancelled' | 'PendingApproval';
export type RFQVendorStatus  = 'Invited' | 'Declined' | 'Submitted';
export type QuotationStatus  = 'Draft' | 'Submitted' | 'Withdrawn' | 'Awarded' | 'Rejected';
export type EvaluationStatus = 'Pending' | 'InProgress' | 'Awarded';

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
  createdByName: string | null;
  updatedByName: string | null;
  updatedAt: string;
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
  createdByName: string | null;
  updatedByName: string | null;
  updatedAt: string;
}

export interface RFQDto extends RFQListDto {
  deliveryDate: string | null;
  notes: string | null;
  terms: string | null;
  updatedAt: string;
  fileKey:  string | null;
  fileName: string | null;
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

// ── Quotation types ───────────────────────────────────────────────────────────

export interface QuotationItemDto {
  id: string;
  rfqItemId: string;
  itemDescription: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  notes: string | null;
}

export interface QuotationListDto {
  id: string;
  rfqId: string;
  rfqNumber: string;
  rfqTitle: string;
  vendorId: string;
  vendorName: string;
  status: QuotationStatus;
  totalPrice: number;
  createdAt: string;
}

export interface BidComparisonItemDto {
  rfqItemId: string;
  itemDescription: string;
  quantity: number;
  unitLabel: string | null;
}

export interface BidComparisonPriceDto {
  rfqItemId: string;
  unitPrice: number;
  lineTotal: number;
  notes: string | null;
}

export interface BidComparisonRowDto {
  vendorId: string;
  vendorName: string;
  quotationId: string;
  status: string;
  totalPrice: number;
  itemPrices: BidComparisonPriceDto[];
}

export interface BidComparisonDto {
  rfqId: string;
  rfqNumber: string;
  rfqTitle: string;
  items: BidComparisonItemDto[];
  vendors: BidComparisonRowDto[];
}

export interface EvaluationScoreDto {
  quotationId: string;
  vendorId: string;
  vendorName: string;
  priceScore: number;
  qualityScore: number;
  deliveryScore: number;
  weightedTotal: number;
}

export interface EvaluatorAssignmentDto {
  id: string;
  assignedUserId: string;
  assignedUserName: string;
  hasSubmitted: boolean;
}

export interface BidEvaluationDto {
  id: string;
  rfqId: string;
  priceWeight: number;
  qualityWeight: number;
  deliveryWeight: number;
  status: EvaluationStatus;
  awardedVendorId: string | null;
  awardedQuotationId: string | null;
  scores: EvaluationScoreDto[];
  evaluators: EvaluatorAssignmentDto[];
}

export interface SubmitQuotationItemRequest {
  rfqItemId: string;
  unitPrice: number;
  notes?: string;
}

export interface SubmitQuotationRequest {
  rfqId: string;
  vendorId: string;
  notes?: string;
  items: SubmitQuotationItemRequest[];
}

export interface VendorScoreInput {
  quotationId: string;
  vendorId: string;
  qualityScore: number;
  deliveryScore: number;
}

export interface EvaluateBidsRequest {
  rfqId: string;
  priceWeight: number;
  qualityWeight: number;
  deliveryWeight: number;
  scores: VendorScoreInput[];
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
    apiClient.get<{ data: PRListDto[] }>('/purchase-requisitions', { params: { companyId } }).then(r => r.data.data),

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
    apiClient.get<{ data: RFQListDto[] }>('/rfqs', { params: { companyId } }).then(r => r.data.data),

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

  getRFQBids: (rfqId: string) =>
    apiClient.get<QuotationListDto[]>(`/rfqs/${rfqId}/bids`),

  getBidComparison: (rfqId: string) =>
    apiClient.get<BidComparisonDto>(`/rfqs/${rfqId}/bids/comparison`),

  evaluateBids: (rfqId: string, data: EvaluateBidsRequest) =>
    apiClient.post<string>(`/rfqs/${rfqId}/evaluate`, data),

  getBidEvaluationResult: (rfqId: string) =>
    apiClient.get<BidEvaluationDto | null>(`/rfqs/${rfqId}/evaluation`),

  awardVendor: (rfqId: string, quotationId: string, vendorId: string) =>
    apiClient.post(`/rfqs/${rfqId}/award`, { rfqId, quotationId, vendorId }),

  uploadRFQAttachment: (rfqId: string, file: File) => {
    const form = new FormData();
    form.append('file', file);
    return apiClient.post<{ data: { key: string } }>(`/rfqs/${rfqId}/upload`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  getRFQAttachmentUrl: (rfqId: string) =>
    apiClient.get<{ data: { url: string } }>(`/rfqs/${rfqId}/download`).then(r => r.data.data.url),

  // ── Quotation (vendor portal) ─────────────────────────────────────────────

  submitQuotation: (data: SubmitQuotationRequest) =>
    apiClient.post<string>('/quotations', data),

  withdrawQuotation: (quotationId: string) =>
    apiClient.post(`/quotations/${quotationId}/withdraw`),

  getMyQuotations: (vendorId: string) =>
    apiClient.get<QuotationListDto[]>('/quotations', { params: { vendorId } }),

  uploadQuotationAttachment: (quotationId: string, file: File) => {
    const form = new FormData();
    form.append('file', file);
    return apiClient.post<{ data: { key: string } }>(`/quotations/${quotationId}/upload`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  getQuotationAttachmentUrl: (quotationId: string) =>
    apiClient.get<{ data: { url: string } }>(`/quotations/${quotationId}/download`).then(r => r.data.data.url),

  // ── Multi-evaluator ───────────────────────────────────────────────────────

  assignEvaluator: (rfqId: string, userId: string, userName: string) =>
    apiClient.post<{ data: { assignmentId: string } }>(`/rfqs/${rfqId}/evaluators`, { userId, userName }),

  submitEvaluatorScores: (rfqId: string, scores: { quotationId: string; qualityScore: number; deliveryScore: number }[]) =>
    apiClient.post(`/rfqs/${rfqId}/evaluators/scores`, { scores }),

  finalizeEvaluation: (rfqId: string) =>
    apiClient.post(`/rfqs/${rfqId}/finalize`),
};
