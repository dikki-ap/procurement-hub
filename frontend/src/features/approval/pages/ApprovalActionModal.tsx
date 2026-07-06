import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { approvalApi } from '../api/approvalApi';

type ModalType = 'revise' | 'reject' | 'delegate' | null;

interface Props {
  workflowId: string;
  type: ModalType;
  user: { id: string; fullName: string } | null;
  onClose: () => void;
  onSuccess: () => void;
}

export default function ApprovalActionModal({ workflowId, type, user, onClose, onSuccess }: Props) {
  const [reason,            setReason]            = useState('');
  const [delegateUserId,    setDelegateUserId]    = useState('');
  const [delegateUserName,  setDelegateUserName]  = useState('');

  const reviseMut = useMutation({
    mutationFn: () => approvalApi.revise(workflowId, user!.id, user!.fullName, reason),
    onSuccess:  () => { toast.success('Revision requested.'); onSuccess(); },
    onError:    (e: any) => toast.error(e?.response?.data?.message ?? 'Failed to revise'),
  });

  const rejectMut = useMutation({
    mutationFn: () => approvalApi.reject(workflowId, user!.id, user!.fullName, reason),
    onSuccess:  () => { toast.success('Rejected.'); onSuccess(); },
    onError:    (e: any) => toast.error(e?.response?.data?.message ?? 'Failed to reject'),
  });

  const delegateMut = useMutation({
    mutationFn: () => approvalApi.delegate(workflowId, user!.id, user!.fullName, delegateUserId, delegateUserName),
    onSuccess:  () => { toast.success('Approval delegated.'); onSuccess(); },
    onError:    (e: any) => toast.error(e?.response?.data?.message ?? 'Failed to delegate'),
  });

  if (!type) return null;

  const titleMap = { revise: 'Request Revision', reject: 'Reject Document', delegate: 'Delegate Approval' };
  const reasonTooShort = (type === 'revise' || type === 'reject') && reason.trim().length < 20;

  const handleSubmit = () => {
    if (type === 'revise')   reviseMut.mutate();
    if (type === 'reject')   rejectMut.mutate();
    if (type === 'delegate') delegateMut.mutate();
  };

  const isPending = reviseMut.isPending || rejectMut.isPending || delegateMut.isPending;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-background rounded-lg shadow-xl p-6 w-full max-w-md space-y-4">
        <h2 className="text-lg font-semibold">{titleMap[type]}</h2>

        {(type === 'revise' || type === 'reject') && (
          <div className="space-y-1">
            <label className="text-sm font-medium">
              Reason <span className="text-muted-foreground text-xs">(min 20 characters)</span>
            </label>
            <textarea
              className="w-full rounded-md border px-3 py-2 text-sm min-h-[100px] resize-none focus:outline-none focus:ring-2 focus:ring-ring"
              placeholder="Provide a clear reason for this action..."
              value={reason}
              onChange={e => setReason(e.target.value)}
            />
            {reason.length > 0 && reasonTooShort && (
              <p className="text-xs text-amber-600">{20 - reason.trim().length} more characters needed.</p>
            )}
          </div>
        )}

        {type === 'delegate' && (
          <div className="space-y-3">
            <div className="space-y-1">
              <label className="text-sm font-medium">Delegate To (User ID)</label>
              <Input
                placeholder="UUID of the user to delegate to"
                value={delegateUserId}
                onChange={e => setDelegateUserId(e.target.value)}
              />
            </div>
            <div className="space-y-1">
              <label className="text-sm font-medium">Delegate To (Full Name)</label>
              <Input
                placeholder="Full name of the delegated approver"
                value={delegateUserName}
                onChange={e => setDelegateUserName(e.target.value)}
              />
            </div>
          </div>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isPending}>Cancel</Button>
          <Button
            onClick={handleSubmit}
            disabled={isPending || (type !== 'delegate' && reasonTooShort) ||
              (type === 'delegate' && (!delegateUserId || !delegateUserName))}
            className={type === 'reject' ? 'bg-red-600 hover:bg-red-700' : ''}
          >
            {isPending ? 'Submitting…' : 'Confirm'}
          </Button>
        </div>
      </div>
    </div>
  );
}
