import { request } from '../request';

const { VITE_CLIENT_ID, VITE_SCOPE } = import.meta.env;
const AUTH_TIMEOUT = 60 * 1000;

/**
 * Login via OpenIddict password grant
 *
 * @param userName User name
 * @param password Password
 */
export function fetchLogin(userName: string, password: string) {
  const params = new URLSearchParams();
  params.append('grant_type', 'password');
  params.append('username', userName);
  params.append('password', password);
  params.append('client_id', VITE_CLIENT_ID);
  params.append('scope', `${VITE_SCOPE} offline_access`);

  return request<Api.Auth.LoginToken>({
    url: '/connect/token',
    method: 'post',
    timeout: AUTH_TIMEOUT,
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    data: params
  });
}

/**
 * Refresh token via OpenIddict refresh_token grant
 *
 * @param refreshToken Refresh token
 */
export function fetchRefreshToken(refreshToken: string) {
  const params = new URLSearchParams();
  params.append('grant_type', 'refresh_token');
  params.append('refresh_token', refreshToken);
  params.append('client_id', VITE_CLIENT_ID);
  params.append('scope', `${VITE_SCOPE} offline_access`);

  return request<Api.Auth.LoginToken>({
    url: '/connect/token',
    method: 'post',
    timeout: AUTH_TIMEOUT,
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    data: params
  });
}

/** Get current ABP application configuration (user + granted policies) */
export function fetchGetCurrentUser() {
  return request<Api.Auth.ApplicationConfiguration>({
    url: '/api/abp/application-configuration'
  });
}
