import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { documentTypeApi, type UpdateDocumentTypeRequest } from '../api/documentTypeApi';
import { extractApiError } from '@/shared/lib/apiError';

const EXTENSION_OPTIONS = [
  { value: '.pdf',  label: 'PDF' },
  { value: '.jpg',  label: 'JPG' },
  { value: '.jpeg', label: 'JPEG' },
  { value: '.png',  label: 'PNG' },
  { value: '.xlsx', label: 'XLSX' },
  { value: '.xls',  label: 'XLS' },
];

const schema = z.object({
  name: z.string().min(1).max(100),
  isActive: z.boolean(),
  extensions: z.array(z.string()),
  maxFileSizeMb: z.coerce.number().int().min(1).max(100),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { name: '', isActive: true, extensions: [], maxFileSizeMb: 10 };

type Props = { open: boolean; id?: string; onClose: () => void };

export function DocumentTypeFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();

  const { data: existing } = useQuery({
    queryKey: ['document-types', id],
    queryFn: () => documentTypeApi.getById(id!),
    enabled: isEdit && open,
  });

  const { register, handleSubmit, reset, control, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULTS,
  });

  useEffect(() => {
    if (!open) { reset(DEFAULTS); return; }
    if (!id)   { reset(DEFAULTS); return; }
  }, [open, id, reset]);

  useEffect(() => {
    if (existing && open) {
      reset({
        name: existing.name,
        isActive: existing.isActive,
        extensions: existing.allowedExtensions
          ? existing.allowedExtensions.split(',').map((s) => s.trim())
          : [],
        maxFileSizeMb: existing.maxFileSizeMb,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: (d: FormData) => documentTypeApi.create({
      name: d.name,
      allowedExtensions: d.extensions.length > 0 ? d.extensions.join(',') : null,
      maxFileSizeMb: d.maxFileSizeMb,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['document-types'] });
      toast.success('Document type created');
      onClose();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: (d: FormData) => documentTypeApi.update(id!, {
      name: d.name,
      isActive: d.isActive,
      allowedExtensions: d.extensions.length > 0 ? d.extensions.join(',') : null,
      maxFileSizeMb: d.maxFileSizeMb,
    } as UpdateDocumentTypeRequest),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['document-types'] });
      toast.success('Document type updated');
      onClose();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Update failed')),
  });

  const isPending = createMut.isPending || updateMut.isPending;
  const onSubmit = (d: FormData) => isEdit ? updateMut.mutate(d) : createMut.mutate(d);

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Document Type' : 'Add Document Type'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="SIUP" className="uppercase" />
            <p className="text-xs text-slate-400 mt-0.5">Will be stored in uppercase.</p>
            {errors.name && <p className="text-xs text-destructive mt-1">{errors.name.message}</p>}
          </div>

          <div>
            <Label>Allowed File Types</Label>
            <p className="text-xs text-slate-400 mb-2">Leave unchecked to allow all supported types.</p>
            <div className="grid grid-cols-3 gap-2">
              {EXTENSION_OPTIONS.map((opt) => (
                <Controller
                  key={opt.value}
                  name="extensions"
                  control={control}
                  render={({ field }) => {
                    const checked = field.value.includes(opt.value);
                    return (
                      <label className={`flex items-center gap-1.5 text-xs rounded-md border px-2 py-1.5 cursor-pointer transition-colors ${checked ? 'border-primary bg-primary/5 text-primary' : 'border-slate-200 text-slate-600 hover:bg-slate-50'}`}>
                        <input
                          type="checkbox"
                          className="hidden"
                          checked={checked}
                          onChange={(e) => {
                            if (e.target.checked) {
                              field.onChange([...field.value, opt.value]);
                            } else {
                              field.onChange(field.value.filter((v) => v !== opt.value));
                            }
                          }}
                        />
                        {opt.label}
                      </label>
                    );
                  }}
                />
              ))}
            </div>
          </div>

          <div>
            <Label>Max File Size (MB)</Label>
            <Input type="number" min={1} max={100} {...register('maxFileSizeMb')} />
            {errors.maxFileSizeMb && <p className="text-xs text-destructive mt-1">{errors.maxFileSizeMb.message}</p>}
          </div>

          {isEdit && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" {...register('isActive')} />
              <Label htmlFor="isActive">Active</Label>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={onClose} disabled={isPending}>Cancel</Button>
            <Button type="submit" disabled={isPending}>{isEdit ? 'Update' : 'Create'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
