import { useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { FileText, Trash2, Upload, AlertTriangle, Eye, Download, RefreshCw } from 'lucide-react';
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
import { SearchableSelect } from '@/shared/components/SearchableSelect';
import { vendorPortalApi, type DocumentStatus, type DocumentTypeConfigDto, type VendorDocumentDto } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';
import { fmtDate } from '@/shared/lib/date';

const GLOBAL_ALLOWED_LABEL = 'PDF, JPG, PNG, XLSX';
const GLOBAL_ALLOWED_EXTS  = ['.pdf', '.jpg', '.jpeg', '.png', '.xlsx', '.xls'];
const GLOBAL_MAX_MB        = 10;

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
  documentType: string;
  documentNumber: string;
  expiredAt: string;
  issuedAt: string;
  notes: string;
};

const EMPTY_FORM: UploadForm = {
  file: null,
  documentType: '',
  documentNumber: '',
  expiredAt: '',
  issuedAt: '',
  notes: '',
};

function getAllowedExtensions(dt: DocumentTypeConfigDto | undefined): string[] {
  if (!dt?.allowedExtensions) return GLOBAL_ALLOWED_EXTS;
  return dt.allowedExtensions.split(',').map((s) => s.trim().toLowerCase());
}

function getAllowedLabel(dt: DocumentTypeConfigDto | undefined): string {
  if (!dt?.allowedExtensions) return GLOBAL_ALLOWED_LABEL;
  return dt.allowedExtensions
    .split(',')
    .map((s) => s.trim().replace('.', '').toUpperCase())
    .join(', ');
}

