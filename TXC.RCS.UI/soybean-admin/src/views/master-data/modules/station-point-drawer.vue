<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import type { MasterFieldMeta } from '@/composables/use-master-field-meta';
import { useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({ name: 'StationPointDrawer' });

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData: Api.MasterData.StationPointItem | null;
  addressOptions: { label: string; value: string }[];
  masterFields: MasterFieldMeta[];
}

const props = defineProps<Props>();

interface Emits {
  (
    e: 'submitted',
    payload: {
      operateType: NaiveUI.TableOperateType;
      data: Api.MasterData.CreateStationPoint | Api.MasterData.UpdateStationPoint;
      id?: string;
    }
  ): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', { default: false });

const { formRef, validate, restoreValidation } = useNaiveForm();

interface FormModel {
  addressCode: string;
  port: string;
  armSide: number | null;
  equipmentType: number | null;
  machineNo: number | null;
  remark: string | null;
  isEnabled: boolean;
}

function createDefault(): FormModel {
  return {
    addressCode: '',
    port: '',
    armSide: null,
    equipmentType: null,
    machineNo: null,
    remark: null,
    isEnabled: true
  };
}

const model = reactive<FormModel>(createDefault());

const isEdit = computed(() => props.operateType === 'edit');

const rules = {
  addressCode: { required: true, message: '请选择地址码', trigger: ['blur', 'change'] },
  port: { required: true, message: '请输入口位', trigger: ['blur', 'input'] }
};

function fieldMeta(key: string) {
  return props.masterFields.find(f => f.key === key);
}

function fieldLabel(key: string) {
  return fieldMeta(key)?.label ?? key;
}

function fieldHint(key: string) {
  return fieldMeta(key)?.hint;
}

function enumOptions(key: string) {
  const field = fieldMeta(key);
  if (!field?.enum) return [];
  return Object.entries(field.enum).map(([value, label]) => ({
    label: `${label}（${value}）`,
    value: Number(value)
  }));
}

function buildMasterValues(): Record<string, number> {
  const values: Record<string, number> = {};
  if (model.armSide !== null && !Number.isNaN(model.armSide)) values.armSide = model.armSide;
  if (model.equipmentType !== null && !Number.isNaN(model.equipmentType))
    values.equipmentType = model.equipmentType;
  if (model.machineNo !== null && !Number.isNaN(model.machineNo)) values.machineNo = model.machineNo;
  return values;
}

function initModel() {
  if (props.operateType === 'edit' && props.rowData) {
    const mv = props.rowData.masterValues ?? {};
    Object.assign(model, {
      addressCode: props.rowData.addressCode,
      port: props.rowData.port,
      armSide: mv.armSide ?? null,
      equipmentType: mv.equipmentType ?? null,
      machineNo: mv.machineNo ?? null,
      remark: props.rowData.remark ?? null,
      isEnabled: props.rowData.isEnabled
    });
  } else {
    Object.assign(model, createDefault());
  }
}

async function handleSubmit() {
  await validate();
  const payload = {
    addressCode: model.addressCode.trim(),
    port: model.port.trim(),
    masterValues: buildMasterValues(),
    remark: model.remark?.trim() || null,
    isEnabled: model.isEnabled
  };

  if (isEdit.value) {
    emit('submitted', { operateType: 'edit', id: props.rowData!.id, data: payload });
  } else {
    emit('submitted', { operateType: 'add', data: payload });
  }
}

watch(visible, val => {
  if (val) {
    initModel();
    restoreValidation();
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="520">
    <NDrawerContent :title="isEdit ? '编辑工艺点位' : '新增工艺点位'" :native-scrollbar="false" closable>
      <NAlert type="info" class="mb-12px" :bordered="false">
        以下参数写入 TaskCode 位段，由 OptionCode Schema 定义含义；有下拉项的字段请选择语义值，系统会自动保存对应编码。
      </NAlert>

      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="112">
        <NFormItem label="地址码" path="addressCode">
          <NSelect
            v-model:value="model.addressCode"
            filterable
            tag
            :options="addressOptions"
            placeholder="选择或输入地址码"
          />
        </NFormItem>
        <NFormItem label="口位" path="port">
          <NInput v-model:value="model.port" placeholder="设备库位编号，如 1 / 2" />
        </NFormItem>

        <NDivider title-placement="left">TaskCode 主数据</NDivider>

        <NFormItem :label="fieldLabel('armSide')" path="armSide">
          <NSelect
            v-model:value="model.armSide"
            clearable
            :options="enumOptions('armSide')"
            placeholder="选择机械臂运行侧"
          />
          <template v-if="fieldHint('armSide')" #feedback>
            <span class="text-12px opacity-60">{{ fieldHint('armSide') }}</span>
          </template>
        </NFormItem>

        <NFormItem :label="fieldLabel('equipmentType')" path="equipmentType">
          <NSelect
            v-model:value="model.equipmentType"
            clearable
            :options="enumOptions('equipmentType')"
            placeholder="选择设备类型"
          />
          <template v-if="fieldHint('equipmentType')" #feedback>
            <span class="text-12px opacity-60">{{ fieldHint('equipmentType') }}</span>
          </template>
        </NFormItem>

        <NFormItem :label="fieldLabel('machineNo')" path="machineNo">
          <NInputNumber v-model:value="model.machineNo" class="w-full" clearable :min="1" :max="255" />
          <template v-if="fieldHint('machineNo')" #feedback>
            <span class="text-12px opacity-60">{{ fieldHint('machineNo') }}</span>
          </template>
        </NFormItem>

        <NFormItem label="备注" path="remark">
          <NInput v-model:value="model.remark" type="textarea" :rows="2" placeholder="可空，如 Erack 口1" />
        </NFormItem>
        <NFormItem label="启用" path="isEnabled">
          <NSwitch v-model:value="model.isEnabled" />
        </NFormItem>
      </NForm>

      <template #footer>
        <NSpace justify="end">
          <NButton @click="visible = false">{{ $t('common.cancel') }}</NButton>
          <NButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</NButton>
        </NSpace>
      </template>
    </NDrawerContent>
  </NDrawer>
</template>
