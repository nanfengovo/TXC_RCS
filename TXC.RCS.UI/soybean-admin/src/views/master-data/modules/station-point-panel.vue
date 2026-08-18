<script setup lang="tsx">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { NButton, NPopconfirm, NTag } from 'naive-ui';
import {
  fetchCreateStationPoint,
  fetchDeleteStationPoint,
  fetchGetAddressMapList,
  fetchGetStationPointList,
  fetchUpdateStationPoint
} from '@/service/api';
import { useMasterFieldMeta } from '@/composables/use-master-field-meta';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import { renderLabeledMasterValue } from './master-value-display';
import StationPointDrawer from './station-point-drawer.vue';

defineOptions({ name: 'StationPointPanel' });

const appStore = useAppStore();
const masterMeta = useMasterFieldMeta();

onMounted(() => {
  masterMeta.load();
});

const addressOptions = ref<{ label: string; value: string }[]>([]);

async function loadAddressOptions() {
  const { data } = await fetchGetAddressMapList({ page: 1, pageSize: 500, isEnabled: true });
  const codes = [...new Set((data?.items ?? []).map(x => x.addressCode))].sort();
  addressOptions.value = codes.map(v => ({ label: v, value: v }));
}

const searchModel = reactive({
  keyword: null as string | null,
  addressCode: null as string | null,
  enabledFilter: null as string | null
});

const query = reactive({ page: 1, pageSize: 10 });

function masterRaw(row: Api.MasterData.StationPointItem, key: string) {
  return row.masterValues?.[key];
}

function renderMaster(key: string, row: Api.MasterData.StationPointItem) {
  return renderLabeledMasterValue(
    masterMeta.formatValue,
    masterMeta.formatValueWithCode,
    masterMeta.hasEnum,
    key,
    masterRaw(row, key)
  );
}

function abpTransform(
  response: FlatResponseData<any, Api.MasterData.PagedList<Api.MasterData.StationPointItem>>
): PaginationData<Api.MasterData.StationPointItem> {
  const page = response.data;
  return {
    data: page?.items ?? [],
    pageNum: query.page,
    pageSize: query.pageSize,
    total: page?.totalCount ?? 0
  };
}

const {
  columns,
  columnChecks,
  data,
  getData,
  getDataByPage,
  loading,
  mobilePagination
} = useNaivePaginatedTable({
  api: () =>
    fetchGetStationPointList({
      page: query.page,
      pageSize: query.pageSize,
      keyword: searchModel.keyword,
      addressCode: searchModel.addressCode,
      isEnabled:
        searchModel.enabledFilter === '1'
          ? true
          : searchModel.enabledFilter === '0'
            ? false
            : null
    }),
  transform: abpTransform,
  onPaginationParamsChange: params => {
    query.page = params.page ?? 1;
    query.pageSize = params.pageSize ?? 10;
  },
  columns: () => [
    {
      key: 'index',
      title: $t('common.index'),
      align: 'center',
      width: 56,
      render: (_, index) => index + 1
    },
    {
      key: 'addressCode',
      title: '地址码',
      align: 'center',
      minWidth: 100
    },
    {
      key: 'port',
      title: '口位',
      align: 'center',
      width: 72
    },
    {
      key: 'armSide',
      title: '机械臂运行侧',
      align: 'center',
      width: 120,
      render: row => renderMaster('armSide', row)
    },
    {
      key: 'equipmentType',
      title: '设备类型',
      align: 'center',
      width: 130,
      render: row => renderMaster('equipmentType', row)
    },
    {
      key: 'machineNo',
      title: '机台编号',
      align: 'center',
      width: 100,
      render: row => renderMaster('machineNo', row)
    },
    {
      key: 'remark',
      title: '备注',
      align: 'center',
      minWidth: 120,
      ellipsis: { tooltip: true },
      render: row => row.remark || '—'
    },
    {
      key: 'isEnabled',
      title: '启用',
      align: 'center',
      width: 80,
      render: row => (
        <NTag size="small" type={row.isEnabled ? 'success' : 'default'}>
          {row.isEnabled ? '是' : '否'}
        </NTag>
      )
    },
    {
      key: 'operate',
      title: $t('common.operate'),
      align: 'center',
      width: 160,
      fixed: 'right',
      render: row => (
        <div class="flex-center gap-8px">
          <NButton type="primary" ghost size="small" onClick={() => handleEdit(row.id)}>
            {$t('common.edit')}
          </NButton>
          <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="small">
                  {$t('common.delete')}
                </NButton>
              )
            }}
          </NPopconfirm>
        </div>
      )
    }
  ]
});

