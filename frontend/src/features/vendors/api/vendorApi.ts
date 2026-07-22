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
  createdByName: string | null;
  updatedByName: string | null;
  updatedAt: string;
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

export interface VendorBankAccountDto {
  id: string;
  bankName: string;
  accountNumber: string;
  accountName: string;
  branchName: string | null;
  currency: string;
  isDefault: boolean;
  notes: string | null;
}

export interface VendorScoreDto {
  id: string;
  periodYear: number;
  periodQuarter: number;
  deliveryScore: number | null;
  qualityScore: number | null;
  priceScore: number | null;
  responseScore: number | null;
  docScore: number | null;
  totalScore: number | null;
  tier: string | null;
  notes: string | null;
  calculatedAt: string;
}

export interface VendorCapabilityDto {
  id: string;
  materialCategoryId: string;
  materialCategoryName: string | null;
  minOrderQty: number | null;
  maxOrderQty: number | null;
  uom: string | null;
  leadTimeDays: number | null;
  effectiveDate: string | null;
  expiryDate: string | null;
  isExpired: boolean;
  notes: string | null;
}

export interface VendorDetailDto extends VendorDto {
  address: string | null;
  city: string | null;
  province: string | null;
  postalCode: string | null;
  country: string | null;
  defaultPaymentTermId: string | null;
  defaultCurrencyId: string | null;
  isPkp: boolean;
  pphRate: number | null;
  contacts: VendorContactDto[];
  documents: VendorDocumentDto[];
  capabilities: VendorCapabilityDto[];
  bankAccounts: VendorBankAccountDto[];
}

export interface RegisterVendorRequest {
  companyId: string;
  legalName: string;
  tradeName?: string;
  vendorType: VendorType;
  npwp?: string;
  siup?: string;
  nib?: string;
  address?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  country?: string;
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

  getDocumentUrl: (vendorId: string, documentId: string, inline = false) =>
    apiClient
      .get<{ data: { url: string; fileName: string } }>(`${BASE}/${vendorId}/documents/${documentId}/download`, {
        params: inline ? { inline: 'true' } : undefined,
      })
      .then((r) => r.data.data),

  addCapability: (
    vendorId: string,
    payload: {
      materialCategoryId: string;
      minOrderQty?: number | null;
      maxOrderQty?: number | null;
      uom?: string | null;
      leadTimeDays?: number | null;
      effectiveDate?: string | null;
      expiryDate?: string | null;
      notes?: string | null;
    }
  ) => apiClient.post(`${BASE}/${vendorId}/capabilities`, payload),

  updateCapability: (
    vendorId: string,
    capabilityId: string,
    payload: {
      minOrderQty?: number | null;
      maxOrderQty?: number | null;
      uom?: string | null;
      leadTimeDays?: number | null;
      effectiveDate?: string | null;
      expiryDate?: string | null;
      notes?: string | null;
    }
  ) => apiClient.put(`${BASE}/${vendorId}/capabilities/${capabilityId}`, payload),

  deleteCapability: (vendorId: string, capabilityId: string) =>
    apiClient.delete(`${BASE}/${vendorId}/capabilities/${capabilityId}`),

  addBankAccount: (
    vendorId: string,
    payload: { bankName: string; accountNumber: string; accountName: string; branchName?: string | null; currency: string; isDefault: boolean; notes?: string | null }
  ) => apiClient.post(`${BASE}/${vendorId}/bank-accounts`, payload),

  updateBankAccount: (
    vendorId: string,
    bankAccountId: string,
    payload: { bankName: string; accountNumber: string; accountName: string; branchName?: string | null; currency: string; isDefault: boolean; notes?: string | null }
  ) => apiClient.put(`${BASE}/${vendorId}/bank-accounts/${bankAccountId}`, payload),

  deleteBankAccount: (vendorId: string, bankAccountId: string) =>
    apiClient.delete(`${BASE}/${vendorId}/bank-accounts/${bankAccountId}`),

  getScoreHistory: (vendorId: string) =>
    apiClient.get<{ data: VendorScoreDto[] }>(`${BASE}/${vendorId}/scores`).then((r) => r.data.data),

  updateTaxInfo: (vendorId: string, isPkp: boolean, pphRate: number | null, vendor: VendorDetailDto) =>
    apiClient.put(`${BASE}/${vendorId}`, {
      legalName:           vendor.legalName,
      tradeName:           vendor.tradeName,
      vendorType:          vendor.vendorType,
      npwp:                vendor.npwp,
      siup:                vendor.siup,
      nib:                 vendor.nib,
      address:             vendor.address,
      city:                vendor.city,
      province:            vendor.province,
      postalCode:          vendor.postalCode,
      country:             vendor.country,
      defaultPaymentTermId: vendor.defaultPaymentTermId,
      defaultCurrencyId:   vendor.defaultCurrencyId,
      isPkp,
      pphRate,
    }),
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

  getDocumentUrl: (vendorId: string, documentId: string, inline = false) =>
    apiClient
      .get<{ data: { url: string; fileName: string } }>(`/vendor-portal/${vendorId}/documents/${documentId}/download`, {
        params: inline ? { inline: 'true' } : undefined,
      })
      .then((r) => r.data.data),

  getDocumentTypes: (vendorId: string) =>
    apiClient.get<{ data: DocumentTypeConfigDto[] }>(`/vendor-portal/${vendorId}/document-types`).then((r) => r.data.data),

  addBankAccount: (
    vendorId: string,
    payload: { bankName: string; accountNumber: string; accountName: string; branchName?: string | null; currency: string; isDefault: boolean; notes?: string | null }
  ) => apiClient.post(`/vendor-portal/${vendorId}/bank-accounts`, payload),

  updateBankAccount: (
    vendorId: string,
    bankAccountId: string,
    payload: { bankName: string; accountNumber: string; accountName: string; branchName?: string | null; currency: string; isDefault: boolean; notes?: string | null }
  ) => apiClient.put(`/vendor-portal/${vendorId}/bank-accounts/${bankAccountId}`, payload),

  deleteBankAccount: (vendorId: string, bankAccountId: string) =>
    apiClient.delete(`/vendor-portal/${vendorId}/bank-accounts/${bankAccountId}`),

  getScoreHistory: (vendorId: string) =>
    apiClient.get<{ data: VendorScoreDto[] }>(`/vendor-portal/${vendorId}/scores`).then((r) => r.data.data),

  updateProfile: (
    vendorId: string,
    payload: {
      tradeName?: string | null;
      npwp?: string | null;
      siup?: string | null;
      nib?: string | null;
      address?: string | null;
      city?: string | null;
      province?: string | null;
      postalCode?: string | null;
      country?: string | null;
    }
  ) => apiClient.put(`/vendor-portal/${vendorId}/profile`, payload),
};
