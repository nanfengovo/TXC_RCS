<script setup lang="tsx">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { storeToRefs } from 'pinia';
import type { DataTableColumns } from 'naive-ui';
import { NButton, NCode, NTag } from 'naive-ui';
import { useThemeStore } from '@/store/modules/theme';
import { useRcsConfigStore } from '@/store/modules/rcs-config';
import { fetchGetTaskMonitorDetail } from '@/service/api';
import TaskMonitorStepper from './task-monitor-stepper.vue';
import { formatTaskTime, getActiveLegMeta, getLifecycleMeta, getSourceMeta } from './task-status';

defineOptions({ name: 'TaskMonitorModal' });

interface Props {
  taskId: string | null;
}

const props = defineProps<Props>();

const visible = defineModel<boolean>('visible', { default: false });

const themeStore = useThemeStore();
const rcsConfigStore = useRcsConfigStore();
const { config: rcsConfig } = storeToRefs(rcsConfigStore);
const loading = ref(false);
const inFlight = ref(false);
const detail = ref<Api.Task.MonitorDetail | null>(null);
const polling = ref(false);
const activeTab = ref<'overview' | 'flow' | 'logs'>('overview');
const expandedAdvanced = ref<string[]>([]);
const lastRefresh = ref('-');
let timer: ReturnType<typeof setInterval> | null = null;

const drawerWidth = computed(() => rcsConfig.value.taskMonitorWidth);
const task = computed(() => detail.value?.task);
const lifecycle = computed(() => getLifecycleMeta(task.value?.lifecycleStatus));
const source = computed(() => getSourceMeta(task.value?.source));
const timeline = computed(() => detail.value?.timeline ?? []);
const logs = computed(() => detail.value?.logs ?? []);

const overviewItems = computed(() => {
  const t = task.value;
  if (!t) return [];
  return [
    { label: '路径', value: `${t.fromAddress}/${t.fromPort || '-'} → ${t.toAddress}/${t.toPort || '-'}` },
    { label: '批次 Lot', value: t.lotId || '-' },
    { label: '料盒', value: t.containerId || '-' },
    { label: '等待事件', value: t.waitingEvent || '-' },
    { label: '当前步骤', value: getActiveLegMeta(t.activeLeg).label },
    { label: '流程序号', value: String(t.stepIndex ?? '-') },
    { label: 'AGV', value: t.agvSerial || '-' },
    { label: '创建时间', value: formatTaskTime(t.creationTime) },
    { label: '更新时间', value: formatTaskTime(t.lastModificationTime) }
  ];
});

const advancedItems = computed(() => {
  const t = task.value;
  if (!t) return [];
  return [
    { label: '取 Serial', value: t.fetchTaskSerial || '-', mono: true },
    { label: '放 Serial', value: t.putTaskSerial || '-', mono: true },
    { label: '取 Code', value: t.fetchOptionCode || '-', mono: true },
    { label: '放 Code', value: t.putOptionCode || '-', mono: true },
    {
      label: 'Schema',
      value: t.optionCodeSchemaCode ? `${t.optionCodeSchemaCode} v${t.optionCodeSchemaVersion}` : '-'
    }
  ];
});

const logColumns: DataTableColumns<Api.Task.InteractionLog> = [
  {
    type: 'expand',
    expandable: (row: Api.Task.InteractionLog) => Boolean(row.detailJson),
    renderExpand: (row: Api.Task.InteractionLog) => (
      <div class="px-12px py-8px">
        <NCode language="json" code={formatDetailJson(row.detailJson)} word-wrap />
      </div>
    )
  },
  {
    title: '时间',
    key: 'creationTime',
    width: 168,
    render: (row: Api.Task.InteractionLog) => formatTaskTime(row.creationTime)
  },
  { title: '分类', key: 'category', width: 88, ellipsis: { tooltip: true } },
  { title: '事件', key: 'eventName', width: 120, ellipsis: { tooltip: true } },
  {
    title: '腿',
    key: 'leg',
    width: 64,
    render: (row: Api.Task.InteractionLog) => row.leg || '-'
  },
  {
    title: '结果',
    key: 'success',
    width: 72,
    render: (row: Api.Task.InteractionLog) => (
      <NTag type={row.success ? 'success' : 'error'} size="small">
        {row.success ? '成功' : '失败'}
      </NTag>
    )
  },
  { title: '说明', key: 'message', ellipsis: { tooltip: true } }
];

function formatDetailJson(raw?: string | null) {
  if (!raw) return '';
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}

async function loadDetail() {
  if (!props.taskId || inFlight.value) return;
  inFlight.value = true;
  loading.value = true;
  try {
    const { data, error } = await fetchGetTaskMonitorDetail(props.taskId);
    if (!error && data) {
      detail.value = data;
      lastRefresh.value = formatTaskTime(new Date().toISOString());
    }
  } finally {
    loading.value = false;
    inFlight.value = false;
  }
}

function stopPolling() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

function startPolling() {
  stopPolling();
  if (!polling.value || !visible.value || !props.taskId) return;
  timer = setInterval(() => {
    if (document.hidden) return;
    void loadDetail();
  }, rcsConfig.value.taskMonitorPollMs);
}

watch(
  () => rcsConfig.value.taskMonitorPollMs,
  () => startPolling()
);

watch(polling, () => startPolling());

watch(
  () => [visible.value, props.taskId] as const,
  async ([show, id]) => {
    if (show && id) {
      activeTab.value = 'overview';
      expandedAdvanced.value = [];
      await loadDetail();
      startPolling();
    } else {
      stopPolling();
      detail.value = null;
    }
  }
);