export default function VendorPortalDocumentsPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);

  const [showUpload, setShowUpload] = useState(false);
  const [form, setForm] = useState<UploadForm>(EMPTY_FORM);
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null);
  const [replaceTarget, setReplaceTarget] = useState<{ id: string; documentType: string } | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [previewUrl, setPreviewUrl]   = useState<string | null>(null);
  const [previewName, setPreviewName] = useState<string | null>(null);

  const { data: docs = [], isLoading } = useQuery({
    queryKey: ['vendor-portal', 'documents', vendorId],
    queryFn: () => vendorPortalApi.getDocuments(vendorId!),
    enabled: !!vendorId,
  });

  const { data: docTypes = [] } = useQuery({
    queryKey: ['vendor-portal', 'document-types', vendorId],
    queryFn: () => vendorPortalApi.getDocumentTypes(vendorId!),
    enabled: !!vendorId,
  });

  const activeTypes = docTypes.filter((t) => t.isActive);
  const selectedType = activeTypes.find((t) => t.name === form.documentType);

  const allowedExts = getAllowedExtensions(selectedType);
  const allowedLabel = getAllowedLabel(selectedType);
  const maxMb = selectedType?.maxFileSizeMb ?? GLOBAL_MAX_MB;
  const maxBytes = maxMb * 1024 * 1024;
  const acceptAttr = allowedExts.join(',');

  const resetUpload = () => {
    setShowUpload(false);
    setReplaceTarget(null);
    setForm(EMPTY_FORM);
    setFileError(null);
    setUploadProgress(0);
    if (fileRef.current) fileRef.current.value = '';
  };

  const openReplace = (doc: VendorDocumentDto) => {
    setReplaceTarget({ id: doc.id, documentType: doc.documentType });
    setForm({
      ...EMPTY_FORM,
      documentType:   doc.documentType,
      documentNumber: doc.documentNumber ?? '',
      issuedAt:       doc.issuedAt       ?? '',
      expiredAt:      doc.expiredAt       ?? '',
      notes:          doc.notes           ?? '',
    });
    setShowUpload(true);
  };

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
      setUploadProgress(0);
      return vendorPortalApi.uploadDocument(vendorId!, data, setUploadProgress);
    },
    onSuccess: async () => {
      if (replaceTarget) {
        await vendorPortalApi.deleteDocument(vendorId!, replaceTarget.id).catch(() => {});
      }
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'documents', vendorId] });
      toast.success(replaceTarget ? 'Document replaced' : 'Document uploaded');
      resetUpload();
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

  const handleDownload = async (docId: string, fileName: string | null) => {
    try {
      const { url } = await vendorPortalApi.getDocumentUrl(vendorId!, docId, false);
      // Open presigned URL in a new tab — SeaweedFS returns Content-Disposition: attachment
      window.open(url, '_blank', 'noopener,noreferrer');
    } catch {
      toast.error('Failed to download file');
    }
  };

  const handlePreview = async (docId: string, fileName: string | null) => {
    try {
      const { url } = await vendorPortalApi.getDocumentUrl(vendorId!, docId, true);
      setPreviewName(fileName);
      setPreviewUrl(url);
    } catch {
      toast.error('Failed to load preview');
    }
  };

  const isPdf = (name: string | null) => name?.toLowerCase().endsWith('.pdf') ?? false;

  const set = (key: keyof UploadForm) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }));

  const onTypeChange = (value: string) => {
    setForm(f => ({ ...f, documentType: value, file: null }));
    setFileError(null);
    if (fileRef.current) fileRef.current.value = '';
  };

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    if (!file) { setForm(f => ({ ...f, file: null })); return; }

    const ext = '.' + file.name.split('.').pop()!.toLowerCase();
    if (!allowedExts.includes(ext)) {
      setFileError(`File type '${ext}' is not allowed. Accepted: ${allowedLabel}.`);
      e.target.value = '';
      setForm(f => ({ ...f, file: null }));
      return;
    }
    if (file.size > maxBytes) {
      setFileError(`File is too large. Maximum size for this type is ${maxMb} MB.`);
      e.target.value = '';
      setForm(f => ({ ...f, file: null }));
      return;
    }
    setFileError(null);
    setForm(f => ({ ...f, file }));
  };

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
            <div key={d.id} className="bg-white rounded-xl border border-slate-100 p-4">
              <div className="flex items-start gap-3">
                <FileText className="h-5 w-5 text-slate-400 flex-shrink-0 mt-0.5" />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <p className="text-sm font-medium text-slate-900">{d.documentType}</p>
                    <StatusBadge status={d.status} />
                  </div>
                  {d.documentNumber && <p className="text-xs text-slate-500 mt-0.5">#{d.documentNumber}</p>}
                  <p className="text-xs text-slate-400 mt-0.5">{d.fileName ?? 'Unknown file'}</p>
                  {d.expiredAt && <p className="text-xs text-slate-400 mt-0.5">Expires: {fmtDate(d.expiredAt)}</p>}
                </div>
                <div className="flex items-center gap-0.5 flex-shrink-0">
                  <Button
                    variant="ghost" size="icon"
                    className="h-8 w-8 text-slate-400 hover:text-blue-600"
                    title="Preview"
                    onClick={() => handlePreview(d.id, d.fileName)}
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost" size="icon"
                    className="h-8 w-8 text-slate-400 hover:text-amber-600"
                    title="Replace document"
                    onClick={() => openReplace(d)}
                  >
                    <RefreshCw className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost" size="icon"
                    className="h-8 w-8 text-slate-400 hover:text-emerald-600"
                    title="Download"
                    onClick={() => handleDownload(d.id, d.fileName)}
                  >
                    <Download className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost" size="icon"
                    className="h-8 w-8 text-red-400 hover:text-red-600"
                    title="Delete"
                    onClick={() => setDeleteTarget(d.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Upload modal */}
      <Dialog open={showUpload} onOpenChange={(v) => { if (!v && !uploadMut.isPending) resetUpload(); }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              {replaceTarget
                ? <><RefreshCw className="h-4 w-4" /> Replace Document</>
                : <><Upload className="h-4 w-4" /> Upload Document</>}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4 mt-2">
            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">Document Type <span className="text-red-500">*</span></label>
              {replaceTarget ? (
                <div className="rounded-md border border-input bg-muted/50 px-3 py-2 text-sm text-slate-600">
                  {form.documentType}
                </div>
              ) : (
                <SearchableSelect
                  options={activeTypes.map(t => ({ value: t.name, label: t.name }))}
                  value={form.documentType}
                  onChange={onTypeChange}
                  placeholder="Select a document type…"
                />
              )}
            </div>

            {form.documentType && (
              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-700">File <span className="text-red-500">*</span></label>
                <input
                  ref={fileRef}
                  type="file"
                  accept={acceptAttr}
                  className="w-full text-sm text-slate-600 file:mr-3 file:py-1.5 file:px-3 file:rounded-md file:border-0 file:text-xs file:font-medium file:bg-primary file:text-primary-foreground hover:file:opacity-90"
                  onChange={onFileChange}
                />
                <p className="text-xs text-slate-400">
                  Allowed: {allowedLabel} · Max {maxMb} MB
                </p>
                {fileError && (
                  <p className="text-xs text-red-500 flex items-center gap-1">
                    <AlertTriangle className="h-3 w-3" /> {fileError}
                  </p>
                )}
              </div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-700">
                  Document Number <span className="text-slate-400 font-normal">(optional)</span>
                </label>
                <Input placeholder="e.g. 123/SK/2024" value={form.documentNumber} onChange={set('documentNumber')} />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-slate-700">
                  Issued Date <span className="text-slate-400 font-normal">(optional)</span>
                </label>
                <Input type="date" value={form.issuedAt} onChange={set('issuedAt')} />
              </div>
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">
                Expiry Date <span className="text-slate-400 font-normal">(optional)</span>
              </label>
              <Input type="date" value={form.expiredAt} onChange={set('expiredAt')} />
            </div>

            <div className="space-y-1">
              <label className="text-xs font-medium text-slate-700">
                Notes <span className="text-slate-400 font-normal">(optional)</span>
              </label>
              <Input placeholder="e.g. Renewed certificate, replaces previous" value={form.notes} onChange={set('notes')} />
            </div>

            {!form.documentType && (
              <p className="text-xs text-amber-600 flex items-center gap-1">
                <AlertTriangle className="h-3 w-3" /> Please select a document type first.
              </p>
            )}
            {form.documentType && !form.file && !fileError && (
              <p className="text-xs text-amber-600 flex items-center gap-1">
                <AlertTriangle className="h-3 w-3" /> Please select a file to upload.
              </p>
            )}
          </div>

          {uploadMut.isPending && (
            <div className="space-y-1.5 mt-2">
              <div className="flex justify-between text-xs text-slate-500">
                <span>Uploading…</span>
                <span className="font-medium tabular-nums">{uploadProgress}%</span>
              </div>
              <div className="h-2 rounded-full bg-slate-100 overflow-hidden">
                <div
                  className="h-full rounded-full bg-primary transition-all duration-150 ease-out"
                  style={{ width: `${uploadProgress}%` }}
                />
              </div>
            </div>
          )}

          <div className="flex justify-end gap-2 mt-2">
            <Button variant="outline" onClick={resetUpload} disabled={uploadMut.isPending}>
              Cancel
            </Button>
            <Button
              onClick={() => uploadMut.mutate()}
              disabled={uploadMut.isPending || !form.file || !!fileError || !form.documentType}
              className="gap-1"
            >
              {replaceTarget ? <RefreshCw className="h-4 w-4" /> : <Upload className="h-4 w-4" />}
              {uploadMut.isPending
                ? `Uploading ${uploadProgress}%`
                : replaceTarget ? 'Replace' : 'Upload'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Document Preview Modal */}
      <Dialog open={!!previewUrl} onOpenChange={(v) => { if (!v) { setPreviewUrl(null); setPreviewName(null); } }}>
        <DialogContent className="max-w-4xl max-h-[90vh] flex flex-col">
          <DialogHeader>
            <DialogTitle>{previewName ?? 'Document Preview'}</DialogTitle>
          </DialogHeader>
          <div className="flex-1 overflow-hidden rounded-md border border-slate-200 min-h-[60vh]">
            {previewUrl && isPdf(previewName) ? (
              <iframe src={previewUrl} className="w-full h-full" style={{ minHeight: '60vh' }} title="document preview" />
            ) : previewUrl ? (
              <div className="flex items-center justify-center h-full p-4">
                <img src={previewUrl} alt={previewName ?? 'document'} className="max-w-full max-h-[65vh] object-contain rounded" />
              </div>
            ) : null}
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
