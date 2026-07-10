import { apiClient } from '@/shared/lib/axios';

export interface DocumentTypeDto {
  id: string;
  name: string;
  isActive: boolean;
  allowedExtensions: string | null;
  maxFileSizeMb: number;
}

export interface CreateDocumentTypeRequest {
  name: string;
  allowedExtensions: string | null;
  maxFileSizeMb: number;
}

export interface UpdateDocumentTypeRequest {
  name: string;
  isActive: boolean;
  allowedExtensions: string | null;
  maxFileSizeMb: number;
}

const BASE = '/master-data/document-types';

export const documentTypeApi = {
  getAll: () =>
    apiClient.get<{ data: DocumentTypeDto[] }>(BASE).then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: DocumentTypeDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateDocumentTypeRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateDocumentTypeRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
};
