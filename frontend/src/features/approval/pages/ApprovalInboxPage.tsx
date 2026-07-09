import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Inbox, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { approvalApi, type WorkflowStatus } from '../api/approvalApi';
import { useAuthStore } from '@/stores/authStore';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

const StatusBadge = ({ status }: { status: WorkflowStatus }) => {
  const cfg: Record<WorkflowStatus, string> = {
    Pending:   'bg-blue-50 text-blue-700',
    Approved:  'bg-emerald-50 text-emerald-700',
    Revised:   'bg-amber-50 text-amber-700',
    Rejected:  'bg-red-50 text-red-700',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

export default function ApprovalInboxPage() {
  const navigate  = useNavigate();
  const { user }  = useAuthStore();
  const companyId = user?.companyId ?? '';
  const userId    = user?.id ?? '';

  const { data: items = [], isLoading } = useQuery({
    queryKey: ['approval-inbox', userId, companyId],
    queryFn:  () => approvalApi.getInbox(userId, companyId),
    enabled:  !!userId && !!companyId,
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;

  return (
    <div className="flex flex-col min-h-[calc(100vh-112px)]">
      <div className="flex items-center gap-3">
        <Inbox className="h-5 w-5 text-muted-foreground" />
        <h1 className="text-2xl font-semibold">Approval Inbox</h1>
        {items.length > 0 && (
          <span className="ml-auto text-sm text-muted-foreground">{items.length} pending</span>
        )}
      </div>

      {items.length === 0 ? (
        <div className="flex-1 flex flex-col items-center justify-center text-muted-foreground">
          <Inbox className="h-12 w-12 mb-3 opacity-30" />
          <p className="text-sm">No pending approvals.</p>
        </div>
      ) : (
        <div className="rounded-md border overflow-hidden mt-6">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                {['Document', 'Type', 'Total Value', 'Level', 'Submitted', 'Status', ''].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {items.map(item => (
                <tr key={item.workflowId} className="border-t hover:bg-muted/20">
                  <td className="px-3 py-2">
                    <p className="font-medium">{item.referenceNumber}</p>
                    <p className="text-xs text-muted-foreground truncate max-w-[200px]">{item.referenceTitle}</p>
                  </td>
                  <td className="px-3 py-2 text-muted-foreground">{item.referenceType}</td>
                  <td className="px-3 py-2 font-mono text-xs">Rp {fmt(item.totalValue)}</td>
                  <td className="px-3 py-2 text-center">
                    <span className="text-xs font-medium">{item.currentLevel}/{item.maxLevel}</span>
                  </td>
                  <td className="px-3 py-2 text-muted-foreground">
                    {new Date(item.createdAt).toLocaleDateString('id-ID')}
                  </td>
                  <td className="px-3 py-2"><StatusBadge status={item.status} /></td>
                  <td className="px-3 py-2">
                    <Button
                      size="sm" variant="ghost"
                      onClick={() => navigate(`/app/approval/${item.workflowId}`)}
                    >
                      Review <ChevronRight className="h-4 w-4 ml-1" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
