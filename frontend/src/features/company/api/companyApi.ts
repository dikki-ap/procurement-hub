import { apiClient } from '@/shared/lib/axios';

export interface CompanyDto {
  id: string;
  name: string;
  code: string;
  type: string;
  address: string | null;
  phone: string | null;
  email: string | null;
  logoUrl: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateCompanyRequest {
  name: string;
  type: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
}

export const companyApi = {
  get: () =>
    apiClient.get<{ data: CompanyDto }>('/company').then((r) => r.data.data),

  update: (data: UpdateCompanyRequest) =>
    apiClient.put('/company', data),

  uploadLogo: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return apiClient
      .post<{ data: { logoKey: string } }>('/company/logo', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data.data);
  },
};
