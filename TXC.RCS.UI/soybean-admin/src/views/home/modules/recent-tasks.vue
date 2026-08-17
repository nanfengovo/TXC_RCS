<script setup lang="tsx">
import { computed } from 'vue';
import { NTag } from 'naive-ui';
import type { DataTableColumns } from 'naive-ui';
import { useRouterPush } from '@/hooks/common/router';
import { $t } from '@/locales';
import { formatTaskTime, getLifecycleMeta, getSourceMeta } from '@/views/task/modules/task-status';

defineOptions({
  name: 'RecentTasks'
});

interface Props {
  title: string;
  tasks: Api.Task.TaskItem[];
  loading: boolean;
  emptyText?: string;
}

const props = withDefaults(defineProps<Props>(), {
  emptyText: undefined
});

const { routerPushByKey } = useRouterPush();

function formatRoute(row: Api.Task.TaskItem) {
  return `${row.fromAddress}/${row.fromPort || '-'} → ${row.toAddress || '-'}/${row.toPort || '-'}`;
}

const columns = computed<DataTableColumns<Api.Task.TaskItem>>(() => [
  {
    key: 'id',
    title: $t('page.home.taskId'),
    ellipsis: { tooltip: true },
    minWidth: 160
  },
  {
    key: 'source',
    title: $t('page.home.source'),
    width: 90,
    render: row => {
      const meta = getSourceMeta(row.source);
      return <NTag type={meta.color}>{meta.label}</NTag>;
    }
  },
  {
    key: 'lifecycleStatus',
    title: $t('page.home.status'),
    width: 100,
    render: row => {
      const meta = getLifecycleMeta(row.lifecycleStatus);
      return <NTag type={meta.color}>{meta.label}</NTag>;
    }
  },
  {
    key: 'route',
    title: $t('page.home.route'),
    minWidth: 180,
    ellipsis: { tooltip: true },
    render: row => formatRoute(row)
  },
  {
    key: 'waitingEvent',
    title: $t('page.home.waitingEvent'),
    width: 120,
    render: row => row.waitingEvent || '-'
  },
  {
    key: 'creationTime',
    title: $t('page.home.creationTime'),
    width: 170,
    render: row => formatTaskTime(row.creationTime)
  }
]);
</script>

<template>
  <NCard :title="title" :bordered="false" size="small" class="card-wrapper">
    <template #header-extra>
      <NButton text type="primary" @click="routerPushByKey('task')">
        {{ $t('page.home.moreTasks') }}
      </NButton>
    </template>
    <NDataTable
      :columns="columns"
      :data="tasks"
      :loading="loading"
      :pagination="false"
      :row-key="row => row.id"
      :scroll-x="900"
      size="small"
    >
      <template #empty>
        <NEmpty :description="emptyText || $t('page.home.empty')" />
      </template>
    </NDataTable>
  </NCard>
</template>

<style scoped></style>
