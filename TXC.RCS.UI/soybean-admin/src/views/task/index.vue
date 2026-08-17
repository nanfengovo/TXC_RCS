<script setup lang="tsx">
import { onBeforeUnmount, reactive, ref, watch } from 'vue';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { NButton, NPopconfirm, NTag } from 'naive-ui';
import {
  fetchCancelTask,
  fetchDeleteTask,
  fetchGetTaskList,
  fetchRetryMesReport
} from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import TaskSearch from './modules/task-search.vue';
import TaskOperateDrawer from './modules/task-operate-drawer.vue';
import TaskMonitorModal from './modules/task-monitor-modal.vue';
import {
  canRetryMes,
  formatTaskTime,
  getLifecycleMeta,
  getSourceMeta,
  isTaskCancelable,
  isTaskDeletable
} from './modules/task-status';

defineOptions({ name: 'TaskPage' });

const appStore = useAppStore();

const searchModel = reactive<Api.Task.TaskSearchParams>({
  keyword: null,
  source: null,
  lifecycleStatus: null,
  fromAddress: null,
  toAddress: null,
  containerId: null,
  lotId: null
});

const query = reactive({
  page: 1,
  pageSize: 10
});

function abpTransform(
  response: FlatResponseData<any, Api.Task.TaskList>
): PaginationData<Api.Task.TaskItem> {
  const page = response.data;
  return {
    data: page?.items ?? [],
    pageNum: query.page,
    pageSize: query.pageSize,
    total: page?.totalCount ?? 0
  };
}

const {
  columns,
  columnChecks,
  data,
  getData,
  getDataByPage,
  loading,
  mobilePagination
} = useNaivePaginatedTable({
  api: () =>
    fetchGetTaskList({
      page: query.page,
      pageSize: query.pageSize,
      ...searchModel
    }),
  transform: abpTransform,
  onPaginationParamsChange: params => {
    query.page = params.page ?? 1;
    query.pageSize = params.pageSize ?? 10;
  },
  columns: () => [
    {
      key: 'index',
      title: $t('common.index'),
      align: 'center',
      width: 64,
      render: (_, index) => index + 1 + (query.page - 1) * query.pageSize
    },
    {
      key: 'id',
      title: '任务号',
      align: 'center',
      minWidth: 180,
      ellipsis: { tooltip: true }
    },
    {
      key: 'source',
      title: '来源',
      align: 'center',
      width: 90,
      render: row => {
        const meta = getSourceMeta(row.source);
        return <NTag type={meta.color}>{meta.label}</NTag>;
      }
    },
    {
      key: 'lifecycleStatus',
      title: '状态',
      align: 'center',
      width: 100,
      render: row => {
        const meta = getLifecycleMeta(row.lifecycleStatus);
        return <NTag type={meta.color}>{meta.label}</NTag>;
      }
    },
    {
      key: 'route',
      title: '路径',
      align: 'center',
      minWidth: 200,
      render: row => (
        <span>
          {row.fromAddress}/{row.fromPort || '-'} → {row.toAddress}/{row.toPort || '-'}
        </span>
      )
    },
    {
      key: 'containerId',
      title: '料盒',
      align: 'center',
      minWidth: 120,
      ellipsis: { tooltip: true },
      render: row => row.containerId || '-'
    },
    {
      key: 'waitingEvent',
      title: '等待事件',
      align: 'center',
      minWidth: 120,
      render: row => row.waitingEvent || '-'
    },
    {
      key: 'creationTime',
      title: '创建时间',
      align: 'center',
      width: 170,
      render: row => formatTaskTime(row.creationTime)
    },
    {
      key: 'operate',
      title: $t('common.operate'),
      align: 'center',
      width: 280,
      fixed: 'right',
      render: row => (
        <div class="flex-center gap-8px flex-wrap">
          <NButton type="primary" ghost size="small" onClick={() => openMonitor(row.id)}>
            监控
          </NButton>
          {isTaskCancelable(row.lifecycleStatus) && (
            <NPopconfirm onPositiveClick={() => handleCancel(row.id)}>
              {{
                default: () => '确认取消该任务？',
                trigger: () => (
                  <NButton type="warning" ghost size="small">
                    取消
                  </NButton>
                )
              }}
            </NPopconfirm>
          )}
          {canRetryMes(row) && (
            <NButton type="info" ghost size="small" onClick={() => handleRetryMes(row.id)}>
              重推MES
            </NButton>
          )}
          {isTaskDeletable(row.lifecycleStatus) && (
            <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
              {{
                default: () => '确认删除该任务记录？',
                trigger: () => (
                  <NButton type="error" ghost size="small">
                    删除
                  </NButton>
                )
              }}
            </NPopconfirm>
          )}
        </div>
      )
    }
  ]
});

const { drawerVisible, handleAdd } = useTableOperate(data, 'id', getData);

const monitorVisible = ref(false);
const monitorTaskId = ref<string | null>(null);
const autoRefresh = ref(false);
let listTimer: ReturnType<typeof setInterval> | null = null;

function openMonitor(id: string) {
  monitorTaskId.value = id;
  monitorVisible.value = true;
}

async function handleCancel(id: string) {
  const { error } = await fetchCancelTask({ id });
  if (error) return;
  window.$message?.success('已取消');
  getData();
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteTask(id);
  if (error) return;
  window.$message?.success($t('common.deleteSuccess'));
  getData();
}

async function handleRetryMes(id: string) {
  const { data: result, error } = await fetchRetryMesReport(id);
  if (error) return;
  if (result?.accepted) {
    window.$message?.success(result.message || 'MES 已接受');
  } else {
    window.$message?.warning(result?.message || 'MES 拒绝');
  }
  getData();
}

function handleSearch() {
  getDataByPage(1);
}

function handleReset() {
  Object.assign(searchModel, {
    keyword: null,
    source: null,
    lifecycleStatus: null,
    fromAddress: null,
    toAddress: null,
    containerId: null,
    lotId: null
  });
  getDataByPage(1);
}

function toggleListPolling(on: boolean) {
  if (listTimer) {
    clearInterval(listTimer);
    listTimer = null;
  }
  if (on) {
    listTimer = setInterval(() => {
      if (document.hidden) return;
      getData();
    }, 8000);
  }
}

watch(autoRefresh, val => toggleListPolling(val), { immediate: true });
onBeforeUnmount(() => toggleListPolling(false));
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <TaskSearch v-model:model="searchModel" @reset="handleReset" @search="handleSearch" />

    <NCard title="搬运任务" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header-extra>
        <TableHeaderOperation v-model:columns="columnChecks" :loading="loading" @refresh="getData">
          <template #default>
            <NSpace align="center">
              <NButton type="primary" size="small" @click="handleAdd">
                <template #icon>
                  <icon-ic-round-plus class="text-icon" />
                </template>
                下发任务
              </NButton>
              <NSpace align="center" :size="6">
                <span class="text-12px opacity-60">列表自动刷新</span>
                <NSwitch v-model:value="autoRefresh" size="small" />
              </NSpace>
            </NSpace>
          </template>
        </TableHeaderOperation>
      </template>

      <NDataTable
        :columns="columns"
        :data="data"
        size="small"
        :flex-height="!appStore.isMobile"
        :scroll-x="1200"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />

      <TaskOperateDrawer v-model:visible="drawerVisible" operate-type="add" @submitted="handleSearch" />
      <TaskMonitorModal v-model:visible="monitorVisible" :task-id="monitorTaskId" />
    </NCard>
  </div>
</template>
