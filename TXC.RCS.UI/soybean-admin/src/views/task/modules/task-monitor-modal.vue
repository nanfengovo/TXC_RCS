<script setup lang="tsx">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { NButton, NTag } from 'naive-ui';
import { fetchGetTaskMonitorDetail } from '@/service/api';
import { formatTaskTime, getLifecycleMeta, getSourceMeta, timelineStatusType } from './task-status';

defineOptions({ name: 'TaskMonitorModal' });

interface Props {
  taskId: string | null;
}

const props = defineProps<Props>();

const visible = defineModel<boolean>('visible', { default: false });

const loading = ref(false);
const detail = ref<Api.Task.MonitorDetail | null>(null);
const polling = ref(true);
let timer: ReturnType<typeof setInterval> | null = null;

const task = computed(() => detail.value?.task);
const lifecycle = computed(() => getLifecycleMeta(task.value?.lifecycleStatus));
const source = computed(() => getSourceMeta(task.value?.source));

const logColumns = [
  {
    title: '时间',
    key: 'creationTime',
    width: 170,
    render: (row: Api.Task.InteractionLog) => formatTaskTime(row.creationTime)
  },
  { title: '分类', key: 'category', width: 90 },
  { title: '事件', key: 'eventName', width: 140 },
  {
    title: '腿',
    key: 'leg',
    width: 70,
    render: (row: Api.Task.InteractionLog) => row.leg || '-'
  },
  {
    title: '结果',
    key: 'success',
    width: 80,
    render: (row: Api.Task.InteractionLog) => (
      <NTag type={row.success ? 'success' : 'error'} size="small">
        {row.success ? '成功' : '失败'}
      </NTag>
    )
  },
  { title: '说明', key: 'message', ellipsis: { tooltip: true } }
];

async function loadDetail() {
  if (!props.taskId) return;
  loading.value = true;
  const { data, error } = await fetchGetTaskMonitorDetail(props.taskId);
  loading.value = false;
  if (error) return;
  detail.value = data;
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
    loadDetail();
  }, 5000);
}

watch(
  () => [visible.value, props.taskId, polling.value] as const,
  async ([show, id]) => {
    if (show && id) {
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
  <NModal
    v-model:show="visible"
    preset="card"
    class="w-900px max-w-95vw"
    title="任务监控"
    :bordered="false"
    display-directive="show"
  >
    <NSpin :show="loading">
      <template v-if="task">
        <NSpace class="mb-16px" align="center" justify="space-between">
          <NSpace>
            <NTag :type="lifecycle.color" size="small">{{ lifecycle.label }}</NTag>
            <NTag :type="source.color" size="small">{{ source.label }}</NTag>
            <span class="text-14px">{{ task.id }}</span>
          </NSpace>
          <NSpace align="center">
            <span class="text-12px opacity-60">自动刷新</span>
            <NSwitch v-model:value="polling" size="small" />
            <NButton size="small" @click="loadDetail">刷新</NButton>
          </NSpace>
        </NSpace>

        <NDescriptions label-placement="left" :column="2" size="small" class="mb-16px">
          <NDescriptionsItem label="路径">
            {{ task.fromAddress }}/{{ task.fromPort || '-' }} → {{ task.toAddress }}/{{ task.toPort || '-' }}
          </NDescriptionsItem>
          <NDescriptionsItem label="料盒">{{ task.containerId || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="等待事件">{{ task.waitingEvent || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="当前腿">{{ task.activeLeg || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="Fetch Serial">{{ task.fetchTaskSerial || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="Put Serial">{{ task.putTaskSerial || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="AGV">{{ task.agvSerial || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="创建时间">{{ formatTaskTime(task.creationTime) }}</NDescriptionsItem>
          <NDescriptionsItem label="Fetch Code">{{ task.fetchOptionCode || '-' }}</NDescriptionsItem>
          <NDescriptionsItem label="Put Code">{{ task.putOptionCode || '-' }}</NDescriptionsItem>
          <NDescriptionsItem v-if="task.lastError" label="错误" :span="2">
            <span class="text-error">{{ task.lastError }}</span>
          </NDescriptionsItem>
        </NDescriptions>

        <NDivider title-placement="left">关键时间节点</NDivider>
        <NTimeline>
          <NTimelineItem
            v-for="step in detail?.timeline || []"
            :key="step.key"
            :type="timelineStatusType(step.status)"
            :title="step.label"
            :time="formatTaskTime(step.time)"
          >
            <div class="text-12px opacity-60">
              <span v-if="step.eventName">{{ step.eventName }}</span>
              <span v-if="step.leg"> · {{ step.leg }}</span>
              <span> · {{ step.status }}</span>
            </div>
          </NTimelineItem>
        </NTimeline>

        <NDivider title-placement="left">交互日志</NDivider>
        <NDataTable
          size="small"
          :bordered="false"
          :single-line="false"
          :max-height="280"
          :columns="logColumns"
          :data="detail?.logs || []"
        />
      </template>
      <NEmpty v-else description="暂无监控数据" />
    </NSpin>
  </NModal>
</template>
