import { apiClient } from '@/shared/lib/axios';

export interface MaterialDto {
  id: string;
  categoryId: string;
  categoryName: string;
  code: string;
  name: string;
  description: string | null;
  uomId: string;
  uomCode: string;
  estimatedPrice: number | null;
  currencyId: string | null;
  currencyCode: string | null;
  isStrategic: boolean;
  isActive: boolean;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface CreateMaterialRequest {
  categoryId: string;
  code: string;
  name: string;
  description?: string;
  uomId: string;
  estimatedPrice?: number;
  currencyId?: string;
  isStrategic: boolean;
}

export interface UpdateMaterialRequest extends CreateMaterialRequest {
  isActive: boolean;
}

const BASE = '/master-data/materials';

export const materialApi = {
  getAll: () => apiClient.get<{ data: MaterialDto[] }>(BASE).then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: MaterialDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateMaterialRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateMaterialRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
