<script setup lang="tsx">
import { reactive, ref } from 'vue';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { NButton, NPopconfirm, NTag, NTooltip } from 'naive-ui';
import {
  fetchCreateAddressMap,
  fetchDeleteAddressMap,
  fetchGetAddressMapList,
  fetchUpdateAddressMap
} from '@/service/api';
import { ADDRESS_MAP_FIELD_META } from '@/constants/address-map-fields';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import AddressMapDrawer from './address-map-drawer.vue';

defineOptions({ name: 'AddressMapPanel' });

const appStore = useAppStore();

const searchModel = reactive({
  keyword: null as string | null,
  enabledFilter: null as string | null
});

const query = reactive({ page: 1, pageSize: 10 });

function abpTransform(
  response: FlatResponseData<any, Api.MasterData.PagedList<Api.MasterData.AddressMapItem>>
): PaginationData<Api.MasterData.AddressMapItem> {
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
    fetchGetAddressMapList({
      page: query.page,
      pageSize: query.pageSize,
      keyword: searchModel.keyword,
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
      minWidth: 120
    },
    {
      key: 'tmTarget',
      title: ADDRESS_MAP_FIELD_META.tmTarget.label,
      align: 'center',
      width: 120,
      render: row => (
        <NTooltip>
          {{
            trigger: () => (
              <NTag size="small" type="default">
                站点 {row.tmTarget}
              </NTag>
            ),
            default: () => `${ADDRESS_MAP_FIELD_META.tmTarget.hint} · 当前值 ${row.tmTarget}`
          }}
        </NTooltip>
      )
    },
    {
      key: 'tmStorage',
      title: ADDRESS_MAP_FIELD_META.tmStorage.label,
      align: 'center',
      minWidth: 100,
      render: row => row.tmStorage || '—'
    },
    {
      key: 'remark',
      title: '备注',
      align: 'center',
      minWidth: 140,
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

async function handleDelete(id: string) {
  const { error } = await fetchDeleteAddressMap(id);
  if (error) return;
  window.$message?.success($t('common.deleteSuccess'));
  getData();
}

function handleSearch() {
  getDataByPage(1);
}

function handleReset() {
  searchModel.keyword = null;
  searchModel.enabledFilter = null;
  getDataByPage(1);
}

async function handleSubmitted(payload: {
  operateType: NaiveUI.TableOperateType;
  data: Api.MasterData.CreateAddressMap | Api.MasterData.UpdateAddressMap;
  id?: string;
}) {
  if (payload.operateType === 'add') {
    const { error } = await fetchCreateAddressMap(payload.data as Api.MasterData.CreateAddressMap);
    if (error) return;
    window.$message?.success($t('common.addSuccess'));
  } else {
    const { error } = await fetchUpdateAddressMap(
      payload.id!,
      payload.data as Api.MasterData.UpdateAddressMap
    );
    if (error) return;
    window.$message?.success($t('common.updateSuccess'));
  }
  drawerVisible.value = false;
  getData();
}

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
              placeholder="地址码 / 备注"
              @keyup.enter="handleSearch"
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
          <NFormItemGi span="24 s:24 m:10">
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
        :scroll-x="900"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-420px"
      />

      <AddressMapDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="handleSubmitted"
      />
    </NCard>
  </div>
</template>
