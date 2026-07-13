import { fmtDateTime } from '@/shared/lib/date';

interface Props {
  name?: string | null;
  at?: string | null;
}

export function AuditCell({ name, at }: Props) {
  return (
    <div className="text-sm leading-snug">
      <div className="font-medium">{name ?? '—'}</div>
      <div className="text-xs text-muted-foreground">{at ? fmtDateTime(at) : '—'}</div>
    </div>
  );
}
