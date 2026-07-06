import { apiClient } from '@/shared/lib/axios';

export interface AuditLogDto {
  id:            string;
  userId?:       string;
  userEmail?:    string;
  userFullName?: string;
  entityType:    string;
  entityId:      string;
  action:        string;
  beforeValues?: string;
  afterValues:   string;
  changedColumns?: string;
  ipAddress?:    string;
  correlationId?: string;
  createdAt:     string;
}

export interface PagedAuditLogDto {
  items:      AuditLogDto[];
  total:      number;
  page:       number;
  pageSize:   number;
  totalPages: number;
}

export interface AuditLogFilter {
  entityType?: string;
  entityId?:   string;
  userId?:     string;
  action?:     string;
  from?:       string;
  to?:         string;
  page?:       number;
  pageSize?:   number;
}

export const auditApi = {
  getAuditLog: (params: AuditLogFilter) =>
    apiClient.get<{ data: PagedAuditLogDto }>('/api/v1/audit', { params }),

  exportAuditLog: (filter: Omit<AuditLogFilter, 'page' | 'pageSize'>) =>
    apiClient.post<{ data: string }>('/api/v1/audit/export', filter),
};
