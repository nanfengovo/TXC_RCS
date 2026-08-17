<script setup lang="ts">
import { watch } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useEcharts } from '@/hooks/common/echarts';
import { getLifecycleMeta, getSourceMeta } from '@/views/task/modules/task-status';

defineOptions({
  name: 'StatusChart'
});

interface SeriesItem {
  key: string;
  value: number;
}

interface Props {
  title: string;
  series: SeriesItem[];
  kind: 'lifecycle' | 'source';
}

const props = defineProps<Props>();

const appStore = useAppStore();

const lifecycleColors: Record<string, string> = {
  Pending: '#fcbc25',
  Running: '#8e9dff',
  Succeeded: '#26deca',
  Failed: '#ec4786',
  Canceled: '#94a3b8'
};

const sourceColors: Record<string, string> = {
  Mes: '#56cdf3',
  Manual: '#865ec0'
};

function toChartData(series: SeriesItem[]) {
  return series.map(item => ({
    name: props.kind === 'lifecycle' ? getLifecycleMeta(item.key).label : getSourceMeta(item.key).label,
    value: item.value,
    itemStyle: {
      color: props.kind === 'lifecycle' ? lifecycleColors[item.key] : sourceColors[item.key]
    }
  }));
}

const { domRef, updateOptions } = useEcharts(() => ({
  tooltip: {
    trigger: 'item'
  },
  legend: {
    bottom: '1%',
    left: 'center',
    itemStyle: {
      borderWidth: 0
    }
  },
  series: [
    {
      name: props.title,
      type: 'pie',
      radius: ['45%', '75%'],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 10,
        borderColor: '#fff',
        borderWidth: 1
      },
      label: {
        show: false,
        position: 'center'
      },
      emphasis: {
        label: {
          show: true,
          fontSize: '12'
        }
      },
      labelLine: {
        show: false
      },
      data: [] as { name: string; value: number }[]
    }
  ]
}));

function syncChart() {
  updateOptions(opts => {
    opts.series[0].name = props.title;
    opts.series[0].data = toChartData(props.series);
    return opts;
  });
}

watch(
  () => [props.series, props.title, appStore.locale],
  () => {
    syncChart();
  },
  { deep: true, immediate: true }
);
</script>

<template>
  <NCard :title="title" :bordered="false" size="small" class="card-wrapper">
    <div ref="domRef" class="h-320px overflow-hidden"></div>
  </NCard>
</template>

<style scoped></style>
