import { apiClient } from '@/shared/lib/axios';

export interface UserDto {
  id: string;
  companyId: string;
  keycloakId: string;
  email: string;
  fullName: string;
  role: string;
  departmentId: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserRequest {
  departmentId?: string | null;
  role: string;
  isActive: boolean;
}

export const INTERNAL_ROLES = [
  'super_admin',
  'requester',
  'purchasing',
  'approver',
  'finance',
  'management',
] as const;

const BASE = '/users';

export const userApi = {
  getAll: (companyId: string) =>
    apiClient
      .get<{ data: UserDto[] }>(BASE, { params: { companyId } })
      .then((r) => r.data.data),

  getById: (id: string) =>
    apiClient.get<{ data: UserDto }>(`${BASE}/${id}`).then((r) => r.data.data),

  update: (id: string, data: UpdateUserRequest) =>
    apiClient.put(`${BASE}/${id}`, data),

  deactivate: (id: string) =>
    apiClient.post(`${BASE}/${id}/deactivate`),
};
