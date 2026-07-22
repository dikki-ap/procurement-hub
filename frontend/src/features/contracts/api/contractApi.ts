import { apiClient } from '@/shared/lib/axios';

export type ContractStatus = 'Draft' | 'Active' | 'Expired' | 'Terminated';

export interface ContractListDto {
  id: string;
  contractNumber: string;
  title: string;
  vendorId: string;
  vendorName: string;
  poId: string | null;
  poNumber: string | null;
  status: ContractStatus;
  startDate: string | null;
  endDate: string | null;
  value: number | null;
  currencyId: string | null;
  createdAt: string;
}

export interface ContractDto {
  id: string;
  companyId: string;
  contractNumber: string;
  title: string;
  vendorId: string;
  vendorName: string;
  poId: string | null;
  poNumber: string | null;
  status: ContractStatus;
  hasFile: boolean;
  signedAt: string | null;
  startDate: string | null;
  endDate: string | null;
  value: number | null;
  currencyId: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
  createdByName: string | null;
  updatedByName: string | null;
}

export interface CreateContractRequest {
  vendorId: string;
  title: string;
  poId: string | null;
  startDate: string | null;
  endDate: string | null;
  value: number | null;
  currencyId: string | null;
  notes: string | null;
}

export interface UpdateContractRequest {
  title: string;
  poId: string | null;
  startDate: string | null;
  endDate: string | null;
  value: number | null;
  currencyId: string | null;
  notes: string | null;
}

export const contractApi = {
  getList: async (): Promise<ContractListDto[]> => {
    const res = await apiClient.get('/contracts');
    return res.data.data;
  },

  getById: async (id: string): Promise<ContractDto> => {
    const res = await apiClient.get(`/contracts/${id}`);
    return res.data.data;
  },

  create: async (body: CreateContractRequest): Promise<string> => {
    const res = await apiClient.post('/contracts', body);
    return res.data.data;
  },

  update: async (id: string, body: UpdateContractRequest): Promise<void> => {
    await apiClient.put(`/contracts/${id}`, body);
  },

  activate: async (id: string): Promise<void> => {
    await apiClient.post(`/contracts/${id}/activate`);
  },

  terminate: async (id: string, reason?: string): Promise<void> => {
    await apiClient.post(`/contracts/${id}/terminate`, { reason: reason ?? null });
  },

  upload: async (id: string, file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const res = await apiClient.post(`/contracts/${id}/upload`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data.data.key;
  },

  getDownloadUrl: async (id: string): Promise<string> => {
    const res = await apiClient.get(`/contracts/${id}/download`);
    return res.data.data.url;
  },

  // Vendor portal
  getByVendor: async (vendorId: string): Promise<ContractListDto[]> => {
    const res = await apiClient.get(`/vendor-portal/${vendorId}/contracts`);
    return res.data.data;
  },
};
