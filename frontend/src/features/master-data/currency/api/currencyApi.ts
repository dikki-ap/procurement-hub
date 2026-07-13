import { apiClient } from '@/shared/lib/axios';

export interface CurrencyDto {
  id: string;
  code: string;
  name: string;
  symbol: string | null;
  exchangeRate: number;
  isBase: boolean;
  isActive: boolean;
  rateUpdatedAt: string | null;
}

export interface CreateCurrencyRequest {
  code: string;
  name: string;
  symbol?: string;
  exchangeRate: number;
  isBase: boolean;
}

export interface UpdateCurrencyRequest extends CreateCurrencyRequest {
  isActive: boolean;
}

const BASE = '/master-data/currencies';

export const currencyApi = {
  getAll: () => apiClient.get<{ data: CurrencyDto[] }>(BASE).then((r) => r.data.data),
  getById: (id: string) =>
    apiClient.get<{ data: CurrencyDto }>(`${BASE}/${id}`).then((r) => r.data.data),
  create: (data: CreateCurrencyRequest) =>
    apiClient.post<{ data: { id: string } }>(BASE, data).then((r) => r.data.data.id),
  update: (id: string, data: UpdateCurrencyRequest) =>
    apiClient.put(`${BASE}/${id}`, { id, ...data }),
  delete: (id: string) => apiClient.delete(`${BASE}/${id}`),
  syncRates: () => apiClient.post(`${BASE}/fetch-rates`),
  getExchangeRateSettings: () =>
    apiClient.get<{ data: { autoSync: boolean } }>(`${BASE}/exchange-rate-settings`).then((r) => r.data.data),
  updateExchangeRateSettings: (autoSync: boolean) =>
    apiClient.put(`${BASE}/exchange-rate-settings`, { autoSync }),
};
