import { ref, watch } from 'vue';
import { defineStore } from 'pinia';
import { defu } from 'defu';
import { DEFAULT_RCS_CONFIG, RCS_CONFIG_STORAGE_KEY, type RcsRuntimeConfig } from '@/constants/rcs-config';
import { SetupStoreId } from '@/enum';
import { localStg } from '@/utils/storage';

function loadConfig(): RcsRuntimeConfig {
  const stored = localStg.get(RCS_CONFIG_STORAGE_KEY) as Partial<RcsRuntimeConfig> | null;
  return defu(stored ?? {}, DEFAULT_RCS_CONFIG) as RcsRuntimeConfig;
}

export const useRcsConfigStore = defineStore(SetupStoreId.RcsConfig, () => {
  const config = ref<RcsRuntimeConfig>(loadConfig());

  function persist() {
    localStg.set(RCS_CONFIG_STORAGE_KEY, config.value);
  }

  function update(partial: Partial<RcsRuntimeConfig>) {
    config.value = { ...config.value, ...partial };
    persist();
  }

  function reset() {
    config.value = { ...DEFAULT_RCS_CONFIG };
    persist();
  }

  watch(config, persist, { deep: true });

  return {
    config,
    update,
    reset
  };
});
