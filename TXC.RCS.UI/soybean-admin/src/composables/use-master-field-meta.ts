import { ref } from 'vue';
import { fetchGetOptionCodeSchema } from '@/service/api';

export interface MasterFieldMeta {
  key: string;
  label: string;
  enum?: Record<string, string>;
  hint?: string;
}

/** 与后端 txc_demo.v1.json 保持一致，API 不可用时兜底 */
const FALLBACK_MASTER_FIELDS: MasterFieldMeta[] = [
  {
    key: 'armSide',
    label: '机械臂运行侧',
    enum: { '1': '左侧', '2': '右侧' },
    hint: 'TaskCode 中机械臂从哪一侧取放'
  },
  {
    key: 'equipmentType',
    label: '设备类型',
    enum: { '1': 'Rack', '2': 'H099机台', '3': 'H044机台' },
    hint: 'TaskCode 中目标设备类型编码'
  },
  {
    key: 'machineNo',
    label: '机台编号',
    hint: 'TaskCode 机台编号位段，与现场机台序号对应'
  }
];

export function useMasterFieldMeta() {
  const fields = ref<MasterFieldMeta[]>([...FALLBACK_MASTER_FIELDS]);
  const loaded = ref(false);
  const loading = ref(false);

  async function load() {
    if (loaded.value || loading.value) return;
    loading.value = true;
    const { data, error } = await fetchGetOptionCodeSchema();
    loading.value = false;
    if (!error && data?.parts?.length) {
      const masterFields: MasterFieldMeta[] = [];
      for (const part of data.parts as Api.Task.OptionCodePart[]) {
        for (const field of part.fields ?? []) {
          if (field.source !== 'master') continue;
          const fallback = FALLBACK_MASTER_FIELDS.find(f => f.key === field.key);
          masterFields.push({
            key: field.key,
            label: field.label || field.key,
            enum: field.enum ?? undefined,
            hint: fallback?.hint
          });
        }
      }
      if (masterFields.length) fields.value = masterFields;
    }
    loaded.value = true;
  }

  function getField(key: string) {
    return fields.value.find(f => f.key === key);
  }

  function getLabel(key: string) {
    return getField(key)?.label ?? key;
  }

  function getHint(key: string) {
    return getField(key)?.hint;
  }

  /** 仅语义标签，如「左侧」 */
  function formatValue(key: string, value: number | string | null | undefined) {
    if (value === null || value === undefined || value === '') return '—';
    const str = String(value);
    return getField(key)?.enum?.[str] ?? str;
  }

  /** 语义 + 编码，如「左侧（1）」 */
  function formatValueWithCode(key: string, value: number | string | null | undefined) {
    if (value === null || value === undefined || value === '') return '—';
    const str = String(value);
    const enumLabel = getField(key)?.enum?.[str];
    return enumLabel ? `${enumLabel}（${str}）` : str;
  }

  function hasEnum(key: string) {
    const field = getField(key);
    return Boolean(field?.enum && Object.keys(field.enum).length);
  }

  function getSelectOptions(key: string) {
    const field = getField(key);
    if (!field?.enum) return [] as { label: string; value: number }[];
    return Object.entries(field.enum).map(([value, label]) => ({
      label: `${label}（${value}）`,
      value: Number(value)
    }));
  }

  return {
    fields,
    loading,
    load,
    getField,
    getLabel,
    getHint,
    formatValue,
    formatValueWithCode,
    hasEnum,
    getSelectOptions
  };
}
