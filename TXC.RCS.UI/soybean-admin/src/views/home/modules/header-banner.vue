<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useAuthStore } from '@/store/modules/auth';
import { $t } from '@/locales';

defineOptions({
  name: 'HeaderBanner'
});

interface Props {
  running: number;
  pending: number;
  failed: number;
  lastRefresh: string;
  loading: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  refresh: [];
}>();

const appStore = useAppStore();
const authStore = useAuthStore();

const gap = computed(() => (appStore.isMobile ? 0 : 16));

const statisticData = computed(() => [
  { id: 0, label: $t('page.home.running'), value: String(props.running) },
  { id: 1, label: $t('page.home.pending'), value: String(props.pending) },
  { id: 2, label: $t('page.home.failed'), value: String(props.failed) }
]);
</script>

<template>
  <NCard :bordered="false" class="card-wrapper">
    <NGrid :x-gap="gap" :y-gap="16" responsive="screen" item-responsive>
      <NGi span="24 s:24 m:16">
        <div class="flex-y-center">
          <div class="size-72px shrink-0 flex-center overflow-hidden rd-1/2 bg-primary/12">
            <SystemLogo class="size-44px" />
          </div>
          <div class="pl-12px">
            <h3 class="text-18px font-semibold">
              {{ $t('page.home.greeting', { userName: authStore.userInfo.userName || 'admin' }) }}
            </h3>
            <p class="text-#999 leading-30px">{{ $t('page.home.subtitle') }}</p>
            <p class="text-12px text-#999">{{ $t('page.home.lastRefresh') }}：{{ lastRefresh }}</p>
          </div>
        </div>
      </NGi>
      <NGi span="24 s:24 m:8">
        <div class="h-full flex items-center justify-end gap-24px lt-sm:justify-between">
          <NStatistic v-for="item in statisticData" :key="item.id" class="whitespace-nowrap" v-bind="item" />
          <NButton type="primary" :loading="loading" @click="emit('refresh')">
            {{ $t('page.home.refresh') }}
          </NButton>
        </div>
      </NGi>
    </NGrid>
  </NCard>
</template>

<style scoped></style>
