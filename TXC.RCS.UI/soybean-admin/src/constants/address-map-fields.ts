/** 地址映射字段说明（TM 下发参数） */
export const ADDRESS_MAP_FIELD_META = {
  tmTarget: {
    label: 'TM 站点号',
    hint: '下发搬运任务时传给 TM 的 Target 编号，与 RCS 地址码一一对应'
  },
  tmStorage: {
    label: 'TM 库位',
    hint: 'TM 侧 Storage 标识，多数场景可留空'
  }
} as const;
