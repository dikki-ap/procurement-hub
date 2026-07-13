/** Parses a UTC datetime string safely (appends Z if missing so JS treats it as UTC). */
export const parseUtc = (value: string): Date =>
  new Date(value.endsWith('Z') ? value : value + 'Z');

/** Date only — respects user's browser locale and local timezone. */
export const fmtDate = (value: string | null | undefined): string => {
  if (!value) return '—';
  return parseUtc(value).toLocaleDateString(undefined, {
    day: '2-digit', month: 'short', year: 'numeric',
  });
};

/** Date + time (no seconds) — respects user's browser locale and local timezone. */
export const fmtDateTime = (value: string | null | undefined): string => {
  if (!value) return '—';
  return parseUtc(value).toLocaleString(undefined, {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

/** Date + time + seconds — respects user's browser locale and local timezone. */
export const fmtDateTimeSec = (value: string | null | undefined): string => {
  if (!value) return '—';
  return parseUtc(value).toLocaleString(undefined, {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  });
};
