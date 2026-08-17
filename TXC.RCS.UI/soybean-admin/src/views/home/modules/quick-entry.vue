<script setup lang="ts">
import { computed } from 'vue';
import { useRouterPush } from '@/hooks/common/router';
import { $t } from '@/locales';

defineOptions({
  name: 'QuickEntry'
});

const { routerPushByKey } = useRouterPush();

const entries = computed(() => [
  {
    key: 'task',
    title: $t('page.home.goTask'),
    desc: $t('page.home.goTaskDesc'),
    icon: 'mdi:robot-industrial'
  },
  {
    key: 'create',
    title: $t('page.home.createTask'),
    desc: $t('page.home.createTaskDesc'),
    icon: 'mdi:plus-circle-outline'
  },
  {
    key: 'mes',
    title: $t('page.home.mesEntry'),
    desc: $t('page.home.mesEntryDesc'),
    icon: 'mdi:factory'
  }
]);

function handleClick() {
  routerPushByKey('task');
}
</script>

<template>
  <NCard :title="$t('page.home.quickEntry')" :bordered="false" size="small" class="h-full card-wrapper">
    <NSpace vertical :size="12">
      <NButton
        v-for="item in entries"
        :key="item.key"
        secondary
        block
        class="h-auto justify-start px-16px py-12px"
        @click="handleClick"
      >
        <div class="w-full flex items-center gap-12px text-left">
          <SvgIcon :icon="item.icon" class="text-24px text-primary" />
          <div>
            <div class="text-14px font-medium">{{ item.title }}</div>
            <div class="text-12px text-#999">{{ item.desc }}</div>
          </div>
        </div>
      </NButton>
    </NSpace>
  </NCard>
</template>

<style scoped></style>
