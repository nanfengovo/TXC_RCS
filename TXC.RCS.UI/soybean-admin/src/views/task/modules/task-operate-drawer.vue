<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { useNaiveForm } from '@/hooks/common/form';
import { fetchCreateManualTask, fetchGetOptionCodeSchema } from '@/service/api';
import { $t } from '@/locales';

defineOptions({ name: 'TaskOperateDrawer' });

interface Props {
  operateType: NaiveUI.TableOperateType;
}

defineProps<Props>();

interface Emits {
  (e: 'submitted'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', { default: false });

const { formRef, validate, restoreValidation } = useNaiveForm();

interface FormModel {
  fromAddress: string;
  fromPort: string | null;
  toAddress: string;
  toPort: string | null;
  containerId: string | null;
  optionFields: Record<string, number | null>;
}

const schema = reactive<{ loading: boolean; inputs: Api.Task.OptionCodeInput[] }>({
  loading: false,
  inputs: []
});

function createDefaultModel(): FormModel {
  return {
    fromAddress: '',
    fromPort: null,
    toAddress: '',
    toPort: null,
    containerId: null,
    optionFields: {}
  };
}

const model = reactive<FormModel>(createDefaultModel());

const argsInputs = computed(() => schema.inputs.filter(i => i.source === 'args'));

const rules = {
  fromAddress: { required: true, message: '请输入起点地址', trigger: ['blur', 'input'] },
  toAddress: { required: true, message: '请输入终点地址', trigger: ['blur', 'input'] }
};

async function loadSchema() {
  schema.loading = true;
  const { data, error } = await fetchGetOptionCodeSchema();
  schema.loading = false;
  if (error) return;
  schema.inputs = data?.inputs ?? [];
  for (const input of argsInputs.value) {
    if (!(input.key in model.optionFields)) {
      model.optionFields[input.key] = null;
    }
  }
}

function handleInitModel() {
  const next = createDefaultModel();
  model.fromAddress = next.fromAddress;
  model.fromPort = next.fromPort;
  model.toAddress = next.toAddress;
  model.toPort = next.toPort;
  model.containerId = next.containerId;
  model.optionFields = {};
}

async function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();
  const optionFields: Record<string, number> = {};
  for (const [k, v] of Object.entries(model.optionFields)) {
    if (v === null || v === undefined || Number.isNaN(Number(v))) continue;
    optionFields[k] = Number(v);
  }

  const payload: Api.Task.CreateManualTask = {
    fromAddress: model.fromAddress.trim(),
    fromPort: model.fromPort?.toString().trim() || null,
    toAddress: model.toAddress.trim(),
    toPort: model.toPort?.toString().trim() || null,
    containerId: model.containerId?.toString().trim() || null,
    optionFields: Object.keys(optionFields).length ? optionFields : null
  };

  const { error } = await fetchCreateManualTask(payload);
  if (error) return;

  window.$message?.success($t('common.addSuccess'));
  closeDrawer();
  emit('submitted');
}

watch(visible, async val => {
  if (val) {
    handleInitModel();
    restoreValidation();
    await loadSchema();
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="520">
    <NDrawerContent title="下发搬运任务" :native-scrollbar="false" closable>
      <NSpin :show="schema.loading">
        <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
          <NFormItem label="起点地址" path="fromAddress">
            <NInput v-model:value="model.fromAddress" placeholder="ERACK / H044 / H099" />
          </NFormItem>
          <NFormItem label="起点口" path="fromPort">
            <NInput v-model:value="model.fromPort" placeholder="设备库位，如 1" />
          </NFormItem>
          <NFormItem label="终点地址" path="toAddress">
            <NInput v-model:value="model.toAddress" placeholder="H044 / H099" />
          </NFormItem>
          <NFormItem label="终点口" path="toPort">
            <NInput v-model:value="model.toPort" placeholder="设备库位，如 1" />
          </NFormItem>
          <NFormItem label="料盒号" path="containerId">
            <NInput v-model:value="model.containerId" placeholder="可空" />
          </NFormItem>

          <NDivider title-placement="left">TaskCode 可选字段</NDivider>
          <NAlert type="info" class="mb-12px" :bordered="false">
            臂侧由点位表解析，AGV 库位恒为 0，无需填写。
          </NAlert>

          <NFormItem v-for="item in argsInputs" :key="item.key" :label="item.label || item.key">
            <NInputNumber
              v-if="!item.enum"
              v-model:value="model.optionFields[item.key]"
              class="w-full"
              :min="item.min ?? undefined"
              :max="item.max ?? undefined"
              clearable
            />
            <NSelect
              v-else
              v-model:value="model.optionFields[item.key]"
              clearable
              :options="
                Object.entries(item.enum || {}).map(([value, label]) => ({
                  label: `${value} - ${label}`,
                  value: Number(value)
                }))
              "
            />
          </NFormItem>
        </NForm>
      </NSpin>

      <template #footer>
        <NSpace justify="end">
          <NButton @click="closeDrawer">{{ $t('common.cancel') }}</NButton>
          <NButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</NButton>
        </NSpace>
      </template>
    </NDrawerContent>
  </NDrawer>
</template>
