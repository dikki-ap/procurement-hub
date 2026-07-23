import { apiClient } from './axios';

export async function downloadExcel(url: string, filename: string): Promise<void> {
  const response = await apiClient.get(url, { responseType: 'blob' });
  const blob = new Blob([response.data as BlobPart], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(link.href);
}
