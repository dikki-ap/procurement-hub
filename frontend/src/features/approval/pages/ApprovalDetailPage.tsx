import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, CheckCircle2, RotateCcw, XCircle, UserCheck, Clock } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { approvalApi, type ApprovalActionType, type WorkflowStatus } from '../api/approvalApi';
import { useAuthStore } from '@/stores/authStore';
import ApprovalActionModal from './ApprovalActionModal';
import { extractApiError } from '@/shared/lib/apiError';

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
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

const actionIcon: Record<ApprovalActionType, React.ReactNode> = {
  Approve:  <CheckCircle2 className="h-4 w-4 text-emerald-600" />,
  Revise:   <RotateCcw    className="h-4 w-4 text-amber-500" />,
  Reject:   <XCircle      className="h-4 w-4 text-red-500" />,
  Delegate: <UserCheck    className="h-4 w-4 text-blue-500" />,
};

type ModalType = 'revise' | 'reject' | 'delegate' | null;

export default function ApprovalDetailPage() {
  const { id }      = useParams<{ id: string }>();
  const navigate    = useNavigate();
  const queryClient = useQueryClient();
  const { user }    = useAuthStore();
  const [modal, setModal] = useState<ModalType>(null);

  const { data: workflow, isLoading } = useQuery({
    queryKey: ['approval-workflow', id],
    queryFn:  () => approvalApi.getWorkflow(id!).then(r => r.data),
    enabled:  !!id,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['approval-workflow', id] });
    queryClient.invalidateQueries({ queryKey: ['approval-inbox'] });
  };

  const approveMut = useMutation({
    mutationFn: () => approvalApi.approve(id!, user!.id, user!.fullName),
    onSuccess:  () => { invalidate(); toast.success('Approved successfully.'); },
    onError:    (error: unknown) => toast.error(extractApiError(error, 'Failed to approve')),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!workflow)  return <div className="p-6 text-red-500">Workflow not found.</div>;

  const isPending        = workflow.status === 'Pending';
  const isCurrentApprover = workflow.assignments.some(
    a => a.assignedUserId === user?.id && a.level === workflow.currentLevel);

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/app/approval/inbox')}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back to Inbox
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{workflow.referenceNumber}</h1>
            <StatusBadge status={workflow.status} />
          </div>
          <p className="text-sm text-muted-foreground">{workflow.referenceTitle}</p>
        </div>
      </div>

      {/* Info grid */}
      <div className="grid grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Type',        value: workflow.referenceType },
          { label: 'Total Value', value: `Rp ${fmt(workflow.totalValue)}` },
          { label: 'Level',       value: `${workflow.currentLevel} / ${workflow.maxLevel}` },
          { label: 'Iteration',   value: workflow.iteration },
          { label: 'Submitted',   value: new Date(workflow.createdAt).toLocaleDateString('id-ID') },
          { label: 'Completed',   value: workflow.completedAt ? new Date(workflow.completedAt).toLocaleDateString('id-ID') : '—' },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{String(value)}</dd>
          </div>
        ))}
      </div>

      {/* Assignees */}
      <div>
        <h2 className="text-base font-semibold mb-2">Approvers</h2>
        <div className="flex flex-wrap gap-2">
          {workflow.assignments.map((a, i) => (
            <div key={i} className={`px-3 py-1.5 rounded-md border text-sm flex items-center gap-2
              ${a.level === workflow.currentLevel ? 'border-blue-300 bg-blue-50' : 'bg-muted/30'}`}>
              <span className="font-medium text-xs text-muted-foreground">L{a.level}</span>
              <span>{a.assignedUserName}</span>
              {a.isDelegate && <span className="text-xs text-muted-foreground">(delegate)</span>}
            </div>
          ))}
        </div>
      </div>

      {/* Action buttons */}
      {isPending && isCurrentApprover && (
        <div className="flex gap-2 p-4 rounded-lg bg-blue-50 border border-blue-200">
          <Button onClick={() => approveMut.mutate()} disabled={approveMut.isPending}>
            <CheckCircle2 className="h-4 w-4 mr-2" />
            {approveMut.isPending ? 'Approving…' : 'Approve'}
          </Button>
          <Button variant="outline" onClick={() => setModal('revise')}>
            <RotateCcw className="h-4 w-4 mr-2" /> Revise
          </Button>
          <Button variant="outline" className="text-red-600" onClick={() => setModal('reject')}>
            <XCircle className="h-4 w-4 mr-2" /> Reject
          </Button>
          <Button variant="ghost" onClick={() => setModal('delegate')}>
            <UserCheck className="h-4 w-4 mr-2" /> Delegate
          </Button>
        </div>
      )}

      {/* Timeline */}
      <div>
        <h2 className="text-base font-semibold mb-3">History</h2>
        {workflow.history.length === 0 ? (
          <p className="text-sm text-muted-foreground">No actions taken yet.</p>
        ) : (
          <div className="relative pl-6 space-y-4">
            <div className="absolute left-2 top-2 bottom-2 w-px bg-border" />
            {workflow.history.map(h => (
              <div key={h.id} className="relative flex gap-3">
                <div className="absolute -left-4 flex items-center justify-center w-4 h-4 rounded-full bg-background border">
                  {actionIcon[h.action]}
                </div>
                <div className="ml-4 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium">{h.actorName}</span>
                    <span className="text-xs text-muted-foreground">{h.action} (Level {h.level})</span>
                    <span className="ml-auto text-xs text-muted-foreground">
                      {new Date(h.actedAt).toLocaleString('id-ID')}
                    </span>
                  </div>
                  {h.reason && (
                    <p className="text-xs text-muted-foreground mt-0.5 italic">"{h.reason}"</p>
                  )}
                </div>
              </div>
            ))}
            {/* Pending indicator */}
            {isPending && (
              <div className="relative flex gap-3">
                <div className="absolute -left-4 flex items-center justify-center w-4 h-4 rounded-full bg-background border border-blue-300">
                  <Clock className="h-3 w-3 text-blue-500" />
                </div>
                <div className="ml-4">
                  <span className="text-sm text-muted-foreground">Awaiting Level {workflow.currentLevel} approval…</span>
                </div>
              </div>
            )}
          </div>
        )}
      </div>

      <ApprovalActionModal
        workflowId={id!}
        type={modal}
        user={user}
        onClose={() => setModal(null)}
        onSuccess={() => { setModal(null); invalidate(); }}
      />
    </div>
  );
}
