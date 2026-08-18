<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { fetchGetOptionCodeSchema } from '@/service/api';
import { useRcsConfigStore } from '@/store/modules/rcs-config';
import type { RcsRuntimeConfig } from '@/constants/rcs-config';
import { DEFAULT_RCS_CONFIG } from '@/constants/rcs-config';

defineOptions({ name: 'SettingsPage' });

const rcsConfigStore = useRcsConfigStore();
const { config } = storeToRefs(rcsConfigStore);

const form = reactive<RcsRuntimeConfig>({ ...config.value });
const schemaInfo = ref<{ code: string; version: number } | null>(null);

const timeWindowOptions = [
  { label: '12 小时', value: '12h' },
  { label: '24 小时', value: '24h' },
  { label: '72 小时', value: '72h' }
];

const fromSuggestionsText = computed({
  get: () => form.fromAddressSuggestions.join('\n'),
  set: (v: string) => {
    form.fromAddressSuggestions = v
      .split('\n')
      .map(s => s.trim())
      .filter(Boolean);
  }
});

const toSuggestionsText = computed({
  get: () => form.toAddressSuggestions.join('\n'),
  set: (v: string) => {
    form.toAddressSuggestions = v
      .split('\n')
      .map(s => s.trim())
      .filter(Boolean);
  }
});

async function loadSchema() {
  const { data } = await fetchGetOptionCodeSchema();
  if (data) {
    schemaInfo.value = { code: data.code ?? data.schemaCode ?? '', version: data.version };
  }
}

function save() {
  rcsConfigStore.update({ ...form });
  window.$message?.success('配置已保存');
}

function reset() {
  Object.assign(form, DEFAULT_RCS_CONFIG);
  rcsConfigStore.reset();
  window.$message?.success('已恢复默认配置');
}

onMounted(loadSchema);
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px">
    <NAlert type="info" :bordered="false">
      此处配置轮询间隔、工艺区域、常用地址等业务参数，保存在浏览器本地。OptionCode Schema 仍由后端发布。
      <template v-if="schemaInfo">
        当前 Schema：<code>{{ schemaInfo.code }} v{{ schemaInfo.version }}</code>
      </template>
    </NAlert>

    <NCard title="RCS 运行配置" :bordered="false" size="small" class="card-wrapper">
      <NForm label-placement="left" label-width="140">
        <NGrid responsive="screen" item-responsive :x-gap="16">
          <NFormItemGi span="24 s:12 m:8" label="仪表盘轮询 (ms)">
            <NInputNumber v-model:value="form.dashboardPollMs" :min="3000" :max="120000" class="w-full" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="任务列表轮询 (ms)">
            <NInputNumber v-model:value="form.taskListPollMs" :min="3000" :max="120000" class="w-full" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="监控轮询 (ms)">
            <NInputNumber v-model:value="form.taskMonitorPollMs" :min="2000" :max="60000" class="w-full" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="默认统计窗">
            <NSelect v-model:value="form.dashboardTimeWindow" :options="timeWindowOptions" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="工艺区域">
            <NInput v-model:value="form.processArea" placeholder="如 FAB-AMHS" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="监控抽屉宽度">
            <NInputNumber v-model:value="form.taskMonitorWidth" :min="360" :max="720" class="w-full" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="搜索默认折叠">
            <NSwitch v-model:value="form.taskSearchCollapsed" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="默认起点">
            <NInput v-model:value="form.defaultFromAddress" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="默认终点">
            <NInput v-model:value="form.defaultToAddress" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="默认登录用户">
            <NInput v-model:value="form.defaultLoginUser" placeholder="admin" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:8" label="默认登录密码">
            <NInput v-model:value="form.defaultLoginPassword" type="password" show-password-on="click" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:12" label="起点建议 (每行一个)">
            <NInput v-model:value="fromSuggestionsText" type="textarea" :rows="4" />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:12" label="终点建议 (每行一个)">
            <NInput v-model:value="toSuggestionsText" type="textarea" :rows="4" />
          </NFormItemGi>
        </NGrid>

        <NSpace class="mt-16px">
          <NButton type="primary" @click="save">保存</NButton>
          <NButton @click="reset">恢复默认</NButton>
        </NSpace>
      </NForm>
    </NCard>
  </div>
</template>

<style scoped>
code {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}
</style>
