import { apiClient } from '@/shared/lib/axios';

export type VendorStatus = 'Pending' | 'Active' | 'Suspended' | 'Blacklisted';
export type VendorType   = 'Manufacturer' | 'Distributor' | 'Trader';
export type VendorTier   = 'Probation' | 'Bronze' | 'Silver' | 'Gold';
export type DocumentType = string;

export interface DocumentTypeConfigDto {
  id: string;
  name: string;
  isActive: boolean;
  allowedExtensions: string | null;
  maxFileSizeMb: number;
}
export type DocumentStatus = 'Active' | 'Expired' | 'Revoked';

export interface VendorDto {
  id: string;
  vendorCode: string;
  legalName: string;
  tradeName: string | null;
  npwp: string | null;
  siup: string | null;
  nib: string | null;
  vendorType: VendorType;
  status: VendorStatus;
  tier: VendorTier;
  score: number;
  isBlacklisted: boolean;
  blacklistReason: string | null;
  approvedAt: string | null;
  createdAt: string;
}

export interface VendorContactDto {
  id: string;
  name: string;
  position: string | null;
  email: string | null;
  phone: string | null;
  isPrimary: boolean;
}

export interface VendorDocumentDto {
  id: string;
  documentType: DocumentType;
  documentNumber: string | null;
  fileUrl: string;
  fileName: string | null;
  fileSize: number | null;
  expiredAt: string | null;
  issuedAt: string | null;
  status: DocumentStatus;
  notes: string | null;
}

export interface VendorDetailDto extends VendorDto {
  contacts: VendorContactDto[];
  documents: VendorDocumentDto[];
  capabilities: {
    id: string;
    materialCategoryId: string;
    minOrderQty: number | null;
    leadTimeDays: number | null;
    notes: string | null;
  }[];
}

export interface RegisterVendorRequest {
  companyId: string;
  legalName: string;
  tradeName?: string;
  vendorType: VendorType;
  npwp?: string;
  siup?: string;
  nib?: string;
  contactName: string;
  contactPosition?: string;
  contactEmail: string;
  contactPhone?: string;
}

const BASE = '/vendors';

export const vendorApi = {
  getAll: (companyId: string) =>
    apiClient.get<{ data: VendorDto[] }>(BASE, { params: { companyId } }).then((r) => r.data.data),

  getById: (id: string) =>
    apiClient.get<{ data: VendorDetailDto }>(`${BASE}/${id}`).then((r) => r.data.data),

  getDocuments: (id: string) =>
    apiClient.get<{ data: VendorDocumentDto[] }>(`${BASE}/${id}/documents`).then((r) => r.data.data),

  approve: (id: string) =>
    apiClient.post(`${BASE}/${id}/approve`),

  suspend: (id: string) =>
    apiClient.post(`${BASE}/${id}/suspend`),

  blacklist: (id: string, reason: string) =>
    apiClient.post(`${BASE}/${id}/blacklist`, { reason }),

  reinstate: (id: string) =>
    apiClient.post(`${BASE}/${id}/reinstate`),

  deleteDocument: (vendorId: string, documentId: string) =>
    apiClient.delete(`${BASE}/${vendorId}/documents/${documentId}`),
};

export const vendorRegistrationApi = {
  register: (data: RegisterVendorRequest) =>
    apiClient.post<{ data: { id: string; message: string } }>('/vendor-registration', data).then((r) => r.data.data),
};

export const vendorPortalApi = {
  getMyVendorId: () =>
    apiClient.get<{ data: { vendorId: string } }>('/vendor-portal/me').then((r) => r.data.data.vendorId),

  getProfile: (vendorId: string) =>
    apiClient.get<{ data: VendorDetailDto }>(`/vendor-portal/${vendorId}`).then((r) => r.data.data),

  getDocuments: (vendorId: string) =>
    apiClient.get<{ data: VendorDocumentDto[] }>(`/vendor-portal/${vendorId}/documents`).then((r) => r.data.data),

  uploadDocument: (vendorId: string, formData: FormData, onProgress?: (pct: number) => void) =>
    apiClient.post<{ data: { id: string } }>(`/vendor-portal/${vendorId}/documents`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: onProgress
        ? (e) => { if (e.total) onProgress(Math.round((e.loaded / e.total) * 100)); }
        : undefined,
    }).then((r) => r.data.data),

  deleteDocument: (vendorId: string, documentId: string) =>
    apiClient.delete(`/vendor-portal/${vendorId}/documents/${documentId}`),

  getDocumentTypes: (vendorId: string) =>
    apiClient.get<{ data: DocumentTypeConfigDto[] }>(`/vendor-portal/${vendorId}/document-types`).then((r) => r.data.data),
};
