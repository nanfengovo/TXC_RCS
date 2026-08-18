import { NTag, NTooltip } from 'naive-ui';

/** 表格单元格：有枚举时显示语义标签，悬停可见编码 */
export function renderLabeledMasterValue(
  formatValue: (key: string, value: number | string | null | undefined) => string,
  formatValueWithCode: (key: string, value: number | string | null | undefined) => string,
  hasEnum: (key: string) => boolean,
  key: string,
  raw: number | string | null | undefined
) {
  if (raw === null || raw === undefined || raw === '') {
    return '—';
  }

  const label = formatValue(key, raw);
  const full = formatValueWithCode(key, raw);

  if (hasEnum(key) && label !== String(raw)) {
    return (
      <NTooltip>
        {{
          trigger: () => (
            <NTag size="small" type="info">
              {label}
            </NTag>
          ),
          default: () => `编码 ${raw} · ${full}`
        }}
      </NTooltip>
    );
  }

  return full;
}
