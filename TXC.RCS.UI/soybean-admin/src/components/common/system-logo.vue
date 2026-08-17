<script setup lang="ts">
import logoUrl from '@/assets/imgs/logo.svg';

defineOptions({
  name: 'SystemLogo'
});
</script>

<template>
  <img :src="logoUrl" alt="TXC RCS" class="txc-logo block max-h-full max-w-full object-contain object-left" />
</template>

<style scoped>
.txc-logo {
  height: inherit;
  width: auto;
}
</style>