onBeforeUnmount(stopPolling);
</script>

<template>
  <NDrawer
    v-model:show="visible"
    display-directive="show"
    :width="drawerWidth"
    placement="right"
    :trap-focus="false"
    :block-scroll="false"
  >
    <NDrawerContent
      closable
      :native-scrollbar="false"
      body-content-class="task-monitor-drawer__body"
      :class="themeStore.darkMode ? 'task-monitor-drawer is-dark' : 'task-monitor-drawer is-light'"
    >
      <template #header>
        <div class="task-monitor-drawer__header">
          <div class="task-monitor-drawer__title-row">
            <span class="task-monitor-drawer__title">任务监控</span>
            <NTag :type="lifecycle.color" size="small">{{ lifecycle.label }}</NTag>
            <NTag :type="source.color" size="small">{{ source.label }}</NTag>
          </div>
          <div v-if="task" class="task-monitor-drawer__task-id">{{ task.id }}</div>
        </div>
      </template>

      <template #footer>
        <NSpace v-if="task" align="center" justify="space-between" class="w-full">
          <span class="text-12px opacity-55">最近刷新 {{ lastRefresh }}</span>
          <NSpace align="center">
            <span class="text-12px opacity-60">自动刷新</span>
            <NSwitch v-model:value="polling" size="small" />
            <NButton size="small" :loading="loading" @click="loadDetail">刷新</NButton>
          </NSpace>
        </NSpace>
      </template>

      <NSpin :show="loading && !detail">
        <template v-if="task">
          <NTabs v-model:value="activeTab" type="line" animated size="small" class="task-monitor-tabs">
            <NTabPane name="overview" tab="概要" display-directive="show:lazy">
              <div class="task-monitor-panel">
                <div v-if="task.lastError" class="task-monitor-alert">
                  {{ task.lastError }}
                </div>
                <div class="task-monitor-meta">
                  <div v-for="item in overviewItems" :key="item.label" class="task-monitor-meta__row">
                    <span class="task-monitor-meta__label">{{ item.label }}</span>
                    <span class="task-monitor-meta__value" :title="item.value">{{ item.value }}</span>
                  </div>
                </div>
                <NCollapse v-model:expanded-names="expandedAdvanced">
                  <NCollapseItem title="高级字段" name="advanced">
                    <div class="task-monitor-meta">
                      <div
                        v-for="item in advancedItems"
                        :key="item.label"
                        class="task-monitor-meta__row"
                      >
                        <span class="task-monitor-meta__label">{{ item.label }}</span>
                        <span
                          class="task-monitor-meta__value"
                          :class="{ 'is-mono': item.mono }"
                          :title="item.value"
                        >
                          {{ item.value }}
                        </span>
                      </div>
                    </div>
                  </NCollapseItem>
                </NCollapse>
              </div>
            </NTabPane>

            <NTabPane name="flow" tab="流程" display-directive="show:lazy">
              <div class="task-monitor-panel">
                <TaskMonitorStepper :steps="timeline" />
              </div>
            </NTabPane>

            <NTabPane name="logs" tab="交互日志" display-directive="show:lazy">
              <div class="task-monitor-panel task-monitor-panel--logs">
                <NDataTable
                  size="small"
                  :bordered="false"
                  :single-line="false"
                  :columns="logColumns"
                  :data="logs"
                  :row-key="row => row.id"
                  virtual-scroll
                  :max-height="320"
                  :scroll-x="640"
                />
              </div>
            </NTabPane>
          </NTabs>
        </template>
        <NEmpty v-else description="暂无监控数据" />
      </NSpin>
    </NDrawerContent>
  </NDrawer>
</template>

<style scoped>
.task-monitor-drawer__header {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.task-monitor-drawer__title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.task-monitor-drawer__title {
  font-size: 16px;
  font-weight: 600;
}

.task-monitor-drawer__task-id {
  font-size: 12px;
  opacity: 0.65;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.task-monitor-drawer.is-dark {
  --monitor-glass: rgb(20 24 28 / 88%);
  --monitor-stroke: rgb(255 255 255 / 10%);
}

.task-monitor-drawer.is-light {
  --monitor-glass: rgb(255 255 255 / 92%);
  --monitor-stroke: rgb(0 0 0 / 8%);
}

.task-monitor-panel {
  padding-top: 4px;
}

.task-monitor-panel--logs {
  min-height: 240px;
}

.task-monitor-alert {
  margin-bottom: 12px;
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 13px;
  line-height: 1.5;
  color: var(--n-error-color);
  background: rgba(var(--error-color, 208 48 48) / 0.08);
  border: 1px solid rgba(var(--error-color, 208 48 48) / 0.2);
}

.task-monitor-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.task-monitor-meta__row {
  min-height: 34px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 4px 0;
  border-bottom: 1px solid var(--monitor-stroke);
}

.task-monitor-meta__row:last-child {
  border-bottom: none;
}

.task-monitor-meta__label {
  width: 88px;
  flex-shrink: 0;
  font-size: 13px;
  opacity: 0.65;
}

.task-monitor-meta__value {
  flex: 1;
  font-size: 13px;
  font-weight: 500;
  text-align: right;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.task-monitor-meta__value.is-mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 12px;
}

.task-monitor-tabs :deep(.n-tabs-pane-wrapper) {
  padding-top: 8px;
}

:deep(.task-monitor-drawer__body) {
  padding-top: 0;
}
</style>
