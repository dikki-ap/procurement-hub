import { apiClient } from '@/shared/lib/axios';

export type WorkflowStatus = 'Pending' | 'Approved' | 'Revised' | 'Rejected' | 'Cancelled';
export type ApprovalActionType = 'Approve' | 'Revise' | 'Reject' | 'Delegate';

export interface ApprovalHistoryDto {
  id: string;
  level: number;
  action: ApprovalActionType;
  actorName: string;
  reason: string | null;
  actedAt: string;
}

export interface ApproverAssignmentDto {
  assignedUserId: string;
  assignedUserName: string;
  level: number;
  isDelegate: boolean;
}

export interface ApprovalWorkflowDto {
  id: string;
  referenceType: string;
  referenceId: string;
  referenceNumber: string;
  referenceTitle: string;
  totalValue: number;
  currentLevel: number;
  maxLevel: number;
  status: WorkflowStatus;
  iteration: number;
  completedAt: string | null;
  createdAt: string;
  history: ApprovalHistoryDto[];
  assignments: ApproverAssignmentDto[];
}

export interface ApprovalInboxItemDto {
  workflowId: string;
  referenceType: string;
  referenceNumber: string;
  referenceTitle: string;
  totalValue: number;
  currentLevel: number;
  maxLevel: number;
  status: WorkflowStatus;
  createdAt: string;
}

export interface ApprovalPolicyDto {
  id: string;
  companyId: string;
  referenceType: string;
  name: string;
  minValue: number;
  maxValue: number | null;
  requiredLevels: number;
  isStrategicOverride: boolean;
  isSingleSourceOverride: boolean;
  isActive: boolean;
}

export const approvalApi = {
  getInbox: (userId: string, companyId: string) =>
    apiClient.get<ApprovalInboxItemDto[]>('/approval-workflows/inbox', {
      params: { userId, companyId },
    }),

  getWorkflow: (id: string) =>
    apiClient.get<ApprovalWorkflowDto>(`/approval-workflows/${id}`),

  approve: (id: string, approverId: string, approverName: string) =>
    apiClient.post(`/approval-workflows/${id}/approve`, { approverId, approverName }),

  revise: (id: string, approverId: string, approverName: string, reason: string) =>
    apiClient.post(`/approval-workflows/${id}/revise`, { approverId, approverName, reason }),

  reject: (id: string, approverId: string, approverName: string, reason: string) =>
    apiClient.post(`/approval-workflows/${id}/reject`, { approverId, approverName, reason }),

  delegate: (id: string, approverId: string, approverName: string, delegateToUserId: string, delegateToUserName: string) =>
    apiClient.post(`/approval-workflows/${id}/delegate`, {
      approverId, approverName, delegateToUserId, delegateToUserName,
    }),

  getPolicies: (companyId: string) =>
    apiClient.get<ApprovalPolicyDto[]>('/approval-policies', { params: { companyId } }),

  createPolicy: (data: {
    companyId: string; referenceType: string; name: string;
    minValue: number; maxValue?: number; requiredLevels: number;
    isStrategicOverride: boolean; isSingleSourceOverride: boolean;
  }) => apiClient.post<string>('/approval-policies', data),
};
