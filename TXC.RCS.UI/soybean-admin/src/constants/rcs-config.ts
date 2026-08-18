/** RCS 前端运行时配置默认值（可在「系统配置」页调整并持久化到 localStorage） */
export interface RcsRuntimeConfig {
  /** 首页仪表盘轮询间隔（毫秒） */
  dashboardPollMs: number;
  /** 任务列表轮询间隔（毫秒） */
  taskListPollMs: number;
  /** 任务监控轮询间隔（毫秒） */
  taskMonitorPollMs: number;
  /** 首页默认统计时间窗 */
  dashboardTimeWindow: '12h' | '24h' | '72h';
  /** 工艺区域标识（仪表盘筛选） */
  processArea: string;
  /** 常用起点地址建议 */
  fromAddressSuggestions: string[];
  /** 常用终点地址建议 */
  toAddressSuggestions: string[];
  /** 下发任务表单默认起点 */
  defaultFromAddress: string;
  /** 下发任务表单默认终点 */
  defaultToAddress: string;
  /** 任务搜索区默认折叠 */
  taskSearchCollapsed: boolean;
  /** 监控抽屉宽度（像素） */
  taskMonitorWidth: number;
  /** 开发/演示默认登录用户名 */
  defaultLoginUser: string;
  /** 开发/演示默认登录密码 */
  defaultLoginPassword: string;
}

export const DEFAULT_RCS_CONFIG: RcsRuntimeConfig = {
  dashboardPollMs: 10000,
  taskListPollMs: 8000,
  taskMonitorPollMs: 5000,
  dashboardTimeWindow: '24h',
  processArea: 'FAB-AMHS',
  fromAddressSuggestions: ['ERACK', 'STK01', 'OHT-BAY01'],
  toAddressSuggestions: ['H044', 'H045', 'EQP-A01', 'EQP-B02'],
  defaultFromAddress: 'ERACK',
  defaultToAddress: 'H044',
  taskSearchCollapsed: true,
  taskMonitorWidth: 480,
  defaultLoginUser: 'admin',
  defaultLoginPassword: '1q2w3E*'
};

export const RCS_CONFIG_STORAGE_KEY = 'rcs-runtime-config';
