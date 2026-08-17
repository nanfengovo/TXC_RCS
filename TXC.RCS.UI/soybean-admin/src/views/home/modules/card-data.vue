<script setup lang="ts">
import { computed } from 'vue';
import { createReusableTemplate } from '@vueuse/core';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';

defineOptions({
  name: 'CardData'
});

interface Props {
  total: number;
  running: number;
  pending: number;
  failed: number;
  mes: number;
  manual: number;
}

const props = defineProps<Props>();

interface CardData {
  key: string;
  title: string;
  value: number;
  color: {
    start: string;
    end: string;
  };
  icon: string;
}

const cardData = computed<CardData[]>(() => [
  {
    key: 'total',
    title: $t('page.home.total'),
    value: props.total,
    color: { start: '#56cdf3', end: '#719de3' },
    icon: 'mdi:clipboard-list-outline'
  },
  {
    key: 'running',
    title: $t('page.home.running'),
    value: props.running,
    color: { start: '#865ec0', end: '#5144b4' },
    icon: 'mdi:play-circle-outline'
  },
  {
    key: 'pending',
    title: $t('page.home.pending'),
    value: props.pending,
    color: { start: '#fcbc25', end: '#f68057' },
    icon: 'mdi:timer-sand'
  },
  {
    key: 'failed',
    title: $t('page.home.failed'),
    value: props.failed,
    color: { start: '#ec4786', end: '#b955a4' },
    icon: 'mdi:alert-circle-outline'
  },
  {
    key: 'mes',
    title: $t('page.home.mes'),
    value: props.mes,
    color: { start: '#26deca', end: '#1aa89a' },
    icon: 'mdi:factory'
  },
  {
    key: 'manual',
    title: $t('page.home.manual'),
    value: props.manual,
    color: { start: '#8e9dff', end: '#6c7ae0' },
    icon: 'mdi:account-hard-hat'
  }
]);

interface GradientBgProps {
  gradientColor: string;
}

const [DefineGradientBg, GradientBg] = createReusableTemplate<GradientBgProps>();

const themeStore = useThemeStore();

function getGradientColor(color: CardData['color']) {
  return `linear-gradient(to bottom right, ${color.start}, ${color.end})`;
}
</script>

<template>
  <NCard :bordered="false" size="small" class="card-wrapper">
    <DefineGradientBg v-slot="{ $slots, gradientColor }">
      <div
        class="px-16px pb-4px pt-8px text-white"
        :style="{ backgroundImage: gradientColor, borderRadius: themeStore.themeRadius + 'px' }"
      >
        <component :is="$slots.default" />
      </div>
    </DefineGradientBg>

    <NGrid cols="s:1 m:2 l:3" responsive="screen" :x-gap="16" :y-gap="16">
      <NGi v-for="item in cardData" :key="item.key">
        <GradientBg :gradient-color="getGradientColor(item.color)" class="flex-1">
          <h3 class="text-16px">{{ item.title }}</h3>
          <div class="flex justify-between pt-12px">
            <SvgIcon :icon="item.icon" class="text-32px" />
            <CountTo :start-value="0" :end-value="item.value" class="text-30px text-white dark:text-dark" />
          </div>
        </GradientBg>
      </NGi>
    </NGrid>
  </NCard>
</template>

<style scoped></style>
