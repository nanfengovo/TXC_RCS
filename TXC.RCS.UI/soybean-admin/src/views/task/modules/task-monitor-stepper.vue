<script setup lang="ts">
import { computed } from 'vue';
import { formatTaskTime } from './task-status';

defineOptions({ name: 'TaskMonitorStepper' });

interface Props {
  steps: Api.Task.TimelineStep[];
}

const props = defineProps<Props>();

const progressIndex = computed(() => {
  const list = props.steps;
  if (!list.length) return -1;
  const currentIdx = list.findIndex(s => s.status === 'current' || s.status === 'error');
  if (currentIdx >= 0) return currentIdx;
  const lastDone = [...list].reverse().findIndex(s => s.status === 'done');
  if (lastDone >= 0) return list.length - 1 - lastDone;
  return 0;
});

function stepClass(status: string) {
  return {
    'is-done': status === 'done',
    'is-current': status === 'current',
    'is-error': status === 'error',
    'is-canceled': status === 'canceled',
    'is-pending': status === 'pending'
  };
}

function lineActive(index: number, side: 'left' | 'right') {
  const pivot = progressIndex.value;
  if (side === 'left') return index > 0 && index - 1 <= pivot;
  return index < props.steps.length - 1 && index <= pivot;
}
</script>

<template>
  <div class="task-stepper">
    <div v-if="!steps.length" class="task-stepper__empty">暂无流程节点</div>
    <div v-else class="task-stepper__scroll">
      <div class="task-stepper__track">
        <div v-for="(step, index) in steps" :key="step.key" class="task-stepper__col">
          <div class="task-stepper__dot-row">
            <div
              class="task-stepper__line"
              :class="{ 'is-active': lineActive(index, 'left') }"
            />
            <div class="task-stepper__dot" :class="stepClass(step.status)" />
            <div
              class="task-stepper__line"
              :class="{ 'is-active': lineActive(index, 'right') }"
            />
          </div>
          <div class="task-stepper__label" :class="stepClass(step.status)">{{ step.label }}</div>
          <div class="task-stepper__time">{{ formatTaskTime(step.time) }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.task-stepper {
  --step-ease: cubic-bezier(0.2, 0.8, 0.2, 1);
  --step-accent: rgb(var(--primary-color));
  --step-muted: rgba(128, 128, 128, 0.45);
  min-height: 96px;
}

.task-stepper__empty {
  padding: 24px;
  text-align: center;
  font-size: 13px;
  opacity: 0.55;
}

.task-stepper__scroll {
  overflow-x: auto;
  overflow-y: hidden;
  padding-bottom: 4px;
  scrollbar-width: thin;
}

.task-stepper__track {
  display: flex;
  width: max-content;
  min-width: 100%;
  gap: 2px;
}

.task-stepper__col {
  min-width: 96px;
  flex: 1 0 96px;
}

.task-stepper__dot-row {
  display: flex;
  align-items: center;
}

.task-stepper__line {
  height: 2px;
  flex: 1;
  background: var(--step-muted);
  transition: background-color 0.35s var(--step-ease);
}

.task-stepper__line.is-active {
  background: var(--step-accent);
}

.task-stepper__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  margin: 0 6px;
  flex-shrink: 0;
  background: var(--step-muted);
  transition: transform 0.25s var(--step-ease), background-color 0.25s var(--step-ease);
}

.task-stepper__dot.is-done {
  background: #2ec08a;
}

.task-stepper__dot.is-current {
  background: var(--step-accent);
  transform: scale(1.15);
  animation: step-pulse 2s var(--step-ease) infinite;
}

.task-stepper__dot.is-error {
  background: #ff5f5f;
}

.task-stepper__dot.is-canceled {
  background: #f5a524;
}

.task-stepper__label {
  margin-top: 8px;
  font-size: 12px;
  line-height: 1.35;
  text-align: center;
  color: var(--n-text-color-2);
  transition: color 0.25s var(--step-ease);
}

.task-stepper__label.is-current {
  color: var(--n-text-color);
  font-weight: 600;
}

.task-stepper__label.is-error {
  color: #ff5f5f;
}

.task-stepper__time {
  margin-top: 2px;
  font-size: 11px;
  text-align: center;
  color: var(--n-text-color-3);
  font-variant-numeric: tabular-nums;
}

@keyframes step-pulse {
  0%,
  100% {
    box-shadow: 0 0 0 0 rgba(var(--primary-color), 0.35);
  }
  50% {
    box-shadow: 0 0 0 6px rgba(var(--primary-color), 0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .task-stepper__dot.is-current {
    animation: none;
  }

  .task-stepper__dot,
  .task-stepper__line,
  .task-stepper__label {
    transition: none;
  }
}
</style>