const { drawerVisible, operateType, editingData, handleAdd, handleEdit } = useTableOperate(
  data,
  'id',
  getData
);

const addressOptionsForForm = computed(() => addressOptions.value);
const masterFieldMeta = computed(() => masterMeta.fields.value);

async function handleDelete(id: string) {
  const { error } = await fetchDeleteStationPoint(id);
  if (error) return;
  window.$message?.success($t('common.deleteSuccess'));
  getData();
}

function handleSearch() {
  getDataByPage(1);
}

function handleReset() {
  searchModel.keyword = null;
  searchModel.addressCode = null;
  searchModel.enabledFilter = null;
  getDataByPage(1);
}

async function handleSubmitted(payload: {
  operateType: NaiveUI.TableOperateType;
  data: Api.MasterData.CreateStationPoint | Api.MasterData.UpdateStationPoint;
  id?: string;
}) {
  if (payload.operateType === 'add') {
    const { error } = await fetchCreateStationPoint(payload.data as Api.MasterData.CreateStationPoint);
    if (error) return;
    window.$message?.success($t('common.addSuccess'));
  } else {
    const { error } = await fetchUpdateStationPoint(
      payload.id!,
      payload.data as Api.MasterData.UpdateStationPoint
    );
    if (error) return;
    window.$message?.success($t('common.updateSuccess'));
  }
  drawerVisible.value = false;
  getData();
}

watch(
  () => drawerVisible.value,
  open => {
    if (open) loadAddressOptions();
  }
);

loadAddressOptions();

defineExpose({ refresh: getData });
</script>

<template>
  <div class="flex-col-stretch gap-12px">
    <NCard :bordered="false" size="small" class="card-wrapper">
      <NForm label-placement="left" :label-width="72" :show-feedback="false">
        <NGrid responsive="screen" item-responsive :x-gap="12">
          <NFormItemGi span="24 s:12 m:8" label="关键词">
            <NInput
              v-model:value="searchModel.keyword"
              clearable
              placeholder="地址 / 口位 / 备注"
              @keyup.enter="handleSearch"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="地址码">
            <NSelect
              v-model:value="searchModel.addressCode"
              clearable
              filterable
              :options="addressOptions"
              placeholder="全部"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:12 m:6" label="启用">
            <NSelect
              v-model:value="searchModel.enabledFilter"
              clearable
              :options="[
                { label: '启用', value: '1' },
                { label: '停用', value: '0' }
              ]"
              placeholder="全部"
            />
          </NFormItemGi>
          <NFormItemGi span="24 s:24 m:4">
            <NSpace>
              <NButton type="primary" ghost size="small" @click="handleSearch">查询</NButton>
              <NButton size="small" @click="handleReset">{{ $t('common.reset') }}</NButton>
            </NSpace>
          </NFormItemGi>
        </NGrid>
      </NForm>
    </NCard>

    <NCard :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header-extra>
        <TableHeaderOperation v-model:columns="columnChecks" :loading="loading" @add="handleAdd" @refresh="getData">
          <template #default>
            <NButton type="primary" size="small" @click="handleAdd">
              <template #icon>
                <icon-ic-round-plus class="text-icon" />
              </template>
              {{ $t('common.add') }}
            </NButton>
          </template>
          <template #suffix>
            <span />
          </template>
        </TableHeaderOperation>
      </template>

      <NDataTable
        :columns="columns"
        :data="data"
        size="small"
        :flex-height="!appStore.isMobile"
        :scroll-x="1020"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-420px"
      />

      <StationPointDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        :address-options="addressOptionsForForm"
        :master-fields="masterFieldMeta"
        @submitted="handleSubmitted"
      />
    </NCard>
  </div>
</template>
