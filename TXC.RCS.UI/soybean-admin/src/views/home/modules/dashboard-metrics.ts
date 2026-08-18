import dayjs from 'dayjs';
import { fetchGetTaskList } from '@/service/api';

export type DashboardTimeWindow = '12h' | '24h' | '72h';

export type MetricDataSource = 'live' | 'partial' | 'pending';

export interface DashboardMetricDef {
  id: string;
  title: string;
  category: 'task' | 'dispatch' | 'device' | 'efficiency';
  priority: 'P0' | 'P1';
  dataSource: MetricDataSource;
  note?: string;
}

/** 按 Excel《Dashboard_数据统计》整理的指标定义 */
export const DASHBOARD_METRICS: DashboardMetricDef[] = [
  {
    id: 'task_duration',
    title: '单任务完成时长（均/上/下）',
    category: 'task',
    priority: 'P0',
    dataSource: 'partial',
    note: '以订单开始/结束时间计算，当前用任务创建与完成时间近似'
  },
  {
    id: 'arm_duration',
    title: '机械臂动作时长（均/上/下）',
    category: 'task',
    priority: 'P0',
    dataSource: 'pending',
    note: '需调度放行→完成时间，待统一接口'
  },
  {
    id: 'agv_motion',
    title: 'AGV 运动时长（均/上/下）',
    category: 'task',
    priority: 'P0',
    dataSource: 'partial',
    note: '以调度开始与到达时间为准，当前近似'
  },
  {
    id: 'reposition',
    title: '二次定位含拍照时长',
    category: 'task',
    priority: 'P0',
    dataSource: 'pending',
    note: '计算方法未统一'
  },
  {
    id: 'area_throughput',
    title: '工艺区域任务总量',
    category: 'task',
    priority: 'P0',
    dataSource: 'live'
  },
  {
    id: 'slot_util',
    title: '储位平均利用率',
    category: 'task',
    priority: 'P0',
    dataSource: 'pending',
    note: '需调度提供储位数据'
  },
  {
    id: 'standby',
    title: 'AMR 待机时长',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending',
    note: '需调度回原点后等待时间'
  },
  {
    id: 'work_duration',
    title: 'AMR 工作时长',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'partial'
  },
  {
    id: 'utilization_avg',
    title: '整场平均稼动率',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'utilization_unit',
    title: '单台 AMR 稼动率',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending',
    note: '需在线时长与充电时长'
  },
  {
    id: 'charge_duration',
    title: '单台充电时长',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'charge_ratio',
    title: '整场充放比',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'move_duration',
    title: '单台移动时长',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'collab_duration',
    title: '单台协作时长',
    category: 'dispatch',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'battery_soc',
    title: 'SOC / 电压 / 电流',
    category: 'device',
    priority: 'P1',
    dataSource: 'pending',
    note: '电池管理软件'
  },
  {
    id: 'battery_soh',
    title: '电池健康度 SOH',
    category: 'device',
    priority: 'P1',
    dataSource: 'pending'
  },
  {
    id: 'mileage',
    title: '单台里程',
    category: 'device',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'alarm_duration',
    title: '故障报警时长',
    category: 'device',
    priority: 'P0',
    dataSource: 'partial',
    note: 'RCS 记录报警起止，当前以失败任务近似'
  },
  {
    id: 'empty_ratio',
    title: '空占比统计',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'mtbf',
    title: 'MTBF',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'mttr',
    title: 'MTTR',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'mtba',
    title: 'MTBA',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'mtta',
    title: 'MTTA',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  },
  {
    id: 'jam_rate',
    title: 'Cobot Pick & Place Jam Rate',
    category: 'efficiency',
    priority: 'P0',
    dataSource: 'pending'
  }
];

export const CATEGORY_LABELS: Record<DashboardMetricDef['category'], string> = {
  task: '任务统计',
  dispatch: '调度 / 规划',
  device: '设备',
  efficiency: '项目效率 / MCS'
};

function windowHours(w: DashboardTimeWindow) {
  return w === '12h' ? 12 : w === '24h' ? 24 : 72;
}

function inWindow(iso: string | null | undefined, hours: number) {
  if (!iso) return false;
  return dayjs(iso).isAfter(dayjs().subtract(hours, 'hour'));
}

function taskDurationMinutes(task: Api.Task.TaskItem) {
  const end = task.lastModificationTime || task.creationTime;
  const ms = dayjs(end).diff(dayjs(task.creationTime));
  return Math.max(0, ms / 60000);
}

function durationStats(minutes: number[]) {
  if (!minutes.length) return { avg: 0, min: 0, max: 0, count: 0 };
  const sum = minutes.reduce((a, b) => a + b, 0);
  return {
    avg: Math.round(sum / minutes.length),
    min: Math.round(Math.min(...minutes)),
    max: Math.round(Math.max(...minutes)),
    count: minutes.length
  };
}

function formatDurationStats(stats: ReturnType<typeof durationStats>) {
  if (!stats.count) return '—';
  return `${stats.avg} / ${stats.max} / ${stats.min} min`;
}

export interface ComputedMetricValue {
  id: string;
  display: string;
  dataSource: MetricDataSource;
}

