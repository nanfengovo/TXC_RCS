<script setup lang="ts">
import { computed } from 'vue';
import { useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({ name: 'TaskSearch' });

interface Emits {
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useNaiveForm();

const model = defineModel<Api.Task.TaskSearchParams>('model', { required: true });

const sourceOptions = [
  { label: '人工', value: 'Manual' },
  { label: 'MES', value: 'Mes' }
];

const statusOptions = [
  { label: '待开始', value: 'Pending' },
  { label: '运行中', value: 'Running' },
  { label: '已完成', value: 'Succeeded' },
  { label: '失败', value: 'Failed' },
  { label: '已取消', value: 'Canceled' }
];

async function reset() {
  await restoreValidation();
  emit('reset');
}

async function search() {
  await validate();
  emit('search');
}

const labelWidth = computed(() => '80px');
</script>

<template>
  <NCard :bordered="false" size="small" class="card-wrapper">
    <NForm ref="formRef" :model="model" label-placement="left" :label-width="labelWidth">
      <NGrid responsive="screen" item-responsive>
        <NFormItemGi span="24 s:12 m:6" label="关键词" path="keyword">
          <NInput v-model:value="model.keyword" clearable placeholder="任务号 / 料盒 / 批次" />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:6" label="来源" path="source">
          <NSelect v-model:value="model.source" clearable :options="sourceOptions" placeholder="全部" />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:6" label="状态" path="lifecycleStatus">
          <NSelect v-model:value="model.lifecycleStatus" clearable :options="statusOptions" placeholder="全部" />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:6" label="起点" path="fromAddress">
          <NInput v-model:value="model.fromAddress" clearable placeholder="如 ERACK" />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:6" label="终点" path="toAddress">
          <NInput v-model:value="model.toAddress" clearable placeholder="如 H044" />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:6" label="料盒" path="containerId">
          <NInput v-model:value="model.containerId" clearable />
        </NFormItemGi>
        <NFormItemGi span="24 s:12 m:12" class="search-buttons">
          <NSpace class="w-full" justify="end">
            <NButton @click="reset">
              <template #icon>
                <icon-ic-round-refresh class="text-icon" />
              </template>
              {{ $t('common.reset') }}
            </NButton>
            <NButton type="primary" ghost @click="search">
              <template #icon>
                <icon-ic-round-search class="text-icon" />
              </template>
              {{ $t('common.search') }}
            </NButton>
          </NSpace>
        </NFormItemGi>
      </NGrid>
    </NForm>
  </NCard>
</template>

<style scoped>
.search-buttons {
  padding-top: 4px;
}
</style>
