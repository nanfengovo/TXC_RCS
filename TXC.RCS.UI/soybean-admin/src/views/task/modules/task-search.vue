<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { storeToRefs } from 'pinia';
import { useNaiveForm } from '@/hooks/common/form';
import { useCollapseAnimation } from '@/composables/use-collapse-animation';
import { mergeAddressOptions, useAddressOptions } from '@/composables/use-address-options';
import { useRcsConfigStore } from '@/store/modules/rcs-config';
import { $t } from '@/locales';

defineOptions({ name: 'TaskSearch' });

interface Emits {
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useNaiveForm();
const rcsConfigStore = useRcsConfigStore();
const { config } = storeToRefs(rcsConfigStore);
const { addressOptions, loadAddresses } = useAddressOptions();

const model = defineModel<Api.Task.TaskSearchParams>('model', { required: true });

const expanded = ref(!config.value.taskSearchCollapsed);
const { contentRef } = useCollapseAnimation(expanded);

const activeFilterCount = computed(() =>
  [
    model.value.keyword,
    model.value.source,
    model.value.lifecycleStatus,
    model.value.fromAddress,
    model.value.toAddress,
    model.value.containerId,
    model.value.lotId
  ].filter(Boolean).length
);

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

const fromOptions = computed(() =>
  mergeAddressOptions(addressOptions.value, config.value.fromAddressSuggestions)
);

const toOptions = computed(() =>
  mergeAddressOptions(addressOptions.value, config.value.toAddressSuggestions)
);

function toggleExpanded() {
  expanded.value = !expanded.value;
  rcsConfigStore.update({ taskSearchCollapsed: !expanded.value });
}

async function reset() {
  await restoreValidation();
  emit('reset');
}

async function search() {
  await validate();
  emit('search');
}

onMounted(() => {
  loadAddresses();
  if (!expanded.value && contentRef.value) {
    contentRef.value.style.display = 'none';
    contentRef.value.style.height = '0';
    contentRef.value.style.opacity = '0';
  }
});

watch(
  () => config.value.taskSearchCollapsed,
  collapsed => {
    expanded.value = !collapsed;
  }
);

const labelWidth = computed(() => '80px');
</script>

<template>
  <NCard
    :bordered="false"
    size="small"
    class="card-wrapper task-search"
    :class="{ 'task-search--collapsed': !expanded }"
  >
    <div class="task-search__bar">
      <NSpace align="center" :size="8">
        <NButton quaternary size="small" @click="toggleExpanded">
          <template #icon>
            <icon-ic-round-keyboard-arrow-down
              class="text-icon transition-transform duration-300"
              :class="{ 'rotate-180': expanded }"
            />
          </template>
          {{ expanded ? '收起筛选' : '展开筛选' }}
        </NButton>
        <NTag v-if="activeFilterCount" size="small" type="info">{{ activeFilterCount }} 项条件</NTag>
      </NSpace>
      <NSpace>
        <NButton size="small" @click="reset">
          <template #icon>
            <icon-ic-round-refresh class="text-icon" />
          </template>
          {{ $t('common.reset') }}
        </NButton>
        <NButton type="primary" size="small" ghost @click="search">
          <template #icon>
            <icon-ic-round-search class="text-icon" />
          </template>
          {{ $t('common.search') }}
        </NButton>
      </NSpace>
    </div>

    <div ref="contentRef" class="task-search__body">
      <NForm ref="formRef" :model="model" label-placement="left" :label-width="labelWidth" class="pt-12px">
        <NGrid responsive="screen" item-responsive>
          <NFormItemGi span="24 s:12 m:6" label="关键词" path="keyword">
            <NInput v-model:value="model.keyword" clearable placeholder="任务号 / 料盒 / 批次" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="来源" path="source">
            <NSelect v-model:value="model.source" clearable :options="sourceOptions" placeholder="全部" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="状态" path="lifecycleStatus">
            <NSelect
              v-model:value="model.lifecycleStatus"
              clearable
              :options="statusOptions"
              placeholder="全部"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="起点" path="fromAddress">
            <NAutoComplete
              :value="model.fromAddress ?? ''"
              clearable
              :options="fromOptions"
              placeholder="如 ERACK"
              @update:value="model.fromAddress = $event || null"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="终点" path="toAddress">
            <NAutoComplete
              :value="model.toAddress ?? ''"
              clearable
              :options="toOptions"
              placeholder="如 H044"
              @update:value="model.toAddress = $event || null"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="料盒" path="containerId">
            <NInput v-model:value="model.containerId" clearable />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="批次" path="lotId">
            <NInput v-model:value="model.lotId" clearable placeholder="Lot ID" />
          </NFormItemGi>
        </NGrid>
      </NForm>
    </div>
  </NCard>
</template>

<style scoped>
.task-search--collapsed :deep(.n-card__content) {
  padding-top: 10px;
  padding-bottom: 10px;
}

.task-search__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.task-search__body {
  overflow: hidden;
  will-change: height, opacity;
}
</style>
