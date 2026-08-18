<script setup lang="ts">
import { ref } from 'vue';
import AddressMapPanel from './modules/address-map-panel.vue';
import StationPointPanel from './modules/station-point-panel.vue';

defineOptions({ name: 'MasterDataPage' });

const activeTab = ref<'address' | 'station'>('address');
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px">
    <NAlert type="info" :bordered="false">
      维护 RCS 主数据：地址映射（TXC_AddressMaps）与工艺点位（TXC_StationPoints）。任务下发与筛选会读取此处启用的地址与口位。
    </NAlert>

    <NCard title="主数据维护" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <NTabs v-model:value="activeTab" type="line" animated>
        <NTabPane name="address" tab="地址映射">
          <AddressMapPanel />
        </NTabPane>
        <NTabPane name="station" tab="工艺点位">
          <StationPointPanel />
        </NTabPane>
      </NTabs>
    </NCard>
  </div>
</template>

<style scoped>
:deep(.n-tabs-pane-wrapper) {
  padding-top: 8px;
}
</style>
