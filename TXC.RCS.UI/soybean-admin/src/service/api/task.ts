import { request } from '../request';

function toAbpQuery(params: {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
  source?: string | null;
  lifecycleStatus?: string | null;
  fromAddress?: string | null;
  toAddress?: string | null;
  containerId?: string | null;
  lotId?: string | null;
}) {
  const page = params.page ?? 1;
  const pageSize = params.pageSize ?? 10;
  return {
    SkipCount: (page - 1) * pageSize,
    MaxResultCount: pageSize,
    Sorting: 'CreationTime desc',
    Keyword: params.keyword || undefined,
    Source: params.source || undefined,
    LifecycleStatus: params.lifecycleStatus || undefined,
    FromAddress: params.fromAddress || undefined,
    ToAddress: params.toAddress || undefined,
    ContainerId: params.containerId || undefined,
    LotId: params.lotId || undefined
  };
}

/** GET /api/app/task */
export function fetchGetTaskList(params: Parameters<typeof toAbpQuery>[0]) {
  return request<Api.Task.TaskList>({
    url: '/api/app/task',
    method: 'get',
    params: toAbpQuery(params)
  });
}

export function fetchGetTask(id: string) {
  return request<Api.Task.TaskItem>({
    url: `/api/app/task/${encodeURIComponent(id)}`,
    method: 'get'
  });
}

export function fetchGetTaskMonitorDetail(id: string) {
  return request<Api.Task.MonitorDetail>({
    url: `/api/app/task/${encodeURIComponent(id)}/monitor-detail`,
    method: 'get'
  });
}

export function fetchCreateManualTask(data: Api.Task.CreateManualTask) {
  return request<Api.Task.TaskItem>({
    url: '/api/app/task/manual',
    method: 'post',
    data
  });
}

export function fetchCancelTask(data: Api.Task.CancelTask) {
  return request<Api.Task.TaskItem>({
    url: '/api/app/task/cancel',
    method: 'post',
    data
  });
}

export function fetchDeleteTask(id: string) {
  return request({
    url: `/api/app/task/${encodeURIComponent(id)}`,
    method: 'delete'
  });
}

export function fetchRetryMesReport(id: string) {
  return request<Api.Task.MesReportResult>({
    url: `/api/app/task/${encodeURIComponent(id)}/retry-mes-report`,
    method: 'post'
  });
}

export function fetchGetOptionCodeSchema() {
  return request<Api.Task.PublishedOptionCodeSchema>({
    url: '/api/app/task/option-code-schema',
    method: 'get'
  });
}
