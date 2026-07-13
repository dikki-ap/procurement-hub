import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Users, Plus, Pencil, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable'
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal'
import { AuditCell } from '@/shared/components/AuditCell'
import { approverMatrixApi, type ApproverMatrixEntryDto } from '../api/approverMatrixApi'
import { extractApiError } from '@/shared/lib/apiError'

const REFERENCE_TYPES = ['PR', 'PO', 'RFQ'] as const
type ReferenceType = typeof REFERENCE_TYPES[number]

const REF_TYPE_BADGE: Record<ReferenceType, string> = {
  PR:  'bg-blue-50 text-blue-700',
  PO:  'bg-emerald-50 text-emerald-700',
  RFQ: 'bg-purple-50 text-purple-700',
}

const RefTypeBadge = ({ type }: { type: string }) => {
  const cls = REF_TYPE_BADGE[type as ReferenceType] ?? 'bg-slate-100 text-slate-600'
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>
      {type}
    </span>
  )
}

const EMPTY_FORM = {
  referenceType: 'PR' as string,
  level: '1',
  name: '',
  position: '',
  email: '',
}

type FormState = typeof EMPTY_FORM
type ModalState =
  | { mode: 'add' }
  | { mode: 'edit'; entry: ApproverMatrixEntryDto }
  | null

function formErrors(form: FormState) {
  const errs: string[] = []
  const lvl = parseInt(form.level)
  if (!form.referenceType) errs.push('Reference type is required.')
  if (isNaN(lvl) || lvl < 1) errs.push('Level must be 1 or higher.')
  if (!form.name.trim()) errs.push('Name is required.')
  if (form.name.trim().length > 200) errs.push('Name must be 200 characters or fewer.')
  if (!form.position.trim()) errs.push('Position is required.')
  if (form.position.trim().length > 200) errs.push('Position must be 200 characters or fewer.')
  if (!form.email.trim()) errs.push('Email is required.')
  if (form.email.trim().length > 256) errs.push('Email must be 256 characters or fewer.')
  if (form.email.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) {
    errs.push('Email must be a valid address.')
  }
  return errs
}

