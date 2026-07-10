import { useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { FileText, Trash2, Upload, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { vendorPortalApi, type DocumentStatus, type DocumentType } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';

const DOCUMENT_TYPES: DocumentType[] = ['Siup', 'Npwp', 'Nib', 'Iso9001', 'Halal', 'Akta', 'Other'];

const StatusBadge = ({ status }: { status: DocumentStatus }) => {
  const cfg: Record<DocumentStatus, string> = {
    Active:  'bg-emerald-50 text-emerald-700',
    Expired: 'bg-red-50 text-red-700',
    Revoked: 'bg-slate-100 text-slate-500',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

type UploadForm = {
  file: File | null;
  documentType: DocumentType;
  documentNumber: string;
  expiredAt: string;
  issuedAt: string;
  notes: string;
};

const EMPTY_FORM: UploadForm = {
  file: null,
  documentType: 'Npwp',
  documentNumber: '',
  expiredAt: '',
  issuedAt: '',
  notes: '',
};

export default function VendorPortalDocumentsPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);

  const [showUpload, setShowUpload] = useState(false);
  const [form, setForm] = useState<UploadForm>(EMPTY_FORM);
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null);

  const { data: docs = [], isLoading } = useQuery({
    queryKey: ['vendor-portal', 'documents', vendorId],
    queryFn: () => vendorPortalApi.getDocuments(vendorId!),
    enabled: !!vendorId,
  });

  const uploadMut = useMutation({
    mutationFn: () => {
      if (!form.file) throw new Error('No file selected');
      const data = new FormData();
      data.append('file', form.file);
      data.append('documentType', form.documentType);
      if (form.documentNumber) data.append('documentNumber', form.documentNumber);
      if (form.expiredAt)      data.append('expiredAt', form.expiredAt);
      if (form.issuedAt)       data.append('issuedAt', form.issuedAt);
      if (form.notes)          data.append('notes', form.notes);
      return vendorPortalApi.uploadDocument(vendorId!, data);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'documents', vendorId] });
      toast.success('Document uploaded');
      setShowUpload(false);
      setForm(EMPTY_FORM);
      if (fileRef.current) fileRef.current.value = '';
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Upload failed')),
  });

  const deleteMut = useMutation({
    mutationFn: (docId: string) => vendorPortalApi.deleteDocument(vendorId!, docId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'documents', vendorId] });
      toast.success('Document deleted');
      setDeleteTarget(null);
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Delete failed')),
  });

  const set = (key: keyof UploadForm) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }));

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <FileText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Documents</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage your compliance documents</p>
          </div>
        </div>
        <Button size="sm" className="gap-1" onClick={() => setShowUpload(true)}>
          <Upload className="h-4 w-4" /> Upload Document
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => <div key={i} className="h-20 bg-slate-100 rounded-xl animate-pulse" />)}
        </div>
      ) : docs.length === 0 ? (
        <div className="text-center py-16 text-slate-400">
          <FileText className="h-10 w-10 mx-auto mb-3 opacity-40" />
          <p className="text-sm">No documents uploaded yet.</p>
          <p className="text-xs mt-1">Upload NPWP, SIUP, NIB and other required documents.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {docs.map((d) => (
            <div key={d.id} className="flex items-center gap-4 bg-white rounded-xl border border-slate-100 p-4">
              <FileText className="h-5 w-5 text-slate-400 flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-slate-900">{d.documentType}</p>
                {d.documentNumber && <p className="text-xs text-slate-500">#{d.documentNumber}</p>}
                <p className="text-xs text-slate-400 mt-0.5">{d.fileName ?? 'Unknown file'}</p>
              </div>
              <div className="flex items-center gap-3">
                <div className="text-right">
                  <StatusBadge status={d.status} />
                  {d.expiredAt && (
                    <p className="text-xs text-slate-400 mt-1">
                      Exp: {new Date(d.expiredAt + 'Z').toLocaleDateString()}
                    </p>
                  )}
                </div>
                <Button
                  variant="ghost" size="icon" className="h-8 w-8 text-red-400 hover:text-red-600"
                  onClick={() => setDeleteTarget(d.id)}
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Upload modal */}
      <Dialog open={showUpload} onOpenChange={(v) => { if (!v && !uploadMut.isPending) { setShowUpload(false); setForm(EMPTY_FORM); } }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Upload className="h-4 w-4" /> Upload Document
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4 mt-2">
            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">Document Type <span className="text-red-500">*</span></label>
              <select
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={form.documentType}
                onChange={set('documentType') as React.ChangeEventHandler<HTMLSelectElement>}
              >
                {DOCUMENT_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">File <span className="text-red-500">*</span></label>
              <input
                ref={fileRef}
                type="file"
                className="w-full text-sm text-slate-600 file:mr-3 file:py-1.5 file:px-3 file:rounded-md file:border-0 file:text-xs file:font-medium file:bg-primary file:text-primary-foreground hover:file:opacity-90"
                onChange={(e) => setForm(f => ({ ...f, file: e.target.files?.[0] ?? null }))}
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-700">Document Number</label>
                <Input placeholder="e.g. 123/SK/2024" value={form.documentNumber} onChange={set('documentNumber')} />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-700">Issued Date</label>
                <Input type="date" value={form.issuedAt} onChange={set('issuedAt')} />
              </div>
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">Expiry Date</label>
              <Input type="date" value={form.expiredAt} onChange={set('expiredAt')} />
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">Notes</label>
              <Input placeholder="Optional notes" value={form.notes} onChange={set('notes')} />
            </div>

            {!form.file && (
              <p className="text-xs text-amber-600 flex items-center gap-1">
                <AlertTriangle className="h-3 w-3" /> Please select a file to upload.
              </p>
            )}
          </div>

          <div className="flex justify-end gap-2 mt-2">
            <Button variant="outline" onClick={() => { setShowUpload(false); setForm(EMPTY_FORM); }} disabled={uploadMut.isPending}>
              Cancel
            </Button>
            <Button onClick={() => uploadMut.mutate()} disabled={uploadMut.isPending || !form.file} className="gap-1">
              <Upload className="h-4 w-4" />
              {uploadMut.isPending ? 'Uploading…' : 'Upload'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      <ConfirmDeleteModal
        open={!!deleteTarget}
        title="Delete Document"
        description="Remove this document from your profile? This cannot be undone."
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
