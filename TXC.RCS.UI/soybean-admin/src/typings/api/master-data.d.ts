declare namespace Api {
  namespace MasterData {
    interface PagedList<T> {
      items: T[];
      totalCount: number;
    }

    interface AddressMapItem {
      id: string;
      addressCode: string;
      tmTarget: number;
      tmStorage?: string | null;
      remark?: string | null;
      isEnabled: boolean;
    }

    interface AddressMapSearchParams {
      keyword?: string | null;
      isEnabled?: boolean | null;
    }

    interface CreateAddressMap {
      addressCode: string;
      tmTarget: number;
      tmStorage?: string | null;
      remark?: string | null;
      isEnabled?: boolean;
    }

    interface UpdateAddressMap {
      tmTarget: number;
      tmStorage?: string | null;
      remark?: string | null;
      isEnabled?: boolean;
    }

    interface StationPointItem {
      id: string;
      addressCode: string;
      port: string;
      masterValues: Record<string, number>;
      remark?: string | null;
      isEnabled: boolean;
    }

    interface StationPointSearchParams {
      keyword?: string | null;
      addressCode?: string | null;
      isEnabled?: boolean | null;
    }

    interface CreateStationPoint {
      addressCode: string;
      port: string;
      masterValues?: Record<string, number> | null;
      remark?: string | null;
      isEnabled?: boolean;
    }

    interface UpdateStationPoint {
      addressCode: string;
      port: string;
      masterValues?: Record<string, number> | null;
      remark?: string | null;
      isEnabled?: boolean;
    }
  }
}
