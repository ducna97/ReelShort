// Khớp với cổng Development hiện có trong ReelShort.API/Properties/launchSettings.json.
import type { Credentials, RegisterCredentials, User } from './types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5234/api';

interface ApiEnvelope<T> {
  data?: T;
  message?: string;
  errors?: string[];
}

export async function login(credentials: Credentials, signal?: AbortSignal): Promise<User> {
  const response = await fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
    signal,
  });
  const payload: ApiEnvelope<User> = await response.json().catch(() => ({}));
  if (!response.ok)
    throw new Error(payload.message || payload.errors?.[0] || 'Đăng nhập không thành công.');
  return payload.data || (payload as User);
}

export async function register(
  credentials: RegisterCredentials,
  signal?: AbortSignal,
): Promise<User> {
  const response = await fetch(`${API_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
    signal,
  });
  const payload: ApiEnvelope<User> = await response.json().catch(() => ({}));
  if (!response.ok)
    throw new Error(payload.message || payload.errors?.[0] || 'Không thể tạo tài khoản.');
  return payload.data || (payload as User);
}

export async function socialLogin(
  provider: 'google' | 'facebook',
  accessToken: string,
  signal?: AbortSignal,
): Promise<User> {
  const response = await fetch(`${API_URL}/auth/${provider}-login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ accessToken }),
    signal,
  });
  const payload: ApiEnvelope<User> = await response.json().catch(() => ({}));
  if (!response.ok) {
    const providerName = provider === 'google' ? 'Google' : 'Facebook';
    throw new Error(
      payload.message || payload.errors?.[0] || `Không thể đăng nhập bằng ${providerName}.`,
    );
  }
  return payload.data || (payload as User);
}
