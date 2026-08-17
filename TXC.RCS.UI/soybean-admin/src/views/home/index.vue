<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { $t } from '@/locales';
import HeaderBanner from './modules/header-banner.vue';
import CardData from './modules/card-data.vue';
import StatusChart from './modules/status-chart.vue';
import RecentTasks from './modules/recent-tasks.vue';
import QuickEntry from './modules/quick-entry.vue';
import { useHomeDashboard } from './modules/use-home-dashboard';

defineOptions({
  name: 'HomePage'
});

const appStore = useAppStore();
const { loading, lastRefresh, metrics, statusSeries, sourceSeries, recentTasks, failedTasks, load, start } =
  useHomeDashboard();

start();

const gap = computed(() => (appStore.isMobile ? 0 : 16));
</script>

<template>
  <NSpace vertical :size="16">
    <HeaderBanner
      :running="metrics.running"
      :pending="metrics.pending"
      :failed="metrics.failed"
      :last-refresh="lastRefresh"
      :loading="loading"
      @refresh="load"
    />
    <CardData
      :total="metrics.total"
      :running="metrics.running"
      :pending="metrics.pending"
      :failed="metrics.failed"
      :mes="metrics.mes"
      :manual="metrics.manual"
    />
    <NGrid :x-gap="gap" :y-gap="16" responsive="screen" item-responsive>
      <NGi span="24 s:24 m:14">
        <StatusChart :title="$t('page.home.statusDist')" kind="lifecycle" :series="statusSeries" />
      </NGi>
      <NGi span="24 s:24 m:10">
        <StatusChart :title="$t('page.home.sourceDist')" kind="source" :series="sourceSeries" />
      </NGi>
    </NGrid>
    <NGrid :x-gap="gap" :y-gap="16" responsive="screen" item-responsive>
      <NGi span="24 s:24 m:16">
        <RecentTasks :title="$t('page.home.recentTasks')" :tasks="recentTasks" :loading="loading" />
      </NGi>
      <NGi span="24 s:24 m:8">
        <QuickEntry />
      </NGi>
    </NGrid>
    <RecentTasks
      :title="$t('page.home.exceptions')"
      :tasks="failedTasks"
      :loading="loading"
      :empty-text="$t('page.home.noException')"
    />
  </NSpace>
</template>

<style scoped></style>
