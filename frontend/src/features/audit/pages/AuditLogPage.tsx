import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { ScrollText } from 'lucide-react';
import { auditApi, type AuditLogDto, type AuditLogFilter } from '../api/auditApi';
import { fmtDateTime } from '@/shared/lib/date';

const ACTION_COLORS: Record<string, string> = {
  Created: 'text-emerald-600 bg-emerald-50',
  Updated: 'text-blue-600 bg-blue-50',
  Deleted: 'text-red-600 bg-red-50',
};

const ENTITY_TYPES = [
  'Vendor', 'VendorDocument', 'Material',
  'PurchaseRequisition', 'RFQ', 'VendorQuotation',
  'PurchaseOrder', 'ApprovalWorkflow', 'ApprovalHistory',
  'GoodsReceipt', 'Invoice', 'Contract',
  'ApprovalPolicy', 'Currency', 'ApproverAssignment',
];

function DiffCell({ before, after, changed }: { before?: string; after: string; changed?: string }) {
  const [open, setOpen] = useState(false);

  const changedCols: string[] = changed ? JSON.parse(changed) : [];
  const beforeObj  = before ? JSON.parse(before) : null;
  const afterObj   = JSON.parse(after);

  return (
    <div>
      <button
        onClick={() => setOpen(o => !o)}
        className="text-xs text-blue-600 hover:underline"
      >
        {open ? 'Hide diff' : 'View diff'}
      </button>
      {open && (
        <div className="mt-2 text-xs font-mono space-y-1 max-h-48 overflow-y-auto bg-slate-50 rounded p-2 border border-slate-200">
          {changedCols.length > 0
            ? changedCols.map(col => (
                <div key={col}>
                  <span className="font-semibold text-slate-600">{col}:</span>{' '}
                  <span className="line-through text-red-500">{String(beforeObj?.[col] ?? '')}</span>
                  {' → '}
                  <span className="text-emerald-600">{String(afterObj[col] ?? '')}</span>
                </div>
              ))
            : Object.entries(afterObj).map(([k, v]) => (
                <div key={k}>
                  <span className="font-semibold text-slate-600">{k}:</span>{' '}
                  <span className="text-slate-800">{String(v)}</span>
                </div>
              ))}
        </div>
      )}
    </div>
  );
}

