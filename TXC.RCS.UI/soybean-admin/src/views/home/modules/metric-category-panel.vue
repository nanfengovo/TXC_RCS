<script setup lang="ts">
import { computed } from 'vue';
import {
  CATEGORY_LABELS,
  DASHBOARD_METRICS,
  type ComputedMetricValue,
  type DashboardMetricDef
} from './dashboard-metrics';

defineOptions({ name: 'MetricCategoryPanel' });

interface Props {
  category: DashboardMetricDef['category'];
  metrics: Record<string, ComputedMetricValue>;
}

const props = defineProps<Props>();

const items = computed(() => DASHBOARD_METRICS.filter(m => m.category === props.category));

function sourceTag(source: ComputedMetricValue['dataSource']) {
  if (source === 'live') return { type: 'success' as const, label: '实时' };
  if (source === 'partial') return { type: 'warning' as const, label: '近似' };
  return { type: 'default' as const, label: '待接入' };
}
</script>

<template>
  <NCard :title="CATEGORY_LABELS[category]" :bordered="false" size="small" class="card-wrapper metric-panel">
    <NGrid cols="1 s:2 m:3 l:4" responsive="screen" :x-gap="12" :y-gap="12">
      <NGi v-for="def in items" :key="def.id">
        <div class="metric-card">
          <div class="metric-card__head">
            <NTag size="tiny" :bordered="false">{{ def.priority }}</NTag>
            <NTag size="tiny" :type="sourceTag(metrics[def.id]?.dataSource ?? def.dataSource).type">
              {{ sourceTag(metrics[def.id]?.dataSource ?? def.dataSource).label }}
            </NTag>
          </div>
          <div class="metric-card__title" :title="def.title">{{ def.title }}</div>
          <div class="metric-card__value">{{ metrics[def.id]?.display ?? '—' }}</div>
          <div v-if="def.note" class="metric-card__note">{{ def.note }}</div>
        </div>
      </NGi>
    </NGrid>
  </NCard>
</template>

<style scoped>
.metric-card {
  height: 100%;
  padding: 12px;
  border-radius: 10px;
  border: 1px solid var(--metric-stroke, rgb(255 255 255 / 8%));
  background: var(--metric-surface, rgb(255 255 255 / 3%));
  transition: border-color 0.2s ease;
}

.metric-card:hover {
  border-color: rgb(var(--primary-color) / 35%);
}

.metric-card__head {
  display: flex;
  gap: 6px;
  margin-bottom: 8px;
}

.metric-card__title {
  font-size: 13px;
  line-height: 1.4;
  opacity: 0.85;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  min-height: 36px;
}

.metric-card__value {
  margin-top: 8px;
  font-size: 20px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.02em;
}

.metric-card__note {
  margin-top: 6px;
  font-size: 11px;
  line-height: 1.35;
  opacity: 0.5;
}
</style>