export interface DashboardStats {
  tasks: Api.Task.TaskItem[];
  totalCount: number;
  windowTasks: Api.Task.TaskItem[];
  agvSeries: { name: string; value: number }[];
  throughputSeries: { name: string; value: number }[];
  lifecycleCounts: Record<string, number>;
  metrics: Record<string, ComputedMetricValue>;
}

export function computeDashboardStats(
  tasks: Api.Task.TaskItem[],
  totalCount: number,
  timeWindow: DashboardTimeWindow
): DashboardStats {
  const hours = windowHours(timeWindow);
  const windowTasks = tasks.filter(t => inWindow(t.creationTime, hours));

  const succeeded = windowTasks.filter(t => t.lifecycleStatus === 'Succeeded');
  const durations = succeeded.map(taskDurationMinutes);
  const durationStat = durationStats(durations);

  const agvMap = new Map<string, number>();
  const agvWorkMinutes = new Map<string, number>();
  for (const t of windowTasks) {
    if (t.agvSerial) {
      agvMap.set(t.agvSerial, (agvMap.get(t.agvSerial) ?? 0) + 1);
      if (t.lifecycleStatus === 'Succeeded' || t.lifecycleStatus === 'Running') {
        agvWorkMinutes.set(t.agvSerial, (agvWorkMinutes.get(t.agvSerial) ?? 0) + taskDurationMinutes(t));
      }
    }
  }

  const agvSeries = [...agvMap.entries()]
    .map(([name, value]) => ({ name, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 12);

  const failedInWindow = windowTasks.filter(t => t.lifecycleStatus === 'Failed');
  const alarmMinutes = failedInWindow.map(taskDurationMinutes);
  const alarmStat = durationStats(alarmMinutes);

  const totalWorkMinutes = [...agvWorkMinutes.values()].reduce((a, b) => a + b, 0);

  const lifecycleCounts: Record<string, number> = {};
  for (const t of windowTasks) {
    lifecycleCounts[t.lifecycleStatus] = (lifecycleCounts[t.lifecycleStatus] ?? 0) + 1;
  }

  const throughputSeries = ['12h', '24h', '72h'].map(label => {
    const h = windowHours(label as DashboardTimeWindow);
    return {
      name: label,
      value: tasks.filter(t => inWindow(t.creationTime, h)).length
    };
  });

  const metrics: Record<string, ComputedMetricValue> = {
    task_duration: {
      id: 'task_duration',
      display: formatDurationStats(durationStat),
      dataSource: durationStat.count ? 'partial' : 'pending'
    },
    arm_duration: { id: 'arm_duration', display: '待接入', dataSource: 'pending' },
    agv_motion: {
      id: 'agv_motion',
      display: formatDurationStats(durationStat),
      dataSource: durationStat.count ? 'partial' : 'pending'
    },
    reposition: { id: 'reposition', display: '待接入', dataSource: 'pending' },
    area_throughput: {
      id: 'area_throughput',
      display: String(windowTasks.length),
      dataSource: 'live'
    },
    slot_util: { id: 'slot_util', display: '待接入', dataSource: 'pending' },
    standby: { id: 'standby', display: '待接入', dataSource: 'pending' },
    work_duration: {
      id: 'work_duration',
      display: totalWorkMinutes ? `${Math.round(totalWorkMinutes)} min` : '—',
      dataSource: totalWorkMinutes ? 'partial' : 'pending'
    },
    utilization_avg: { id: 'utilization_avg', display: '待接入', dataSource: 'pending' },
    utilization_unit: { id: 'utilization_unit', display: '待接入', dataSource: 'pending' },
    charge_duration: { id: 'charge_duration', display: '待接入', dataSource: 'pending' },
    charge_ratio: { id: 'charge_ratio', display: '待接入', dataSource: 'pending' },
    move_duration: { id: 'move_duration', display: '待接入', dataSource: 'pending' },
    collab_duration: { id: 'collab_duration', display: '待接入', dataSource: 'pending' },
    battery_soc: { id: 'battery_soc', display: '待接入', dataSource: 'pending' },
    battery_soh: { id: 'battery_soh', display: '待接入', dataSource: 'pending' },
    mileage: { id: 'mileage', display: '待接入', dataSource: 'pending' },
    alarm_duration: {
      id: 'alarm_duration',
      display: formatDurationStats(alarmStat),
      dataSource: alarmStat.count ? 'partial' : 'pending'
    },
    empty_ratio: { id: 'empty_ratio', display: '待接入', dataSource: 'pending' },
    mtbf: { id: 'mtbf', display: '待接入', dataSource: 'pending' },
    mttr: { id: 'mttr', display: '待接入', dataSource: 'pending' },
    mtba: { id: 'mtba', display: '待接入', dataSource: 'pending' },
    mtta: { id: 'mtta', display: '待接入', dataSource: 'pending' },
    jam_rate: { id: 'jam_rate', display: '待接入', dataSource: 'pending' }
  };

  return {
    tasks,
    totalCount,
    windowTasks,
    agvSeries,
    throughputSeries,
    lifecycleCounts,
    metrics
  };
}

export async function fetchDashboardTasks(pageSize = 500) {
  const { data, error } = await fetchGetTaskList({ page: 1, pageSize });
  if (error || !data) {
    return { tasks: [] as Api.Task.TaskItem[], totalCount: 0 };
  }
  return {
    tasks: data.items ?? [],
    totalCount: data.totalCount ?? 0
  };
}
