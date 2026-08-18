<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useDashboardStats } from './modules/use-dashboard-stats';
import DashboardHeader from './modules/dashboard-header.vue';
import MetricCategoryPanel from './modules/metric-category-panel.vue';
import AgvBarChart from './modules/agv-bar-chart.vue';
import ThroughputChart from './modules/throughput-chart.vue';
import StatusChart from './modules/status-chart.vue';
import RecentTasks from './modules/recent-tasks.vue';

defineOptions({ name: 'HomePage' });

const appStore = useAppStore();

const {
  loading,
  lastRefresh,
  timeWindow,
  processArea,
  stats,
  liveMetrics,
  load,
  setTimeWindow,
  startPolling
} = useDashboardStats();

startPolling();

const gap = computed(() => (appStore.isMobile ? 0 : 16));

const statusSeries = computed(() => [
  { key: 'Pending', value: stats.value.lifecycleCounts.Pending ?? 0 },
  { key: 'Running', value: stats.value.lifecycleCounts.Running ?? 0 },
  { key: 'Succeeded', value: stats.value.lifecycleCounts.Succeeded ?? 0 },
  { key: 'Failed', value: stats.value.lifecycleCounts.Failed ?? 0 },
  { key: 'Canceled', value: stats.value.lifecycleCounts.Canceled ?? 0 }
]);

const recentTasks = computed(() => stats.value.windowTasks.slice(0, 6));
const failedTasks = computed(() =>
  stats.value.windowTasks.filter(t => t.lifecycleStatus === 'Failed').slice(0, 6)
);

const categories = ['task', 'dispatch', 'device', 'efficiency'] as const;
</script>

<template>
  <NSpace vertical :size="12">
    <DashboardHeader
      :process-area="processArea"
      :time-window="timeWindow"
      :running="liveMetrics.running"
      :pending="liveMetrics.pending"
      :failed="liveMetrics.failed"
      :last-refresh="lastRefresh"
      :loading="loading"
      @refresh="load"
      @update:time-window="setTimeWindow"
    />

    <NGrid :x-gap="gap" :y-gap="12" responsive="screen" item-responsive>
      <NGi span="24 s:24 m:14">
        <ThroughputChart :series="stats.throughputSeries" />
      </NGi>
      <NGi span="24 s:24 m:10">
        <StatusChart :title="$t('page.home.statusDist')" kind="lifecycle" :series="statusSeries" />
      </NGi>
    </NGrid>

    <AgvBarChart :series="stats.agvSeries" />

    <MetricCategoryPanel
      v-for="cat in categories"
      :key="cat"
      :category="cat"
      :metrics="stats.metrics"
    />

    <NGrid :x-gap="gap" :y-gap="12" responsive="screen" item-responsive>
      <NGi span="24 s:24 m:14">
        <RecentTasks
          :title="$t('page.home.recentTasks')"
          :tasks="recentTasks"
          :loading="loading"
          compact
        />
      </NGi>
      <NGi span="24 s:24 m:10">
        <RecentTasks
          :title="$t('page.home.exceptions')"
          :tasks="failedTasks"
          :loading="loading"
          compact
          :empty-text="$t('page.home.noException')"
        />
      </NGi>
    </NGrid>
  </NSpace>
</template>

<style scoped></style>
