<script setup lang="ts">
defineOptions({
  name: 'GlobalLogo'
});

interface Props {
  /** Whether to show the title */
  showTitle?: boolean;
}

withDefaults(defineProps<Props>(), {
  showTitle: false
});
</script>

<template>
  <RouterLink to="/" class="w-full flex-center nowrap-hidden px-12px">
    <SystemLogo class="h-28px w-full" />
  </RouterLink>
</template>

<style scoped></style>
