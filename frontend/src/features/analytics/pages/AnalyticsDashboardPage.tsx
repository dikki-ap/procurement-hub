import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  AreaChart, Area, PieChart, Pie, Cell,
  BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Legend,
} from 'recharts';
import { LayoutDashboard, AlertTriangle, Download } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';
import { analyticsApi, type DashboardWidgets, type VendorDashboardWidgets } from '../api/analyticsApi';
import { useBaseCurrency } from '@/shared/hooks/useBaseCurrency';
import { downloadExcel } from '@/shared/lib/downloadFile';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

const TIER_COLOR: Record<string, string> = {
  Gold:      '#f59e0b',
  Silver:    '#94a3b8',
  Bronze:    '#b45309',
  Probation: '#ef4444',
};

const CHART_COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'];

function StatCard({ label, value, sub }: { label: string; value: string; sub?: string }) {
  return (
    <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
      {sub && <p className="text-xs text-muted-foreground mt-1">{sub}</p>}
    </div>
  );
}

export default function AnalyticsDashboardPage() {
  const { user } = useAuthStore();
  const isVendor = user?.roles?.includes('vendor') ?? false;
  const companyId = user?.companyId ?? '';
  const base = useBaseCurrency();
  const sym  = base?.symbol ?? base?.code ?? '?';

  const thisYear  = new Date().getFullYear();
  const [fromDate, setFromDate] = useState(`${thisYear}-01-01`);
  const [toDate,   setToDate]   = useState(new Date().toISOString().slice(0, 10));
  const [isExporting, setIsExporting] = useState(false);

  const handleExportSpend = async () => {
    setIsExporting(true);
    try {
      const from = fromDate.replace(/-/g, '');
      const to   = toDate.replace(/-/g, '');
      await downloadExcel(
        `/analytics/spend-report/export?companyId=${companyId}&from=${fromDate}&to=${toDate}`,
        `SpendReport_${from}_${to}.xlsx`,
      );
    } catch {
      toast.error('Failed to export spend report');
    } finally {
      setIsExporting(false);
    }
  };

  const { data: widgets } = useQuery({
    queryKey: ['analytics-widgets', companyId],
    queryFn: () => analyticsApi.getWidgets({ companyId }).then(r => r.data.data),
    enabled: !!companyId,
  });

  const { data: spend } = useQuery({
    queryKey: ['analytics-spend', companyId],
    queryFn: () => analyticsApi.getSpendSummary(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  const { data: performance } = useQuery({
    queryKey: ['analytics-performance', companyId],
    queryFn: () => analyticsApi.getVendorPerformance(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  const { data: funnel } = useQuery({
    queryKey: ['analytics-funnel', companyId],
    queryFn: () => analyticsApi.getFunnelStats(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  const { data: spendByCategory } = useQuery({
    queryKey: ['analytics-spend-category', companyId],
    queryFn:  () => analyticsApi.getSpendByCategory(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  const { data: cycleTime } = useQuery({
    queryKey: ['analytics-cycle-time', companyId],
    queryFn:  () => analyticsApi.getCycleTime(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  const { data: concentration } = useQuery({
    queryKey: ['analytics-concentration', companyId],
    queryFn:  () => analyticsApi.getVendorConcentration(companyId).then(r => r.data.data),
    enabled:  !!companyId && !isVendor,
  });

  if (isVendor) {
    const v = widgets as VendorDashboardWidgets | undefined;
    const tierColor = TIER_COLOR[v?.myTier ?? 'Bronze'] ?? TIER_COLOR.Bronze;
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-2">
          <LayoutDashboard className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <h1 className="text-xl sm:text-2xl font-semibold">My Dashboard</h1>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <StatCard label="Active POs"         value={String(v?.myActivePOs ?? 0)} />
          <StatCard label="Pending Invoices"   value={String(v?.myPendingInvoices ?? 0)} />
          <StatCard label="Performance Score"  value={`${v?.myLatestScore ?? 0}`} sub="out of 100" />
          <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
            <p className="text-sm text-muted-foreground">Current Tier</p>
            <p className="text-2xl font-bold mt-1" style={{ color: tierColor }}>
              {v?.myTier ?? 'Bronze'}
            </p>
          </div>
        </div>
      </div>
    );
  }

  const w = widgets as DashboardWidgets | undefined;

  // Funnel bar chart data
  const funnelData = funnel?.stages?.map(s => ({ name: s.stage, count: s.count })) ?? [];

  // Tier distribution from vendor performance
  const tierCounts: Record<string, number> = {};
  performance?.vendors?.forEach(v => {
    tierCounts[v.tier] = (tierCounts[v.tier] ?? 0) + 1;
  });
  const tierPieData = Object.entries(tierCounts).map(([name, value]) => ({ name, value }));

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <LayoutDashboard className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <h1 className="text-xl sm:text-2xl font-semibold">Dashboard</h1>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <input
            type="date"
            value={fromDate}
            onChange={e => setFromDate(e.target.value)}
            className="h-8 rounded-md border border-input bg-background px-2 text-sm"
          />
          <span className="text-muted-foreground text-sm">to</span>
          <input
            type="date"
            value={toDate}
            onChange={e => setToDate(e.target.value)}
            className="h-8 rounded-md border border-input bg-background px-2 text-sm"
          />
          <Button variant="outline" size="sm" onClick={handleExportSpend} disabled={isExporting} className="gap-1">
            <Download className="h-4 w-4" />
            <span>{isExporting ? 'Exporting...' : 'Export Spend Report'}</span>
          </Button>
        </div>
      </div>

      {/* Widget row */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <StatCard label="Spend This Month"   value={`${sym} ${fmt(w?.spendThisMonth ?? 0)}`} />
        <StatCard label="Pending Approvals"  value={String(w?.pendingApprovals ?? 0)} />
        <StatCard label="Active POs"         value={String(w?.activePOs ?? 0)} />
        <StatCard label="Pending Invoices"   value={String(w?.pendingInvoices ?? 0)} />
        <StatCard label="Open RFQs"          value={String(w?.openRFQs ?? 0)} />
        <StatCard label="Active Vendors"     value={String(w?.totalVendors ?? 0)} />
        <StatCard label="Total PRs (All)"    value={String(w?.totalPRs ?? 0)} />
      </div>

      {/* Spend area chart */}
      {spend && (
        <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-base">Monthly Spend ({sym})</h2>
            <div className="text-xs text-muted-foreground space-x-4">
              <span>This year: <strong>{sym} {fmt(spend.totalThisYear)}</strong></span>
              <span>Last year: <strong>{sym} {fmt(spend.totalLastYear)}</strong></span>
            </div>
          </div>
          <ResponsiveContainer width="100%" height={240}>
            <AreaChart data={spend.monthly} margin={{ top: 4, right: 8, bottom: 0, left: 0 }}>
              <defs>
                <linearGradient id="spendGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#3b82f6" stopOpacity={0.15} />
                  <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}    />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="month" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} tickFormatter={v => `${(v / 1_000_000).toFixed(0)}M`} />
              <Tooltip formatter={(v: number) => `${sym} ${fmt(v)}`} />
              <Area type="monotone" dataKey="amount" stroke="#3b82f6" fill="url(#spendGrad)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Bottom row: funnel + vendor tier pie */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Procurement Funnel */}
        {funnelData.length > 0 && (
          <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
            <h2 className="font-semibold text-base mb-4">Procurement Funnel ({new Date().getFullYear()})</h2>
            <div className="space-y-2">
              {funnelData.map((stage, i) => {
                const max = Math.max(...funnelData.map(s => s.count), 1);
                const pct = (stage.count / max) * 100;
                return (
                  <div key={stage.name} className="flex items-center gap-3">
                    <span className="text-xs text-muted-foreground w-36 shrink-0">{stage.name}</span>
                    <div className="flex-1 bg-slate-100 rounded-full h-5 overflow-hidden">
                      <div
                        className="h-5 rounded-full flex items-center px-2"
                        style={{ width: `${Math.max(pct, 4)}%`, backgroundColor: CHART_COLORS[i % CHART_COLORS.length] }}
                      >
                        <span className="text-white text-xs font-semibold">{stage.count}</span>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* Vendor Tier Distribution */}
        {tierPieData.length > 0 && (
          <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
            <h2 className="font-semibold text-base mb-4">Vendor Tier Distribution</h2>
            <ResponsiveContainer width="100%" height={200}>
              <PieChart>
                <Pie data={tierPieData} cx="50%" cy="50%" outerRadius={75} dataKey="value" label={({ name, value }) => `${name} (${value})`}>
                  {tierPieData.map(entry => (
                    <Cell key={entry.name} fill={TIER_COLOR[entry.name] ?? '#94a3b8'} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>

      {/* Cycle Time KPI Cards */}
      {(cycleTime?.length ?? 0) > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
          <h2 className="font-semibold text-base mb-4">Procurement Cycle Time (last 3 months avg)</h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {cycleTime!.map(stage => (
              <div key={stage.stage} className="text-center p-4 bg-slate-50 rounded-lg">
                <p className="text-xs text-muted-foreground mb-1">{stage.stage}</p>
                <p className="text-3xl font-bold text-indigo-600">{stage.avgDays}</p>
                <p className="text-xs text-muted-foreground mt-1">avg days</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Spend by Category Bar Chart */}
      {(spendByCategory?.length ?? 0) > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
          <h2 className="font-semibold text-base mb-4">Spend by Category ({new Date().getFullYear()})</h2>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={spendByCategory} margin={{ top: 4, right: 8, bottom: 40, left: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="categoryName" tick={{ fontSize: 10 }} angle={-30} textAnchor="end" interval={0} />
              <YAxis tick={{ fontSize: 11 }} tickFormatter={v => `${(v / 1_000_000).toFixed(0)}M`} />
              <Tooltip formatter={(v: number) => [`${sym} ${fmt(v)}`, 'Spend']} />
              <Bar dataKey="totalSpend" radius={[4, 4, 0, 0]}>
                {spendByCategory!.map((_, i) => (
                  <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Vendor Concentration Donut */}
      {(concentration?.topVendors?.length ?? 0) > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <h2 className="font-semibold text-base">Vendor Spend Concentration</h2>
            {concentration!.hasConcentrationRisk && (
              <span className="flex items-center gap-1 text-xs text-amber-600 bg-amber-50 px-2 py-0.5 rounded-full font-medium">
                <AlertTriangle className="h-3 w-3" />
                Concentration Risk &gt;40%
              </span>
            )}
          </div>
          <div className="flex flex-col lg:flex-row gap-6 items-center">
            <ResponsiveContainer width="100%" height={200}>
              <PieChart>
                <Pie
                  data={concentration!.topVendors}
                  dataKey="pctOfTotal"
                  nameKey="vendorName"
                  cx="50%" cy="50%"
                  innerRadius={50} outerRadius={80}
                  label={({ vendorName, pctOfTotal }) => `${pctOfTotal}%`}
                >
                  {concentration!.topVendors.map((_, i) => (
                    <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip formatter={(v: number) => [`${v}%`, 'Spend Share']} />
                <Legend formatter={(value) => value} />
              </PieChart>
            </ResponsiveContainer>
            <div className="w-full lg:w-64 space-y-2">
              {concentration!.topVendors.map((v, i) => (
                <div key={v.vendorId} className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <div className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: CHART_COLORS[i % CHART_COLORS.length] }} />
                    <span className="text-slate-700 truncate max-w-[140px]">{v.vendorName}</span>
                  </div>
                  <span className={`font-semibold ${v.pctOfTotal > 40 ? 'text-amber-600' : 'text-slate-700'}`}>
                    {v.pctOfTotal}%
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Vendor Performance Table */}
      {(performance?.vendors?.length ?? 0) > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
          <h2 className="font-semibold text-base mb-4">Top Vendor Performance</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs text-muted-foreground border-b">
                  <th className="pb-2 pr-4">Vendor</th>
                  <th className="pb-2 pr-4">Tier</th>
                  <th className="pb-2 pr-4">Score</th>
                  <th className="pb-2 pr-4">Delivery</th>
                  <th className="pb-2 pr-4">Quality</th>
                  <th className="pb-2 pr-4">Total Spend</th>
                  <th className="pb-2">POs</th>
                </tr>
              </thead>
              <tbody>
                {performance!.vendors.map(v => (
                  <tr key={v.vendorId} className="border-b border-slate-50 hover:bg-slate-50">
                    <td className="py-2 pr-4 font-medium">{v.vendorName}</td>
                    <td className="py-2 pr-4">
                      <span className="text-xs font-semibold" style={{ color: TIER_COLOR[v.tier] }}>
                        {v.tier}
                      </span>
                    </td>
                    <td className="py-2 pr-4">{v.totalScore.toFixed(1)}</td>
                    <td className="py-2 pr-4">{v.deliveryScore.toFixed(0)}</td>
                    <td className="py-2 pr-4">{v.qualityScore.toFixed(0)}</td>
                    <td className="py-2 pr-4">{sym} {fmt(v.totalSpend)}</td>
                    <td className="py-2">{v.pOCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
