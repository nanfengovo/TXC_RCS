<script setup lang="ts">
import { watch } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useEcharts } from '@/hooks/common/echarts';

defineOptions({ name: 'ThroughputChart' });

interface Props {
  series: { name: string; value: number }[];
}

const props = defineProps<Props>();

const appStore = useAppStore();

const { domRef, updateOptions } = useEcharts(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '3%', top: '12%', containLabel: true },
  xAxis: { type: 'category', data: [] as string[] },
  yAxis: { type: 'value', name: '任务量', minInterval: 1 },
  series: [
    {
      type: 'line',
      smooth: true,
      areaStyle: { opacity: 0.15 },
      data: [] as number[],
      itemStyle: { color: '#8e9dff' },
      lineStyle: { width: 2 }
    }
  ]
}));

function sync() {
  updateOptions(opts => {
    opts.xAxis.data = props.series.map(s => s.name);
    opts.series[0].data = props.series.map(s => s.value);
    return opts;
  });
}

watch(() => [props.series, appStore.locale], sync, { deep: true, immediate: true });
</script>

<template>
  <NCard :title="$t('page.home.throughput')" :bordered="false" size="small" class="card-wrapper">
    <div ref="domRef" class="h-280px overflow-hidden" />
  </NCard>
</template>
