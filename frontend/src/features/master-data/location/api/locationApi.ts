import { apiClient } from '@/shared/lib/axios';

export interface LocationDto {
  id: string;
  companyId: string;
  name: string;
  type: string;
  address: string | null;
  city: string | null;
  province: string | null;
  country: string;
  isActive: boolean;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string;
}

export interface CreateLocationRequest {
  companyId: string;
  name: string;
  type: string;
  address?: string;
  city?: string;
  province?: string;
  country: string;
}

export interface UpdateLocationRequest extends Omit<CreateLocationRequest, 'companyId'> {
  isActive: boolean;
}

const BASE = '/master-data/locations';

export const locationApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: LocationDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: LocationDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateLocationRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateLocationRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
