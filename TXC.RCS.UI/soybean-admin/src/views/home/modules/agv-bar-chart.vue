<script setup lang="ts">
import { watch } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useEcharts } from '@/hooks/common/echarts';

defineOptions({ name: 'AgvBarChart' });

interface Props {
  series: { name: string; value: number }[];
}

const props = defineProps<Props>();

const appStore = useAppStore();

const { domRef, updateOptions } = useEcharts(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '3%', top: '8%', containLabel: true },
  xAxis: { type: 'category', data: [] as string[], axisLabel: { rotate: 30 } },
  yAxis: { type: 'value', name: '任务数', minInterval: 1 },
  series: [
    {
      type: 'bar',
      data: [] as number[],
      itemStyle: {
        borderRadius: [4, 4, 0, 0],
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: '#2dd4bf' },
            { offset: 1, color: '#0d9488' }
          ]
        }
      }
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
  <NCard :title="$t('page.home.agvTasks')" :bordered="false" size="small" class="card-wrapper">
    <NEmpty v-if="!series.length" :description="$t('page.home.noAgvData')" class="py-40px" />
    <div v-else ref="domRef" class="h-280px overflow-hidden" />
  </NCard>
</template>
