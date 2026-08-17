import dayjs from 'dayjs';

export interface StatusMeta {
  label: string;
  color: NaiveUI.ThemeColor;
}

export const LIFECYCLE_META: Record<string, StatusMeta> = {
  Pending: { label: '待开始', color: 'warning' },
  Running: { label: '运行中', color: 'info' },
  Succeeded: { label: '已完成', color: 'success' },
  Failed: { label: '失败', color: 'error' },
  Canceled: { label: '已取消', color: 'error' }
};

export const SOURCE_META: Record<string, StatusMeta> = {
  Manual: { label: '人工', color: 'default' },
  Mes: { label: 'MES', color: 'primary' }
};

export function getLifecycleMeta(status?: string | null): StatusMeta {
  if (!status) return { label: '-', color: 'default' };
  return LIFECYCLE_META[status] || { label: status, color: 'default' };
}

export function getSourceMeta(source?: string | null): StatusMeta {
  if (!source) return { label: '-', color: 'default' };
  return SOURCE_META[source] || { label: source, color: 'default' };
}

export function formatTaskTime(dateTime?: string | null): string {
  if (!dateTime) return '-';
  const date = dayjs(dateTime);
  if (!date.isValid() || date.year() <= 1901) return '-';
  return date.format('YYYY-MM-DD HH:mm:ss');
}

export function isTaskCancelable(status?: string | null) {
  return status === 'Pending' || status === 'Running';
}

export function isTaskDeletable(status?: string | null) {
  return status === 'Succeeded' || status === 'Failed' || status === 'Canceled';
}

export function canRetryMes(task?: Pick<Api.Task.TaskItem, 'source' | 'lifecycleStatus'> | null) {
  if (!task) return false;
  return task.source === 'Mes' && (task.lifecycleStatus === 'Succeeded' || task.lifecycleStatus === 'Canceled');
}

export function timelineStatusType(status: string): 'default' | 'success' | 'error' | 'warning' | 'info' {
  switch (status) {
    case 'done':
      return 'success';
    case 'current':
      return 'info';
    case 'error':
      return 'error';
    case 'canceled':
      return 'warning';
    default:
      return 'default';
  }
}
