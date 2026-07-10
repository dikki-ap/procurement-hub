import { useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { FileText, Trash2, Upload } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { vendorPortalApi, type DocumentStatus } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';

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

export default function VendorPortalDocumentsPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc = useQueryClient();

  const { data: docs = [], isLoading } = useQuery({
    queryKey: ['vendor-portal', 'documents', vendorId],
    queryFn: () => vendorPortalApi.getDocuments(vendorId!),
    enabled: !!vendorId,
  });

  const deleteMut = useMutation({
    mutationFn: (docId: string) => vendorPortalApi.deleteDocument(vendorId!, docId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'documents', vendorId] });
      toast.success('Document deleted');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Delete failed')),
  });

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
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-20 bg-slate-100 rounded-xl animate-pulse" />
          ))}
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
                      Exp: {new Date(d.expiredAt).toLocaleDateString()}
                    </p>
                  )}
                </div>
                <Button
                  variant="ghost" size="icon" className="h-8 w-8 text-red-400 hover:text-red-600"
                  onClick={() => { if (confirm('Delete this document?')) deleteMut.mutate(d.id); }}
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
