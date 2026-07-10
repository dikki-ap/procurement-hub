import { useState } from 'react';
import { PauseCircle, Ban, RotateCcw, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog';

/* ── Suspend ──────────────────────────────────────────────────────── */

type SuspendProps = {
  open: boolean;
  vendorName: string;
  isPending?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
};

export function SuspendModal({ open, vendorName, isPending, onConfirm, onCancel }: SuspendProps) {
  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onCancel(); }}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-orange-600">
            <PauseCircle className="h-5 w-5" />
            Suspend Vendor
          </DialogTitle>
          <DialogDescription>
            Suspend <span className="font-medium text-slate-700">{vendorName}</span>? The vendor
            will be unable to participate in new RFQs until reinstated.
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end gap-2 mt-2">
          <Button variant="outline" onClick={onCancel} disabled={isPending}>
            Cancel
          </Button>
          <Button
            className="bg-orange-500 hover:bg-orange-600 text-white"
            onClick={onConfirm}
            disabled={isPending}
          >
            {isPending ? 'Suspending…' : 'Suspend'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

/* ── Blacklist ────────────────────────────────────────────────────── */

type BlacklistProps = {
  open: boolean;
  vendorName: string;
  isPending?: boolean;
  onConfirm: (reason: string) => void;
  onCancel: () => void;
};

export function BlacklistModal({ open, vendorName, isPending, onConfirm, onCancel }: BlacklistProps) {
  const [reason, setReason] = useState('');

  const handleCancel = () => {
    setReason('');
    onCancel();
  };

  const handleConfirm = () => {
    if (!reason.trim()) return;
    onConfirm(reason.trim());
    setReason('');
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) handleCancel(); }}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-red-600">
            <Ban className="h-5 w-5" />
            Blacklist Vendor
          </DialogTitle>
          <DialogDescription>
            Blacklist <span className="font-medium text-slate-700">{vendorName}</span>? This is a
            serious action. Provide a clear reason below.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-1 mt-1">
          <label className="text-xs font-medium text-slate-700">
            Reason <span className="text-red-500">*</span>
          </label>
          <Input
            placeholder="e.g. Repeated delivery failures, fraud"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            disabled={isPending}
            autoFocus
          />
          {reason.trim() === '' && (
            <p className="text-xs text-red-500 flex items-center gap-1">
              <AlertTriangle className="h-3 w-3" /> Reason is required.
            </p>
          )}
        </div>

        <div className="flex justify-end gap-2 mt-2">
          <Button variant="outline" onClick={handleCancel} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={isPending || !reason.trim()}
          >
            {isPending ? 'Blacklisting…' : 'Blacklist'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

/* ── Reinstate ────────────────────────────────────────────────────── */

type ReinstateProps = {
  open: boolean;
  vendorName: string;
  isPending?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
};

export function ReinstateModal({ open, vendorName, isPending, onConfirm, onCancel }: ReinstateProps) {
  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onCancel(); }}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-blue-600">
            <RotateCcw className="h-5 w-5" />
            Reinstate Vendor
          </DialogTitle>
          <DialogDescription>
            Reinstate <span className="font-medium text-slate-700">{vendorName}</span>? The vendor
            will return to Active status and regain portal access.
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end gap-2 mt-2">
          <Button variant="outline" onClick={onCancel} disabled={isPending}>
            Cancel
          </Button>
          <Button
            className="bg-blue-500 hover:bg-blue-600 text-white"
            onClick={onConfirm}
            disabled={isPending}
          >
            {isPending ? 'Reinstating…' : 'Reinstate'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
