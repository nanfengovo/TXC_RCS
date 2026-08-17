import { computed, onBeforeUnmount, ref } from 'vue';
import { fetchGetTaskList } from '@/service/api';
import { formatTaskTime } from '@/views/task/modules/task-status';

const POLL_MS = 10000;
const PAGE_SIZE = 200;
const RECENT_LIMIT = 8;

export function useHomeDashboard() {
  const loading = ref(false);
  const tasks = ref<Api.Task.TaskItem[]>([]);
  const totalCount = ref(0);
  const lastRefresh = ref('-');
  let timer: ReturnType<typeof setInterval> | null = null;
  let inFlight = false;

  function countBy(status: string) {
    return tasks.value.filter(item => item.lifecycleStatus === status).length;
  }

  const metrics = computed(() => ({
    total: totalCount.value,
    running: countBy('Running'),
    pending: countBy('Pending'),
    failed: countBy('Failed'),
    succeeded: countBy('Succeeded'),
    canceled: countBy('Canceled'),
    mes: tasks.value.filter(item => item.source === 'Mes').length,
    manual: tasks.value.filter(item => item.source === 'Manual').length
  }));

  const statusSeries = computed(() => [
    { key: 'Pending', value: metrics.value.pending },
    { key: 'Running', value: metrics.value.running },
    { key: 'Succeeded', value: metrics.value.succeeded },
    { key: 'Failed', value: metrics.value.failed },
    { key: 'Canceled', value: metrics.value.canceled }
  ]);

  const sourceSeries = computed(() => [
    { key: 'Mes', value: metrics.value.mes },
    { key: 'Manual', value: metrics.value.manual }
  ]);

  const recentTasks = computed(() => tasks.value.slice(0, RECENT_LIMIT));

  const failedTasks = computed(() =>
    tasks.value.filter(item => item.lifecycleStatus === 'Failed').slice(0, RECENT_LIMIT)
  );

  async function load() {
    if (inFlight) return;
    inFlight = true;
    loading.value = true;

    try {
      const { data, error } = await fetchGetTaskList({ page: 1, pageSize: PAGE_SIZE });
      if (!error && data) {
        tasks.value = data.items ?? [];
        totalCount.value = data.totalCount ?? tasks.value.length;
        lastRefresh.value = formatTaskTime(new Date().toISOString());
      }
    } finally {
      loading.value = false;
      inFlight = false;
    }
  }

  function start() {
    void load();
    if (timer) return;
    timer = setInterval(() => {
      if (document.hidden) return;
      void load();
    }, POLL_MS);
  }

  function stop() {
    if (!timer) return;
    clearInterval(timer);
    timer = null;
  }

  onBeforeUnmount(stop);

  return {
    loading,
    lastRefresh,
    metrics,
    statusSeries,
    sourceSeries,
    recentTasks,
    failedTasks,
    load,
    start,
    stop
  };
}
