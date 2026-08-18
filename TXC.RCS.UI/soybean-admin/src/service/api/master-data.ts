import { request } from '../request';

function toAbpPage(params: { page?: number; pageSize?: number; sorting?: string }) {
  const page = params.page ?? 1;
  const pageSize = params.pageSize ?? 10;
  return {
    SkipCount: (page - 1) * pageSize,
    MaxResultCount: pageSize,
    Sorting: params.sorting
  };
}

export function fetchGetAddressMapList(
  params: Api.MasterData.AddressMapSearchParams & { page?: number; pageSize?: number }
) {
  return request<Api.MasterData.PagedList<Api.MasterData.AddressMapItem>>({
    url: '/api/app/address-map',
    method: 'get',
    params: {
      ...toAbpPage(params),
      Keyword: params.keyword || undefined,
      IsEnabled: params.isEnabled ?? undefined
    }
  });
}

export function fetchCreateAddressMap(data: Api.MasterData.CreateAddressMap) {
  return request<Api.MasterData.AddressMapItem>({
    url: '/api/app/address-map',
    method: 'post',
    data
  });
}

export function fetchUpdateAddressMap(id: string, data: Api.MasterData.UpdateAddressMap) {
  return request<Api.MasterData.AddressMapItem>({
    url: `/api/app/address-map/${encodeURIComponent(id)}`,
    method: 'put',
    data
  });
}

export function fetchDeleteAddressMap(id: string) {
  return request({
    url: `/api/app/address-map/${encodeURIComponent(id)}`,
    method: 'delete'
  });
}

export function fetchGetStationPointList(
  params: Api.MasterData.StationPointSearchParams & { page?: number; pageSize?: number }
) {
  return request<Api.MasterData.PagedList<Api.MasterData.StationPointItem>>({
    url: '/api/app/station-point',
    method: 'get',
    params: {
      ...toAbpPage(params),
      Keyword: params.keyword || undefined,
      AddressCode: params.addressCode || undefined,
      IsEnabled: params.isEnabled ?? undefined
    }
  });
}

export function fetchCreateStationPoint(data: Api.MasterData.CreateStationPoint) {
  return request<Api.MasterData.StationPointItem>({
    url: '/api/app/station-point',
    method: 'post',
    data
  });
}

export function fetchUpdateStationPoint(id: string, data: Api.MasterData.UpdateStationPoint) {
  return request<Api.MasterData.StationPointItem>({
    url: `/api/app/station-point/${encodeURIComponent(id)}`,
    method: 'put',
    data
  });
}

export function fetchDeleteStationPoint(id: string) {
  return request({
    url: `/api/app/station-point/${encodeURIComponent(id)}`,
    method: 'delete'
  });
}

/** 拉取全部启用的地址码（供任务表单下拉） */
export async function fetchEnabledAddressCodes() {
  const { data, error } = await fetchGetAddressMapList({
    page: 1,
    pageSize: 500,
    isEnabled: true
  });
  if (error || !data) return [] as string[];
  return [...new Set((data.items ?? []).map(x => x.addressCode))].sort();
}

/** 拉取某地址下启用的口位 */
export async function fetchEnabledPorts(addressCode: string) {
  const { data, error } = await fetchGetStationPointList({
    page: 1,
    pageSize: 200,
    addressCode,
    isEnabled: true
  });
  if (error || !data) return [] as string[];
  return (data.items ?? []).map(x => x.port).sort();
}
