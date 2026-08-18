<script setup lang="ts">
import { computed } from 'vue';
import { formatTaskTime, getActiveLegMeta } from './task-status';

defineOptions({ name: 'TaskDetailExpand' });

const props = defineProps<{
  task: Api.Task.TaskItem;
}>();

const emit = defineEmits<{
  monitor: [];
}>();

const activeLegMeta = computed(() => getActiveLegMeta(props.task.activeLeg));

const schemaText = computed(() =>
  props.task.optionCodeSchemaCode
    ? `${props.task.optionCodeSchemaCode} v${props.task.optionCodeSchemaVersion}`
    : '—'
);
</script>

<template>
  <div class="task-detail-strip">
    <div class="task-detail-strip__head">
      <span class="task-detail-strip__title">扩展字段</span>
      <span class="task-detail-strip__hint">列表行内已展示基础信息，此处仅补充长文本与编码字段</span>
      <NButton size="tiny" type="primary" ghost @click="emit('monitor')">打开监控</NButton>
    </div>

    <NDescriptions
      bordered
      size="small"
      :column="3"
      label-placement="left"
      :label-style="{ width: '96px', whiteSpace: 'nowrap' }"
      class="task-detail-strip__desc"
    >
      <NDescriptionsItem label="料盒">{{ task.containerId || '—' }}</NDescriptionsItem>
      <NDescriptionsItem label="当前步骤">
        <NTag v-if="task.activeLeg" size="small" :type="activeLegMeta.color">{{ activeLegMeta.label }}</NTag>
        <span v-else>—</span>
      </NDescriptionsItem>
      <NDescriptionsItem label="流程序号">{{ task.stepIndex ?? '—' }}</NDescriptionsItem>
      <NDescriptionsItem label="Schema" :span="1">{{ schemaText }}</NDescriptionsItem>
      <NDescriptionsItem label="创建时间">{{ formatTaskTime(task.creationTime) }}</NDescriptionsItem>
      <NDescriptionsItem label="更新时间">{{ formatTaskTime(task.lastModificationTime) }}</NDescriptionsItem>
      <NDescriptionsItem label="取 Serial" :span="3">
        <NText code class="task-detail-strip__mono">{{ task.fetchTaskSerial || '—' }}</NText>
      </NDescriptionsItem>
      <NDescriptionsItem label="放 Serial" :span="3">
        <NText code class="task-detail-strip__mono">{{ task.putTaskSerial || '—' }}</NText>
      </NDescriptionsItem>
      <NDescriptionsItem label="取 Code" :span="3">
        <NText code class="task-detail-strip__mono">{{ task.fetchOptionCode || '—' }}</NText>
      </NDescriptionsItem>
      <NDescriptionsItem label="放 Code" :span="3">
        <NText code class="task-detail-strip__mono">{{ task.putOptionCode || '—' }}</NText>
      </NDescriptionsItem>
      <NDescriptionsItem v-if="task.lastError" label="最后错误" :span="3">
        <NText type="error">{{ task.lastError }}</NText>
      </NDescriptionsItem>
    </NDescriptions>
  </div>
</template>

<style scoped>
.task-detail-strip {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 12px 12px;
  background: var(--n-color-modal);
  border-top: 1px solid var(--n-border-color);
}

.task-detail-strip__head {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
  flex-wrap: wrap;
}

.task-detail-strip__title {
  font-size: 13px;
  font-weight: 600;
}

.task-detail-strip__hint {
  flex: 1;
  font-size: 12px;
  opacity: 0.55;
  min-width: 160px;
}

.task-detail-strip__desc {
  width: 100%;
}

.task-detail-strip__mono {
  display: block;
  word-break: break-all;
  line-height: 1.45;
  font-size: 12px;
}

.task-detail-strip :deep(.n-descriptions-table-wrapper) {
  width: 100%;
}

.task-detail-strip :deep(.n-descriptions-table-content) {
  word-break: break-all;
}
</style>
