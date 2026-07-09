import React, { useState, useMemo } from 'react';
import { Search, ChevronLeft, ChevronRight, ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react';
import { Input } from '@/components/ui/input';

export interface DataTableColumn<T> {
  key: string;
  header: string;
  sortable?: boolean;
  render?: (row: T) => React.ReactNode;
  searchValue?: (row: T) => string;
}

export interface DataTableProps<T extends Record<string, unknown>> {
  data: T[];
  columns: DataTableColumn<T>[];
  isLoading?: boolean;
  searchPlaceholder?: string;
  toolbar?: React.ReactNode;
  rowActions?: (row: T) => React.ReactNode;
  defaultPageSize?: number;
  emptyMessage?: string;
}

type SortDir = 'asc' | 'desc';

function SortIcon({ columnKey, sortKey, sortDir }: { columnKey: string; sortKey: string; sortDir: SortDir }) {
  if (sortKey !== columnKey) return <ChevronsUpDown className="ml-1 inline h-3.5 w-3.5 text-slate-400" />;
  return sortDir === 'asc'
    ? <ChevronUp className="ml-1 inline h-3.5 w-3.5 text-blue-500" />
    : <ChevronDown className="ml-1 inline h-3.5 w-3.5 text-blue-500" />;
}

export function DataTable<T extends Record<string, unknown>>({
  data,
  columns,
  isLoading = false,
  searchPlaceholder = 'Search...',
  toolbar,
  rowActions,
  defaultPageSize = 10,
  emptyMessage = 'No records found.',
}: DataTableProps<T>) {
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState('');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(defaultPageSize);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return data;
    return data.filter((row) =>
      columns.some((col) => {
        const text = col.searchValue
          ? col.searchValue(row)
          : String(row[col.key] ?? '');
        return text.toLowerCase().includes(q);
      })
    );
  }, [data, columns, search]);

  const sorted = useMemo(() => {
    if (!sortKey) return filtered;
    return [...filtered].sort((a, b) => {
      const av = String(a[sortKey] ?? '');
      const bv = String(b[sortKey] ?? '');
      const cmp = av.localeCompare(bv, undefined, { numeric: true, sensitivity: 'base' });
      return sortDir === 'asc' ? cmp : -cmp;
    });
  }, [filtered, sortKey, sortDir]);

  const totalItems = sorted.length;
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
  const safePage = Math.min(page, totalPages);
  const startIndex = (safePage - 1) * pageSize;
  const pageRows = sorted.slice(startIndex, startIndex + pageSize);

  const rangeStart = totalItems === 0 ? 0 : startIndex + 1;
  const rangeEnd = Math.min(startIndex + pageSize, totalItems);

  function handleSort(key: string) {
    if (sortKey === key) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDir('asc');
    }
    setPage(1);
  }

  function handleSearch(value: string) {
    setSearch(value);
    setPage(1);
  }

  function handlePageSize(value: number) {
    setPageSize(value);
    setPage(1);
  }

  const skeletonCount = pageSize < 5 ? pageSize : 5;
  const colSpan = columns.length + (rowActions ? 1 : 0);

  return (
    <div className="space-y-3">
      {/* Toolbar row */}
      <div className="flex items-center justify-between gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
          <Input
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            placeholder={searchPlaceholder}
            className="pl-9 h-9 text-sm bg-white border-slate-200 focus-visible:ring-blue-500/30"
          />
        </div>
        {toolbar && <div className="flex items-center gap-2">{toolbar}</div>}
      </div>

      {/* Table container */}
      <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
        <div className="overflow-x-auto">
        <table className="w-full text-sm min-w-[600px]">
          <thead>
            <tr className="border-b border-slate-100">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={`px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wide whitespace-nowrap select-none ${
                    col.sortable ? 'cursor-pointer hover:text-slate-700 transition-colors duration-100' : ''
                  }`}
                  onClick={col.sortable ? () => handleSort(col.key) : undefined}
                >
                  {col.header}
                  {col.sortable && (
                    <SortIcon columnKey={col.key} sortKey={sortKey} sortDir={sortDir} />
                  )}
                </th>
              ))}
              {rowActions && (
                <th className="px-4 py-3 text-right text-xs font-medium text-slate-500 uppercase tracking-wide">
                  Actions
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: skeletonCount }).map((_, i) => (
                <tr key={i} className="border-b border-slate-50 last:border-0">
                  {Array.from({ length: colSpan }).map((__, j) => (
                    <td key={j} className="px-4 py-3">
                      <div className="h-4 bg-slate-100 rounded animate-pulse" style={{ width: `${60 + (j * 17) % 40}%` }} />
                    </td>
                  ))}
                </tr>
              ))
            ) : pageRows.length === 0 ? (
              <tr>
                <td colSpan={colSpan} className="px-4 py-12 text-center text-sm text-slate-400">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              pageRows.map((row, rowIdx) => (
                <tr
                  key={rowIdx}
                  className="border-b border-slate-50 last:border-0 hover:bg-slate-50 transition-colors duration-100"
                >
                  {columns.map((col) => (
                    <td key={col.key} className="px-4 py-3 text-slate-700">
                      {col.render ? col.render(row) : String(row[col.key] ?? '')}
                    </td>
                  ))}
                  {rowActions && (
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        {rowActions(row)}
                      </div>
                    </td>
                  )}
                </tr>
              ))
            )}
          </tbody>
        </table>
        </div>
      </div>

      {/* Footer: per-page + pagination */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2 text-sm text-slate-500">
        <div className="flex items-center gap-2">
          <span>Rows per page</span>
          <select
            value={pageSize}
            onChange={(e) => handlePageSize(Number(e.target.value))}
            className="h-8 rounded-md border border-slate-200 bg-white px-2 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500/30 cursor-pointer"
          >
            {[10, 25, 50, 100].map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-3">
          <span className="tabular-nums">
            {rangeStart}–{rangeEnd} of {totalItems}
          </span>
          <div className="flex items-center gap-1">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={safePage <= 1}
              className="flex h-8 w-8 items-center justify-center rounded-md border border-slate-200 bg-white text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors duration-100"
              aria-label="Previous page"
            >
              <ChevronLeft className="h-4 w-4" />
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={safePage >= totalPages}
              className="flex h-8 w-8 items-center justify-center rounded-md border border-slate-200 bg-white text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors duration-100"
              aria-label="Next page"
            >
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
