import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Users, Pencil, UserX, CheckCircle2, AlertTriangle,
} from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { useAuthStore } from '@/stores/authStore';
import { userApi, type UserDto, type UpdateUserRequest, INTERNAL_ROLES } from '../api/userApi';
import { departmentApi, type DepartmentDto } from '@/features/master-data/department/api/departmentApi';
import { extractApiError } from '@/shared/lib/apiError';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const ROLE_COLORS: Record<string, string> = {
  super_admin: 'bg-purple-50 text-purple-700',
  purchasing:  'bg-blue-50 text-blue-700',
  finance:     'bg-emerald-50 text-emerald-700',
  approver:    'bg-amber-50 text-amber-700',
  requester:   'bg-indigo-50 text-indigo-700',
  management:  'bg-slate-100 text-slate-700',
};

const RoleBadge = ({ role }: { role: string }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${ROLE_COLORS[role] ?? 'bg-slate-100 text-slate-500'}`}>
    {role.replace('_', ' ')}
  </span>
);

type EditState = { userId: string; form: UpdateUserRequest } | null;

export default function UserListPage() {
  const qc = useQueryClient();
  const { user: me } = useAuthStore();
  const companyId = me?.companyId ?? '';
  const myId = me?.id ?? '';

  const [editState, setEditState] = useState<EditState>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<{ id: string; name: string } | null>(null);

  const { data: users = [], isLoading } = useQuery({
    queryKey: ['users', companyId],
    queryFn: () => userApi.getAll(companyId),
    enabled: !!companyId,
  });

  const { data: departments = [] } = useQuery({
    queryKey: ['departments', companyId],
    queryFn: () => departmentApi.getAll(companyId),
    enabled: !!companyId,
  });

  const deptMap = new Map<string, string>(departments.map((d: DepartmentDto) => [d.id, d.name]));

  const updateMut = useMutation({
    mutationFn: ({ userId, form }: { userId: string; form: UpdateUserRequest }) =>
      userApi.update(userId, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['users'] });
      toast.success('User updated');
      setEditState(null);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Update failed')),
  });

  const deactivateMut = useMutation({
    mutationFn: (id: string) => userApi.deactivate(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['users'] });
      toast.success('User deactivated');
      setDeactivateTarget(null);
    },
    onError: (e: unknown) => {
      toast.error(extractApiError(e, 'Deactivation failed'));
      setDeactivateTarget(null);
    },
  });

  const openEdit = (u: UserDto) => {
    setEditState({
      userId: u.id,
      form: {
        departmentId: u.departmentId,
        role:         u.role,
        isActive:     u.isActive,
      },
    });
  };

  const handleEditSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editState) updateMut.mutate(editState);
  };

  const columns: DataTableColumn<UserDto>[] = [
    {
      key: 'fullName',
      header: 'Name',
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-medium text-slate-900">{row.fullName}</p>
          <p className="text-xs text-slate-500">{row.email}</p>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      sortable: true,
      render: (row) => <RoleBadge role={row.role} />,
    },
    {
      key: 'departmentId',
      header: 'Department',
      render: (row) => (
        <span className="text-sm text-slate-600">
          {row.departmentId ? (deptMap.get(row.departmentId) ?? '—') : '—'}
        </span>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => row.isActive ? (
        <span className="inline-flex items-center gap-1 text-xs text-emerald-600">
          <CheckCircle2 className="h-3.5 w-3.5" /> Active
        </span>
      ) : (
        <span className="inline-flex items-center gap-1 text-xs text-red-500">
          <AlertTriangle className="h-3.5 w-3.5" /> Inactive
        </span>
      ),
    },
    {
      key: 'actions' as keyof UserDto,
      header: '',
      render: (row) => row.id === myId ? null : (
        <div className="flex items-center gap-1 justify-end">
          <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-blue-600"
            title="Edit user" onClick={() => openEdit(row)}>
            <Pencil className="h-3.5 w-3.5" />
          </Button>
          {row.isActive && (
            <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-red-500"
              title="Deactivate user" onClick={() => setDeactivateTarget({ id: row.id, name: row.fullName })}>
              <UserX className="h-3.5 w-3.5" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Users className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">User Management</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">
              Assign roles and departments. Create users in Keycloak admin console.
            </p>
          </div>
        </div>
      </div>

      <DataTable
        data={users}
        columns={columns}
        loading={isLoading}
        rowKey="id"
        emptyMessage="No users found. Users are synced automatically from Keycloak on first login."
      />

      {/* Edit User Modal */}
      <Dialog open={!!editState} onOpenChange={(v) => { if (!v) setEditState(null); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Edit User</DialogTitle>
          </DialogHeader>
          {editState && (
            <form onSubmit={handleEditSubmit} className="space-y-4 mt-2">
              <div>
                <label className="block text-sm font-medium mb-1">Role <span className="text-red-500">*</span></label>
                <select
                  required
                  className={inputCls}
                  value={editState.form.role}
                  onChange={(e) => setEditState(s => s ? { ...s, form: { ...s.form, role: e.target.value } } : s)}
                >
                  {INTERNAL_ROLES.map(r => (
                    <option key={r} value={r}>{r.replace('_', ' ')}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Department</label>
                <select
                  className={inputCls}
                  value={editState.form.departmentId ?? ''}
                  onChange={(e) => setEditState(s => s ? {
                    ...s, form: { ...s.form, departmentId: e.target.value || null }
                  } : s)}
                >
                  <option value="">— No department —</option>
                  {departments.filter((d: DepartmentDto) => d.isActive).map((d: DepartmentDto) => (
                    <option key={d.id} value={d.id}>{d.name}</option>
                  ))}
                </select>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="userActive"
                  checked={editState.form.isActive}
                  onChange={(e) => setEditState(s => s ? { ...s, form: { ...s.form, isActive: e.target.checked } } : s)}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <label htmlFor="userActive" className="text-sm font-medium">Active</label>
              </div>
              <div className="flex justify-end gap-2 pt-1">
                <Button type="button" variant="outline" onClick={() => setEditState(null)}>Cancel</Button>
                <Button type="submit" disabled={updateMut.isPending}>
                  {updateMut.isPending ? 'Saving…' : 'Save'}
                </Button>
              </div>
            </form>
          )}
        </DialogContent>
      </Dialog>

      {/* Deactivate Confirm */}
      {deactivateTarget && (
        <Dialog open onOpenChange={(v) => { if (!v) setDeactivateTarget(null); }}>
          <DialogContent className="max-w-sm">
            <DialogHeader>
              <DialogTitle>Deactivate User</DialogTitle>
            </DialogHeader>
            <p className="text-sm text-slate-600 mt-2">
              Are you sure you want to deactivate <strong>{deactivateTarget.name}</strong>?
              They will lose access to the system.
            </p>
            <div className="flex justify-end gap-2 mt-4">
              <Button variant="outline" onClick={() => setDeactivateTarget(null)}>Cancel</Button>
              <Button
                variant="destructive"
                disabled={deactivateMut.isPending}
                onClick={() => deactivateMut.mutate(deactivateTarget.id)}
              >
                {deactivateMut.isPending ? 'Deactivating…' : 'Deactivate'}
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}
