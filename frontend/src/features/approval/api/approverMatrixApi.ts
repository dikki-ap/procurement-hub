import { apiClient } from '@/shared/lib/axios'

export interface ApproverMatrixEntryDto {
  id: string
  referenceType: 'PR' | 'PO' | 'RFQ'
  level: number
  name: string
  position: string
  email: string
  createdByName: string | null
  createdAt: string
  updatedByName: string | null
  updatedAt: string
}

export interface CreateApproverMatrixEntryRequest {
  referenceType: string
  level: number
  name: string
  position: string
  email: string
}

export interface UpdateApproverMatrixEntryRequest {
  referenceType: string
  level: number
  name: string
  position: string
  email: string
}

export const approverMatrixApi = {
  getAll: (): Promise<ApproverMatrixEntryDto[]> =>
    apiClient.get('/approver-matrix').then(r => r.data.data),

  create: (data: CreateApproverMatrixEntryRequest) =>
    apiClient.post('/approver-matrix', data),

  update: (id: string, data: UpdateApproverMatrixEntryRequest) =>
    apiClient.put(`/approver-matrix/${id}`, data),

  delete: (id: string) =>
    apiClient.delete(`/approver-matrix/${id}`),

  amIApprover: (): Promise<{ isApprover: boolean }> =>
    apiClient.get('/approver-matrix/am-i-approver').then(r => r.data.data),
}
