<script setup lang="ts">
import { computed } from 'vue';
import { useAuthStore } from '@/store/modules/auth';
import type { DashboardTimeWindow } from './dashboard-metrics';

defineOptions({ name: 'DashboardHeader' });

interface Props {
  processArea: string;
  timeWindow: DashboardTimeWindow;
  running: number;
  pending: number;
  failed: number;
  lastRefresh: string;
  loading: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  refresh: [];
  'update:timeWindow': [DashboardTimeWindow];
}>();

const authStore = useAuthStore();

const windowOptions = [
  { label: '12 小时', value: '12h' as const },
  { label: '24 小时', value: '24h' as const },
  { label: '72 小时', value: '72h' as const }
];

const headline = computed(() => `${props.processArea} · AMHS 数据统计`);
</script>

<template>
  <NCard :bordered="false" size="small" class="dashboard-header card-wrapper">
    <div class="dashboard-header__grid">
      <div class="dashboard-header__intro">
        <SystemLogo class="dashboard-header__logo" />
        <div>
          <h2 class="dashboard-header__title">{{ headline }}</h2>
          <p class="dashboard-header__subtitle">
            {{ $t('page.home.greeting', { userName: authStore.userInfo.userName || 'operator' }) }}
            · {{ $t('page.home.dashboardDesc') }}
          </p>
          <p class="dashboard-header__meta">{{ $t('page.home.lastRefresh') }}：{{ lastRefresh }}</p>
        </div>
      </div>

      <div class="dashboard-header__controls">
        <NStatistic :label="$t('page.home.running')" :value="running" class="dashboard-header__stat" />
        <NStatistic :label="$t('page.home.pending')" :value="pending" class="dashboard-header__stat" />
        <NStatistic :label="$t('page.home.failed')" :value="failed" class="dashboard-header__stat" />
        <NSelect
          :value="timeWindow"
          :options="windowOptions"
          size="small"
          class="dashboard-header__window"
          @update:value="emit('update:timeWindow', $event)"
        />
        <NButton type="primary" size="small" :loading="loading" @click="emit('refresh')">
          {{ $t('page.home.refresh') }}
        </NButton>
      </div>
    </div>
  </NCard>
</template>

<style scoped>
.dashboard-header__grid {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.dashboard-header__intro {
  display: flex;
  align-items: center;
  gap: 14px;
  min-width: 0;
}

.dashboard-header__logo {
  width: 140px;
  height: 36px;
  flex-shrink: 0;
}

.dashboard-header__title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.dashboard-header__subtitle {
  margin: 4px 0 0;
  font-size: 13px;
  opacity: 0.72;
}

.dashboard-header__meta {
  margin: 4px 0 0;
  font-size: 12px;
  opacity: 0.55;
}

.dashboard-header__controls {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  justify-content: flex-end;
}

.dashboard-header__stat {
  min-width: 64px;
}

.dashboard-header__window {
  width: 112px;
}
</style>
