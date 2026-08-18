import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { storeToRefs } from 'pinia';
import { useRcsConfigStore } from '@/store/modules/rcs-config';
import { formatTaskTime } from '@/views/task/modules/task-status';
import {
  computeDashboardStats,
  fetchDashboardTasks,
  type DashboardTimeWindow
} from './dashboard-metrics';

export function useDashboardStats() {
  const rcsConfigStore = useRcsConfigStore();
  const { config } = storeToRefs(rcsConfigStore);

  const loading = ref(false);
  const lastRefresh = ref('-');
  const tasks = ref<Api.Task.TaskItem[]>([]);
  const totalCount = ref(0);
  let timer: ReturnType<typeof setInterval> | null = null;
  let inFlight = false;

  const timeWindow = computed(() => config.value.dashboardTimeWindow);

  const stats = computed(() => computeDashboardStats(tasks.value, totalCount.value, timeWindow.value));

  const liveMetrics = computed(() => ({
    running: stats.value.lifecycleCounts.Running ?? 0,
    pending: stats.value.lifecycleCounts.Pending ?? 0,
    failed: stats.value.lifecycleCounts.Failed ?? 0,
    succeeded: stats.value.lifecycleCounts.Succeeded ?? 0
  }));

  async function load() {
    if (inFlight) return;
    inFlight = true;
    loading.value = true;
    try {
      const result = await fetchDashboardTasks(500);
      tasks.value = result.tasks;
      totalCount.value = result.totalCount;
      lastRefresh.value = formatTaskTime(new Date().toISOString());
    } finally {
      loading.value = false;
      inFlight = false;
    }
  }

  function setTimeWindow(window: DashboardTimeWindow) {
    rcsConfigStore.update({ dashboardTimeWindow: window });
  }

  function startPolling() {
    void load();
    if (timer) return;
    timer = setInterval(() => {
      if (document.hidden) return;
      void load();
    }, config.value.dashboardPollMs);
  }

  function stopPolling() {
    if (!timer) return;
    clearInterval(timer);
    timer = null;
  }

  watch(
    () => config.value.dashboardPollMs,
    () => {
      stopPolling();
      startPolling();
    }
  );

  onBeforeUnmount(stopPolling);

  return {
    loading,
    lastRefresh,
    timeWindow,
    processArea: computed(() => config.value.processArea),
    stats,
    liveMetrics,
    load,
    setTimeWindow,
    startPolling,
    stopPolling
  };
}
