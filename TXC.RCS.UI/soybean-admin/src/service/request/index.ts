import type { AxiosResponse } from 'axios';
import { BACKEND_ERROR_CODE, createFlatRequest, createRequest } from '@sa/axios';
import { useAuthStore } from '@/store/modules/auth';
import { localStg } from '@/utils/storage';
import { getServiceBaseURL } from '@/utils/service';
import { $t } from '@/locales';
import { getAuthorization, handleExpiredRequest, showErrorMsg } from './shared';
import type { RequestInstanceState } from './type';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL, otherBaseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

function getAbpErrorMessage(data: any, fallback: string) {
  // OpenIddict: { error, error_description }
  if (typeof data?.error === 'string') {
    return data.error_description || data.error || fallback;
  }

  return (
    data?.error?.message ||
    data?.error?.details ||
    data?.message ||
    data?.msg ||
    fallback
  );
}

export const request = createFlatRequest(
  {
    baseURL,
    // 401 进入拦截器以便刷新 token
    validateStatus: (status: number) => status >= 200 && status < 500
  },
  {
    defaultState: {
      errMsgStack: [],
      refreshTokenPromise: null
    } as RequestInstanceState,
    transform(response: AxiosResponse<App.Service.Response<any>>) {
      // ABP 约定控制器常直接返回 DTO；部分包装在 data
      return (response.data as any)?.data ?? response.data;
    },
    async onRequest(config) {
      const Authorization = getAuthorization();
      Object.assign(config.headers, { Authorization });
      return config;
    },
    isBackendSuccess(response) {
      return response.status >= 200 && response.status < 300;
    },
    async onBackendFail(response, instance) {
      const authStore = useAuthStore();

      if (response.status === 401) {
        const refreshToken = localStg.get('refreshToken');
        if (refreshToken) {
          const success = await handleExpiredRequest(request.state);
          if (success) {
            const Authorization = getAuthorization();
            Object.assign(response.config.headers, { Authorization });
            return instance.request(response.config) as Promise<AxiosResponse>;
          }
        }
        authStore.resetStore();
        return null;
      }

      if (response.status >= 400 && response.status < 500) {
        showErrorMsg(request.state, getAbpErrorMessage(response.data, $t('common.error')));
        return null;
      }

      return null;
    },
    onError(error) {
      let message = error.message;

      if (error.code === BACKEND_ERROR_CODE) {
        message = getAbpErrorMessage(error.response?.data, message);
      }

      if (error.response?.status === 401) {
        return;
      }

      showErrorMsg(request.state, message);
    }
  }
);

export const demoRequest = createRequest(
  {
    baseURL: otherBaseURL.demo
  },
  {
    transform(response: AxiosResponse<App.Service.DemoResponse>) {
      return response.data.result;
    },
    async onRequest(config) {
      const { headers } = config;
      const token = localStg.get('token');
      const Authorization = token ? `Bearer ${token}` : null;
      Object.assign(headers, { Authorization });
      return config;
    },
    isBackendSuccess(response) {
      return response.data.status === '200';
    },
    async onBackendFail(_response) {},
    onError(error) {
      let message = error.message;
      if (error.code === BACKEND_ERROR_CODE) {
        message = error.response?.data?.message || message;
      }
      window.$message?.error(message);
    }
  }
);
