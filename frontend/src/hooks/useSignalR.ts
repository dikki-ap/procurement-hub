import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { keycloak } from '@/shared/lib/keycloak';

type NotificationPayload = {
  id:        string;
  title:     string;
  message:   string;
  link:      string | null;
  createdAt: string;
};

export function useSignalR(onNotification: (n: NotificationPayload) => void) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const hubUrl = (import.meta.env.VITE_API_URL?.replace('/api/v1', '') ?? '') + '/hubs/notifications';

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => {
          if (!keycloak.authenticated) return '';
          await keycloak.updateToken(30).catch(() => keycloak.login());
          return keycloak.token ?? '';
        },
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (payload: NotificationPayload) => {
      onNotification(payload);
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return connectionRef;
}
