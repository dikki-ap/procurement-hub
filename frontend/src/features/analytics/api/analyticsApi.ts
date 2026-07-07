import { apiClient } from '@/shared/lib/axios';

export interface DashboardWidgets {
  spendThisMonth:   number;
  pendingApprovals: number;
  openRFQs:         number;
  activePOs:        number;
  pendingInvoices:  number;
  totalVendors:     number;
  totalPRs:         number;
}

export interface VendorDashboardWidgets {
  myActivePOs:       number;
  myPendingInvoices: number;
  myLatestScore:     number;
  myTier:            string;
}

export interface MonthlySpend {
  month:  string;
  amount: number;
}

export interface SpendSummary {
  monthly:       MonthlySpend[];
  totalThisYear: number;
  totalLastYear: number;
}

export interface VendorPerformanceItem {
  vendorId:     string;
  vendorName:   string;
  tier:         string;
  totalScore:   number;
  deliveryScore: number;
  qualityScore:  number;
  totalSpend:   number;
  pOCount:      number;
}

export interface VendorPerformanceSummary {
  vendors: VendorPerformanceItem[];
}

export interface FunnelStage {
  stage: string;
  count: number;
  value: number;
}

export interface FunnelStats {
  stages: FunnelStage[];
}

export const analyticsApi = {
  getWidgets: (params: { companyId?: string; vendorId?: string }) =>
    apiClient.get<{ data: DashboardWidgets | VendorDashboardWidgets }>('/analytics/widgets', { params }),

  getSpendSummary: (companyId: string, months = 12) =>
    apiClient.get<{ data: SpendSummary }>('/analytics/spend-summary', {
      params: { companyId, months },
    }),

  getVendorPerformance: (companyId: string, topN = 10) =>
    apiClient.get<{ data: VendorPerformanceSummary }>('/analytics/vendor-performance', {
      params: { companyId, topN },
    }),

  getFunnelStats: (companyId: string, year?: number) =>
    apiClient.get<{ data: FunnelStats }>('/analytics/funnel', {
      params: { companyId, year },
    }),
};
