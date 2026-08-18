declare namespace Api {
  namespace Task {
    type Source = 'Manual' | 'Mes' | (string & {});
    type LifecycleStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed' | 'Canceled' | (string & {});

    interface TaskSearchParams {
      keyword?: string | null;
      source?: string | null;
      lifecycleStatus?: string | null;
      fromAddress?: string | null;
      toAddress?: string | null;
      containerId?: string | null;
      lotId?: string | null;
    }

    interface TaskItem {
      id: string;
      source: Source;
      lotId?: string | null;
      lifecycleStatus: LifecycleStatus;
      waitingEvent?: string | null;
      activeLeg?: string | null;
      stepIndex: number;
      fetchTaskSerial?: string | null;
      putTaskSerial?: string | null;
      agvSerial?: string | null;
      fromAddress: string;
      fromPort?: string | null;
      toAddress?: string | null;
      toPort?: string | null;
      containerId?: string | null;
      fetchOptionCode: string;
      putOptionCode: string;
      optionCodeSchemaCode?: string | null;
      optionCodeSchemaVersion: number;
      lastError?: string | null;
      creationTime: string;
      lastModificationTime?: string | null;
    }

    interface TaskList {
      items: TaskItem[];
      totalCount: number;
    }

    interface OptionCodeField {
      key: string;
      label?: string | null;
      source?: string;
      enum?: Record<string, string> | null;
    }

    interface OptionCodePart {
      key: string;
      label?: string | null;
      width?: number;
      fields?: OptionCodeField[];
    }

    interface OptionCodeInput {
      key: string;
      source: string;
      required?: boolean;
      label?: string;
      min?: number | null;
      max?: number | null;
      enum?: Record<string, string> | null;
    }

    interface PublishedOptionCodeSchema {
      /** 后端 JSON 字段为 code */
      code?: string;
      schemaCode?: string;
      version: number;
      title?: string;
      parts?: OptionCodePart[];
      inputs: OptionCodeInput[];
    }

    interface CreateManualTask {
      fromAddress: string;
      fromPort?: string | null;
      toAddress: string;
      toPort?: string | null;
      containerId?: string | null;
      optionFields?: Record<string, number> | null;
    }

    interface CancelTask {
      id: string;
      reason?: string | null;
    }

    interface MesReportResult {
      accepted: boolean;
      message?: string | null;
    }

    interface TimelineStep {
      key: string;
      label: string;
      eventName?: string | null;
      leg?: string | null;
      status: string;
      time?: string | null;
    }

    interface InteractionLog {
      id: string;
      taskId: string;
      category: string;
      eventName: string;
      leg?: string | null;
      message?: string | null;
      detailJson?: string | null;
      success: boolean;
      creationTime: string;
    }

    interface MonitorDetail {
      task: TaskItem;
      timeline: TimelineStep[];
      logs: InteractionLog[];
    }
  }
}
