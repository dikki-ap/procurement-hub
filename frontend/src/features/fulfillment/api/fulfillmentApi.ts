import { apiClient } from '@/shared/lib/axios';

export type POStatus      = 'Draft' | 'PendingApproval' | 'Approved' | 'Issued' | 'Acknowledged' | 'InDelivery' | 'Completed' | 'Cancelled';
export type ReturnStatus  = 'Created' | 'Acknowledged' | 'Shipped' | 'Received';
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
  acknowledgedAt:           string | null;
  acknowledgementDeadline:  string | null;
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
  withholdingTax: number;
  netPayable: number;
  currencyCode: string | null;
  fileUrl: string | null;
  paidAt: string | null;
  paymentReference: string | null;
  notes: string | null;
  rejectionReason: string | null;
  reviewedAt: string | null;
  // 3-way matching
  poMatched: boolean;
  grnMatched: boolean;
  amountMatched: boolean;
  matchingDiscrepancies: string[];
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

export interface ReturnOrderItemDto {
  id: string;
  poItemId: string | null;
  itemDescription: string;
  quantity: number;
  uom: string;
  returnReason: string | null;
}

export interface ReturnOrderListDto {
  id: string;
  returnNumber: string;
  grnId: string;
  poId: string;
  vendorId: string;
  vendorName: string | null;
  status: ReturnStatus;
  reason: string | null;
  createdAt: string;
  acknowledgedAt: string | null;
  shippedAt: string | null;
  receivedAt: string | null;
}

export interface CreateReturnOrderPayload {
  grnId: string;
  reason?: string;
  notes?: string;
  items: {
    poItemId?: string | null;
    itemDescription: string;
    quantity: number;
    uom: string;
    returnReason?: string;
  }[];
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
  getInvoiceById: (id: string)      => apiClient.get<{ data: InvoiceDto }>(`/invoices/${id}`).then(r => r.data.data),
  submitInvoice:  (data: { poId: string; vendorId: string; amount: number; taxAmount: number; currencyId?: string; fileUrl?: string; dueAt?: string; notes?: string }) =>
    apiClient.post<string>('/invoices', data),
  reviewInvoice:  (id: string, approve: boolean, rejectionReason?: string) =>
    apiClient.post(`/invoices/${id}/review`, { approve, rejectionReason }),
  confirmPayment: (id: string, paymentReference: string) =>
    apiClient.post(`/invoices/${id}/confirm-payment`, { paymentReference }),

  // Vendor invoices
  getVendorInvoices: (vendorId: string) => apiClient.get<{ data: InvoiceListDto[] }>(`/invoices?vendorId=${vendorId}`).then(r => r.data.data),

  // Return Orders
  getReturnOrders: (companyId: string) =>
    apiClient.get<{ data: ReturnOrderListDto[] }>(`/return-orders?companyId=${companyId}`).then(r => r.data.data),
  createReturnOrder: (payload: CreateReturnOrderPayload) =>
    apiClient.post<{ data: { id: string } }>('/return-orders', payload).then(r => r.data.data),
  confirmReturnReceived: (id: string) =>
    apiClient.post(`/return-orders/${id}/received`),

  // Vendor return orders (portal)
  getVendorReturnOrders: (vendorId: string) =>
    apiClient.get<{ data: ReturnOrderListDto[] }>(`/vendor-portal/${vendorId}/return-orders`).then(r => r.data.data),
  acknowledgeReturn: (vendorId: string, returnOrderId: string) =>
    apiClient.post(`/vendor-portal/${vendorId}/return-orders/${returnOrderId}/acknowledge`),
};