export default function ApproverMatrixPage() {
  const queryClient = useQueryClient()

  const [modal, setModal]             = useState<ModalState>(null)
  const [deleteTarget, setDeleteTarget] = useState<ApproverMatrixEntryDto | null>(null)
  const [form, setForm]               = useState<FormState>(EMPTY_FORM)
  const [validationErrors, setValidationErrors] = useState<string[]>([])

  const { data: entries = [], isLoading } = useQuery({
    queryKey: ['approver-matrix'],
    queryFn:  approverMatrixApi.getAll,
  })

  const openAdd = () => {
    setForm(EMPTY_FORM)
    setValidationErrors([])
    setModal({ mode: 'add' })
  }

  const openEdit = (entry: ApproverMatrixEntryDto) => {
    setForm({
      referenceType: entry.referenceType,
      level:         String(entry.level),
      name:          entry.name,
      position:      entry.position,
      email:         entry.email,
    })
    setValidationErrors([])
    setModal({ mode: 'edit', entry })
  }

  const closeModal = () => {
    setModal(null)
    setValidationErrors([])
  }

  const createMut = useMutation({
    mutationFn: () => approverMatrixApi.create({
      referenceType: form.referenceType,
      level:         parseInt(form.level),
      name:          form.name.trim(),
      position:      form.position.trim(),
      email:         form.email.trim(),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approver-matrix'] })
      toast.success('Approver added.')
      closeModal()
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to add approver')),
  })

  const updateMut = useMutation({
    mutationFn: () => {
      const entry = (modal as { mode: 'edit'; entry: ApproverMatrixEntryDto }).entry
      return approverMatrixApi.update(entry.id, {
        referenceType: form.referenceType,
        level:         parseInt(form.level),
        name:          form.name.trim(),
        position:      form.position.trim(),
        email:         form.email.trim(),
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approver-matrix'] })
      toast.success('Approver updated.')
      closeModal()
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update approver')),
  })

  const deleteMut = useMutation({
    mutationFn: (id: string) => approverMatrixApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approver-matrix'] })
      // Also invalidate the am-i-approver check since the matrix changed
      queryClient.invalidateQueries({ queryKey: ['am-i-approver'] })
      toast.success('Approver removed.')
      setDeleteTarget(null)
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to remove approver')),
  })

  const isEdit    = modal?.mode === 'edit'
  const isPending = createMut.isPending || updateMut.isPending

  const onSubmit = () => {
    const errs = formErrors(form)
    if (errs.length > 0) {
      setValidationErrors(errs)
      return
    }
    setValidationErrors([])
    isEdit ? updateMut.mutate() : createMut.mutate()
  }

  const columns: DataTableColumn<ApproverMatrixEntryDto>[] = [
    {
      key: 'referenceType',
      header: 'Reference Type',
      sortable: true,
      render: (r) => <RefTypeBadge type={r.referenceType} />,
    },
    {
      key: 'level',
      header: 'Level',
      sortable: true,
      render: (r) => <span className="font-bold text-center block">{r.level}</span>,
    },
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (r) => <span className="font-medium">{r.name}</span>,
    },
    {
      key: 'position',
      header: 'Position',
      sortable: true,
      render: (r) => <span className="text-sm text-slate-600">{r.position}</span>,
    },
    {
      key: 'email',
      header: 'Email',
      render: (r) => <span className="text-sm text-slate-600">{r.email}</span>,
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (r) => <AuditCell name={r.createdByName} at={r.createdAt} />,
    },
    {
      key: 'updatedAt',
      header: 'Last Modified',
      render: (r) => <AuditCell name={r.updatedByName} at={r.updatedAt} />,
    },
  ]

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Users className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Approver Matrix</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">
              Configure who approves each document type at each level
            </p>
          </div>
        </div>
        <Button size="sm" onClick={openAdd} className="gap-1">
          <Plus className="h-4 w-4" /> Add Approver
        </Button>
      </div>

      <DataTable
        data={entries as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search approvers..."
        emptyMessage="No approvers configured yet."
        rowActions={(row) => {
          const entry = row as unknown as ApproverMatrixEntryDto
          return (
            <>
              <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(entry)}>
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget(entry)}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          )
        }}
      />

      {/* Add / Edit modal */}
      <Dialog open={modal !== null} onOpenChange={(v) => { if (!v && !isPending) closeModal() }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{isEdit ? 'Edit Approver' : 'Add Approver'}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 mt-2">
            {validationErrors.length > 0 && (
              <ul className="text-xs text-red-600 space-y-0.5 bg-red-50 rounded-md px-3 py-2">
                {validationErrors.map((err) => (
                  <li key={err}>{err}</li>
                ))}
              </ul>
            )}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Reference Type</label>
                <select
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                  value={form.referenceType}
                  onChange={(e) => setForm((p) => ({ ...p, referenceType: e.target.value }))}
                >
                  {REFERENCE_TYPES.map((t) => (
                    <option key={t}>{t}</option>
                  ))}
                </select>
              </div>

              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Level</label>
                <Input
                  type="number"
                  min={1}
                  value={form.level}
                  onChange={(e) => setForm((p) => ({ ...p, level: e.target.value }))}
                  placeholder="e.g. 1"
                />
              </div>

              <div className="space-y-1 sm:col-span-2">
                <label className="text-xs font-medium text-muted-foreground">Full Name</label>
                <Input
                  value={form.name}
                  onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
                  placeholder="e.g. John Doe"
                  maxLength={200}
                />
              </div>

              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Position / Title</label>
                <Input
                  value={form.position}
                  onChange={(e) => setForm((p) => ({ ...p, position: e.target.value }))}
                  placeholder="e.g. Finance Manager"
                  maxLength={200}
                />
              </div>

              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Email</label>
                <Input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
                  placeholder="john@example.com"
                  maxLength={256}
                />
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" onClick={closeModal} disabled={isPending}>
                Cancel
              </Button>
              <Button onClick={onSubmit} disabled={isPending}>
                {isPending ? 'Saving…' : isEdit ? 'Update' : 'Save Approver'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Remove Approver"
        description={`Remove "${deleteTarget?.name}" from the approver matrix? This cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
