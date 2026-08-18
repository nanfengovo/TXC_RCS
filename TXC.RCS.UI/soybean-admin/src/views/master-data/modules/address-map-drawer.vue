<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { ADDRESS_MAP_FIELD_META } from '@/constants/address-map-fields';
import { useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({ name: 'AddressMapDrawer' });

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData: Api.MasterData.AddressMapItem | null;
}

const props = defineProps<Props>();

interface Emits {
  (
    e: 'submitted',
    payload: {
      operateType: NaiveUI.TableOperateType;
      data: Api.MasterData.CreateAddressMap | Api.MasterData.UpdateAddressMap;
      id?: string;
    }
  ): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', { default: false });

const { formRef, validate, restoreValidation } = useNaiveForm();

interface FormModel {
  addressCode: string;
  tmTarget: number;
  tmStorage: string | null;
  remark: string | null;
  isEnabled: boolean;
}

function createDefault(): FormModel {
  return {
    addressCode: '',
    tmTarget: 1,
    tmStorage: null,
    remark: null,
    isEnabled: true
  };
}

const model = reactive<FormModel>(createDefault());

const isEdit = computed(() => props.operateType === 'edit');

const rules = {
  addressCode: { required: true, message: '请输入地址码', trigger: ['blur', 'input'] },
  tmTarget: {
    required: true,
    type: 'number' as const,
    message: `请输入${ADDRESS_MAP_FIELD_META.tmTarget.label}`,
    trigger: ['blur', 'change']
  }
};

function initModel() {
  if (props.operateType === 'edit' && props.rowData) {
    Object.assign(model, {
      addressCode: props.rowData.addressCode,
      tmTarget: props.rowData.tmTarget,
      tmStorage: props.rowData.tmStorage ?? null,
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
    tmTarget: model.tmTarget,
    tmStorage: model.tmStorage?.trim() || null,
    remark: model.remark?.trim() || null,
    isEnabled: model.isEnabled
  };

  if (isEdit.value) {
    emit('submitted', {
      operateType: 'edit',
      id: props.rowData!.id,
      data: payload
    });
  } else {
    emit('submitted', {
      operateType: 'add',
      data: {
        addressCode: model.addressCode.trim(),
        ...payload
      }
    });
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
  <NDrawer v-model:show="visible" display-directive="show" :width="480">
    <NDrawerContent :title="isEdit ? '编辑地址映射' : '新增地址映射'" :native-scrollbar="false" closable>
      <NAlert type="info" class="mb-12px" :bordered="false">
        地址码是 RCS 逻辑地址；{{ ADDRESS_MAP_FIELD_META.tmTarget.label }} 是下发 TM 时使用的目标站点编号，需与搬运系统配置一致。
      </NAlert>

      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
        <NFormItem label="地址码" path="addressCode">
          <NInput
            v-model:value="model.addressCode"
            :disabled="isEdit"
            placeholder="如 ERACK / H044 / H099"
          />
        </NFormItem>
        <NFormItem :label="ADDRESS_MAP_FIELD_META.tmTarget.label" path="tmTarget">
          <NInputNumber v-model:value="model.tmTarget" class="w-full" :min="1" />
          <template #feedback>
            <span class="text-12px opacity-60">{{ ADDRESS_MAP_FIELD_META.tmTarget.hint }}</span>
          </template>
        </NFormItem>
        <NFormItem :label="ADDRESS_MAP_FIELD_META.tmStorage.label" path="tmStorage">
          <NInput v-model:value="model.tmStorage" placeholder="可空" />
          <template #feedback>
            <span class="text-12px opacity-60">{{ ADDRESS_MAP_FIELD_META.tmStorage.hint }}</span>
          </template>
        </NFormItem>
        <NFormItem label="备注" path="remark">
          <NInput v-model:value="model.remark" type="textarea" :rows="2" placeholder="可空，如 Erack 1" />
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
