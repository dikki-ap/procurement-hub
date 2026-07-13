import { apiClient } from '@/shared/lib/axios';

export type POStatus      = 'Draft' | 'PendingApproval' | 'Approved' | 'Issued' | 'Acknowledged' | 'InDelivery' | 'Completed' | 'Cancelled';
export type GRNStatus     = 'Draft' | 'Confirmed' | 'Discrepancy';
export type QualityStatus = 'Accepted' | 'Partial' | 'Rejected';
export type InvoiceStatus = 'Submitted' | 'UnderReview' | 'Approved' | 'Paid' | 'Rejected';

export interface POItemDto {
  id: string;
  materialId: string | null;
  description: string;
  quantity: number;
  uomCode: string | null;
  unitPrice: number;
  totalPrice: number;
  receivedQty: number;
}

export interface POListDto {
  id: string;
  poNumber: string;
  vendorId: string;
  vendorName: string;
  status: POStatus;
  totalAmount: number;
  expectedDelivery: string | null;
  issuedAt: string | null;
  createdAt: string;
  createdByName: string | null;
  updatedByName: string | null;
  updatedAt: string;
}

export interface PODto extends POListDto {
  rfqId: string | null;
  currencyId: string | null;
  currencyCode: string | null;
  paymentTermId: string | null;
  paymentTermName: string | null;
  deliveryLocationId: string | null;
  deliveryLocation: string | null;
  actualDelivery: string | null;
  fileUrl: string | null;
  notes: string | null;
  termsConditions: string | null;
  acknowledgedAt: string | null;
  completedAt: string | null;
  cancelledAt: string | null;
  cancelledReason: string | null;
  items: POItemDto[];
}

export interface GRNItemDto {
  id: string;
  poItemId: string;
  description: string;
  receivedQty: number;
  rejectedQty: number;
  qualityStatus: QualityStatus;
  rejectReason: string | null;
  notes: string | null;
}

export interface GRNListDto {
  id: string;
  grnNumber: string;
  poId: string;
  poNumber: string;
  status: GRNStatus;
  receivedAt: string | null;
  createdAt: string;
}

export interface GRNDto extends GRNListDto {
  receivedBy: string;
  deliveryNoteNo: string | null;
  notes: string | null;
  items: GRNItemDto[];
}

export interface InvoiceListDto {
  id: string;
  invoiceNumber: string;
  poId: string;
  poNumber: string;
  vendorId: string;
  vendorName: string;
  status: InvoiceStatus;
  totalAmount: number;
  dueAt: string | null;
  submittedAt: string;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface InvoiceDto extends InvoiceListDto {
  amount: number;
  taxAmount: number;
  currencyCode: string | null;
  fileUrl: string | null;
  paidAt: string | null;
  paymentReference: string | null;
  notes: string | null;
  rejectionReason: string | null;
  reviewedAt: string | null;
}

export interface CreatePOPayload {
  companyId: string;
  vendorId: string;
  rfqId?: string;
  currencyId?: string;
  paymentTermId?: string;
  deliveryLocationId?: string;
  expectedDelivery?: string;
  notes?: string;
  termsConditions?: string;
  items: { materialId?: string; description: string; quantity: number; uomId?: string; unitPrice: number }[];
}

export interface CreateGRNPayload {
  poId: string;
  receivedBy: string;
  deliveryNoteNo?: string;
  notes?: string;
  items: { poItemId: string; receivedQty: number; rejectedQty: number; qualityStatus: QualityStatus; rejectReason?: string; notes?: string }[];
}

export const fulfillmentApi = {
  // POs
  getPOList:   (companyId: string)  => apiClient.get<{ data: POListDto[] }>(`/purchase-orders?companyId=${companyId}`).then(r => r.data.data),
  getPOById:   (id: string)         => apiClient.get<PODto>(`/purchase-orders/${id}`),
  createPO:    (data: CreatePOPayload) => apiClient.post<string>('/purchase-orders', data),
  issuePO:     (id: string)         => apiClient.post(`/purchase-orders/${id}/issue`),
  acknowledgePO: (id: string)       => apiClient.post(`/purchase-orders/${id}/acknowledge`),

  // Vendor POs
  getVendorPOs: (vendorId: string)  => apiClient.get<{ data: POListDto[] }>(`/purchase-orders?vendorId=${vendorId}`).then(r => r.data.data),

  // GRNs
  getGRNList:  (poId: string)       => apiClient.get<GRNListDto[]>(`/goods-receipts?poId=${poId}`),
  getGRNById:  (id: string)         => apiClient.get<GRNDto>(`/goods-receipts/${id}`),
  createGRN:   (data: CreateGRNPayload) => apiClient.post<string>('/goods-receipts', data),
  confirmGRN:  (id: string, vendorId: string) => apiClient.post(`/goods-receipts/${id}/confirm?vendorId=${vendorId}`),

  // Invoices
  getInvoiceList: ()                => apiClient.get<{ data: InvoiceListDto[] }>('/invoices').then(r => r.data.data),
  getInvoiceById: (id: string)      => apiClient.get<InvoiceDto>(`/invoices/${id}`),
  submitInvoice:  (data: { poId: string; vendorId: string; amount: number; taxAmount: number; currencyId?: string; fileUrl?: string; dueAt?: string; notes?: string }) =>
    apiClient.post<string>('/invoices', data),
  reviewInvoice:  (id: string, approve: boolean, rejectionReason?: string) =>
    apiClient.post(`/invoices/${id}/review`, { approve, rejectionReason }),
  confirmPayment: (id: string, paymentReference: string) =>
    apiClient.post(`/invoices/${id}/confirm-payment`, { paymentReference }),

  // Vendor invoices
  getVendorInvoices: (vendorId: string) => apiClient.get<{ data: InvoiceListDto[] }>(`/invoices?vendorId=${vendorId}`).then(r => r.data.data),
};
