import { apiClient } from '@/shared/lib/axios';

export interface PaymentTermDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  days: number;
  description: string | null;
  isActive: boolean;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface CreatePaymentTermRequest {
  companyId: string;
  code: string;
  name: string;
  days: number;
  description?: string;
}

export interface UpdatePaymentTermRequest {
  code: string;
  name: string;
  days: number;
  description?: string;
  isActive: boolean;
}

const BASE = '/master-data/payment-terms';

export const paymentTermApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: PaymentTermDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: PaymentTermDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreatePaymentTermRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdatePaymentTermRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
