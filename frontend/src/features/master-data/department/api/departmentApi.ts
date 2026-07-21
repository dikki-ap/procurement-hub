import { apiClient } from '@/shared/lib/axios';

export interface DepartmentDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface DepartmentRequest {
  name: string;
  code: string;
  isActive?: boolean;
}

const BASE = '/master-data/departments';

export const departmentApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: DepartmentDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),

  create: (data: DepartmentRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),

  update: (id: string, data: DepartmentRequest) =>
    apiClient.put(`${BASE}/${id}`, data),

  delete: (id: string) =>
    apiClient.delete(`${BASE}/${id}`),
};
