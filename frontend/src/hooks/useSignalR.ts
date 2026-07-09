import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { keycloak } from '@/shared/lib/keycloak';

const silentLogger: signalR.ILogger = {
  log(level: signalR.LogLevel, message: string) {
    if (message.includes('stopped during negotiation')) return;
    if (level >= signalR.LogLevel.Warning) console.warn('[SignalR]', message);
  },
};

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
      .configureLogging(silentLogger)
      .build();

    connection.on('ReceiveNotification', (payload: NotificationPayload) => {
      onNotification(payload);
    });

    let cancelled = false;

    connection.start().then(() => {
      if (cancelled) connection.stop();
    }).catch((err) => {
      if (!cancelled) console.error('[SignalR]', err);
    });

    connectionRef.current = connection;

    return () => {
      cancelled = true;
      connection.stop();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return connectionRef;
}
