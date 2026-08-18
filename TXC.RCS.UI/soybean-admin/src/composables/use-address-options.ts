import { computed, ref } from 'vue';
import { fetchEnabledAddressCodes, fetchEnabledPorts } from '@/service/api';

/** 从主数据 API 加载地址码与口位，供任务筛选/下发表单使用 */
export function useAddressOptions() {
  const addressCodes = ref<string[]>([]);
  const loading = ref(false);

  async function loadAddresses() {
    loading.value = true;
    addressCodes.value = await fetchEnabledAddressCodes();
    loading.value = false;
  }

  const addressOptions = computed(() =>
    addressCodes.value.map(v => ({ label: v, value: v }))
  );

  async function loadPortOptions(addressCode: string) {
    if (!addressCode?.trim()) return [] as { label: string; value: string }[];
    const ports = await fetchEnabledPorts(addressCode.trim());
    return ports.map(v => ({ label: v, value: v }));
  }

  return {
    addressCodes,
    addressOptions,
    loading,
    loadAddresses,
    loadPortOptions
  };
}

/** 合并 API 地址与本地配置建议，去重排序 */
export function mergeAddressOptions(
  apiOptions: { label: string; value: string }[],
  localSuggestions: string[]
) {
  const set = new Set([...apiOptions.map(o => o.value), ...localSuggestions.filter(Boolean)]);
  return [...set].sort().map(v => ({ label: v, value: v }));
}
