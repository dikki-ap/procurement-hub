import { apiClient } from '@/shared/lib/axios';

export interface MaterialCategoryDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  parentId: string | null;
  isStrategic: boolean;
  isActive: boolean;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface CreateMaterialCategoryRequest {
  companyId: string;
  name: string;
  code: string;
  parentId?: string;
  isStrategic: boolean;
}

export interface UpdateMaterialCategoryRequest {
  name: string;
  code: string;
  parentId?: string;
  isStrategic: boolean;
  isActive: boolean;
}

const BASE = '/master-data/material-categories';

export const materialCategoryApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: MaterialCategoryDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: MaterialCategoryDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateMaterialCategoryRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateMaterialCategoryRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