export default function AuditLogPage() {
  const [filter, setFilter] = useState<AuditLogFilter>({ page: 1, pageSize: 50 });
  const [form, setForm]     = useState<AuditLogFilter>({});

  const { data, isFetching } = useQuery({
    queryKey: ['audit-log', filter],
    queryFn: () => auditApi.getAuditLog(filter).then(r => r.data.data),
  });

  const exportMut = useMutation({
    mutationFn: () => auditApi.exportAuditLog({
      entityType: filter.entityType,
      entityId:   filter.entityId,
      userId:     filter.userId,
      action:     filter.action,
      from:       filter.from,
      to:         filter.to,
    }),
    onSuccess: r => window.open(r.data.data, '_blank'),
  });

  const applyFilter = () => setFilter({ ...form, page: 1, pageSize: 50 });
  const resetFilter = () => { setForm({}); setFilter({ page: 1, pageSize: 50 }); };

  const goPage = (p: number) => setFilter(f => ({ ...f, page: p }));

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <ScrollText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Audit Log</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Track all system activity and changes</p>
          </div>
        </div>
        <button
          onClick={() => exportMut.mutate()}
          disabled={exportMut.isPending}
          className="px-3 py-1.5 text-sm bg-slate-800 text-white rounded-lg hover:bg-slate-700 disabled:opacity-50"
        >
          {exportMut.isPending ? 'Exporting…' : 'Export CSV'}
        </button>
      </div>

      {/* Filter panel */}
      <div className="bg-white rounded-xl border border-slate-100 p-5 shadow-sm">
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
          <select
            value={form.entityType ?? ''}
            onChange={e => setForm(f => ({ ...f, entityType: e.target.value || undefined }))}
            className="border border-slate-200 rounded-lg px-3 py-2 text-sm"
          >
            <option value="">All entity types</option>
            {ENTITY_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
          </select>

          <select
            value={form.action ?? ''}
            onChange={e => setForm(f => ({ ...f, action: e.target.value || undefined }))}
            className="border border-slate-200 rounded-lg px-3 py-2 text-sm"
          >
            <option value="">All actions</option>
            <option value="Created">Created</option>
            <option value="Updated">Updated</option>
            <option value="Deleted">Deleted</option>
          </select>

          <input
            type="text"
            placeholder="Entity ID (UUID)"
            value={form.entityId ?? ''}
            onChange={e => setForm(f => ({ ...f, entityId: e.target.value || undefined }))}
            className="border border-slate-200 rounded-lg px-3 py-2 text-sm"
          />

          <input
            type="datetime-local"
            value={form.from ?? ''}
            onChange={e => setForm(f => ({ ...f, from: e.target.value || undefined }))}
            className="border border-slate-200 rounded-lg px-3 py-2 text-sm"
          />

          <input
            type="datetime-local"
            value={form.to ?? ''}
            onChange={e => setForm(f => ({ ...f, to: e.target.value || undefined }))}
            className="border border-slate-200 rounded-lg px-3 py-2 text-sm"
          />

          <div className="flex gap-2">
            <button
              onClick={applyFilter}
              className="flex-1 bg-slate-800 text-white text-sm rounded-lg px-3 py-2 hover:bg-slate-700"
            >
              Filter
            </button>
            <button
              onClick={resetFilter}
              className="flex-1 border border-slate-200 text-sm rounded-lg px-3 py-2 hover:bg-slate-50"
            >
              Reset
            </button>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-x-auto">
        {isFetching && (
          <div className="px-5 py-3 text-sm text-muted-foreground border-b">Loading…</div>
        )}
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs text-muted-foreground border-b">
              <th className="px-4 py-3">Timestamp</th>
              <th className="px-4 py-3">Entity</th>
              <th className="px-4 py-3">Entity ID</th>
              <th className="px-4 py-3">Action</th>
              <th className="px-4 py-3">Actor</th>
              <th className="px-4 py-3">IP</th>
              <th className="px-4 py-3">Diff</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((row: AuditLogDto) => (
              <tr key={row.id} className="border-b border-slate-50 hover:bg-slate-50">
                <td className="px-4 py-2 whitespace-nowrap text-xs text-muted-foreground">
                  {fmtDateTime(row.createdAt)}
                </td>
                <td className="px-4 py-2 font-medium">{row.entityType}</td>
                <td className="px-4 py-2 text-xs font-mono text-slate-500 truncate max-w-[120px]">
                  {row.entityId}
                </td>
                <td className="px-4 py-2">
                  <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${ACTION_COLORS[row.action] ?? ''}`}>
                    {row.action}
                  </span>
                </td>
                <td className="px-4 py-2">
                  <p className="font-medium">{row.userFullName ?? '—'}</p>
                  <p className="text-xs text-muted-foreground">{row.userEmail}</p>
                </td>
                <td className="px-4 py-2 text-xs text-muted-foreground">{row.ipAddress ?? '—'}</td>
                <td className="px-4 py-2">
                  <DiffCell
                    before={row.beforeValues}
                    after={row.afterValues}
                    changed={row.changedColumns}
                  />
                </td>
              </tr>
            ))}
            {!isFetching && !data?.items.length && (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-muted-foreground text-sm">
                  No audit records found.
                </td>
              </tr>
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t text-sm">
            <span className="text-muted-foreground">
              Showing {((data.page - 1) * data.pageSize) + 1}–{Math.min(data.page * data.pageSize, data.total)} of {data.total}
            </span>
            <div className="flex gap-1">
              <button
                disabled={data.page <= 1}
                onClick={() => goPage(data.page - 1)}
                className="px-3 py-1 border rounded disabled:opacity-40 hover:bg-slate-50"
              >
                ← Prev
              </button>
              <button
                disabled={data.page >= data.totalPages}
                onClick={() => goPage(data.page + 1)}
                className="px-3 py-1 border rounded disabled:opacity-40 hover:bg-slate-50"
              >
                Next →
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
