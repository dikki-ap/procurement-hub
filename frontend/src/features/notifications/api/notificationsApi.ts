import { apiClient } from '@/shared/lib/axios';

export interface NotificationDto {
  id:        string;
  title:     string;
  message:   string;
  link:      string | null;
  isRead:    boolean;
  createdAt: string;
  readAt:    string | null;
}

export const notificationsApi = {
  getMyNotifications: () =>
    apiClient.get<{ data: NotificationDto[] }>('/notifications'),

  markAsRead: (id: string) =>
    apiClient.post(`/notifications/${id}/read`),
};
