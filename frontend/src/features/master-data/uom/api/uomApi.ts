import { apiClient } from '@/shared/lib/axios';

export interface UomDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  isActive: boolean;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface CreateUomRequest {
  companyId: string;
  code: string;
  name: string;
}

export interface UpdateUomRequest {
  code: string;
  name: string;
  isActive: boolean;
}

const BASE = '/master-data/unit-of-measures';

export const uomApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: UomDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: UomDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateUomRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateUomRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
