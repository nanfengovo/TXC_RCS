<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import gsap from 'gsap';

defineOptions({ name: 'AmhsLoginScene' });

defineProps<{
  dark: boolean;
}>();

const sceneRef = ref<HTMLElement | null>(null);
const amrRefs = ref<HTMLElement[]>([]);
let ctx: gsap.Context | null = null;

function setAmrRef(el: unknown, index: number) {
  if (el instanceof HTMLElement) {
    amrRefs.value[index] = el;
  }
}

onMounted(() => {
  if (!sceneRef.value) return;

  ctx = gsap.context(() => {
    gsap.to('.amhs-track__pulse', {
      x: '100%',
      duration: 3.2,
      repeat: -1,
      ease: 'none'
    });

    gsap.to('.amhs-node', {
      opacity: 0.35,
      duration: 1.6,
      yoyo: true,
      repeat: -1,
      stagger: 0.4,
      ease: 'sine.inOut'
    });

    amrRefs.value.forEach((el, i) => {
      gsap.to(el, {
        x: i % 2 === 0 ? 120 : -120,
        duration: 4 + i * 0.6,
        repeat: -1,
        yoyo: true,
        ease: 'sine.inOut'
      });
    });
  }, sceneRef.value);
});

onBeforeUnmount(() => {
  ctx?.revert();
});
</script>

<template>
  <div ref="sceneRef" class="amhs-scene" :class="{ 'is-dark': dark, 'is-light': !dark }" aria-hidden="true">
    <div class="amhs-scene__grid" />
    <div class="amhs-scene__glow" />

    <div class="amhs-scene__content">
      <div class="amhs-scene__badge">AMHS · OHT · AMR</div>
      <h1 class="amhs-scene__headline">半导体厂内<br />智能搬运调度</h1>
      <p class="amhs-scene__desc">RCS 统一调度 MES 派工、TM 任务与 AMR/AGV 协同，保障洁净室物料闭环流转。</p>

      <div class="amhs-tracks">
        <div v-for="i in 3" :key="i" class="amhs-track">
          <div class="amhs-track__rail" />
          <div class="amhs-track__pulse" />
          <div :ref="el => setAmrRef(el, i - 1)" class="amhs-amr">
            <span class="amhs-amr__body" />
            <span class="amhs-amr__label">AMR-{{ i }}</span>
          </div>
        </div>
      </div>

      <div class="amhs-nodes">
        <div v-for="node in ['STK', 'EQP', 'ERACK', 'OHT']" :key="node" class="amhs-node">
          {{ node }}
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.amhs-scene {
  position: relative;
  width: 100%;
  height: 100%;
  overflow: hidden;
  transition:
    background 0.25s ease,
    color 0.25s ease;
}

.amhs-scene.is-dark {
  background: linear-gradient(145deg, #050a0f 0%, #0a1520 45%, #061018 100%);
  color: #e8f4f8;
}

.amhs-scene.is-light {
  background: linear-gradient(145deg, #f8fbff 0%, #eef6fb 45%, #eaf3f9 100%);
  color: #102a43;
}

.amhs-scene__grid {
  position: absolute;
  inset: 0;
  background-size: 32px 32px;
  mask-image: radial-gradient(ellipse 80% 70% at 50% 50%, black, transparent);
}

.amhs-scene.is-dark .amhs-scene__grid {
  background-image:
    linear-gradient(rgb(45 212 191 / 6%) 1px, transparent 1px),
    linear-gradient(90deg, rgb(45 212 191 / 6%) 1px, transparent 1px);
}

.amhs-scene.is-light .amhs-scene__grid {
  background-image:
    linear-gradient(rgb(13 148 136 / 8%) 1px, transparent 1px),
    linear-gradient(90deg, rgb(13 148 136 / 8%) 1px, transparent 1px);
}

.amhs-scene__glow {
  position: absolute;
  width: 420px;
  height: 420px;
  top: 20%;
  left: 10%;
  border-radius: 50%;
  filter: blur(40px);
  pointer-events: none;
}

.amhs-scene.is-dark .amhs-scene__glow {
  background: radial-gradient(circle, rgb(45 212 191 / 18%), transparent 70%);
}

.amhs-scene.is-light .amhs-scene__glow {
  background: radial-gradient(circle, rgb(45 212 191 / 20%), transparent 72%);
}

.amhs-scene__content {
  position: relative;
  z-index: 1;
  padding: 48px 40px;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.amhs-scene__badge {
  display: inline-flex;
  align-self: flex-start;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 11px;
  letter-spacing: 0.08em;
  border: 1px solid rgb(45 212 191 / 35%);
  color: #2dd4bf;
  background: rgb(45 212 191 / 8%);
}

.amhs-scene__headline {
  margin: 16px 0 0;
  font-size: clamp(28px, 3.2vw, 40px);
  font-weight: 700;
  line-height: 1.15;
  letter-spacing: -0.02em;
}

.amhs-scene__desc {
  margin: 14px 0 0;
  max-width: 420px;
  font-size: 14px;
  line-height: 1.6;
}

.amhs-scene.is-dark .amhs-scene__desc {
  color: rgb(232 244 248 / 72%);
}

.amhs-scene.is-light .amhs-scene__desc {
  color: rgb(16 42 67 / 72%);
}

.amhs-tracks {
  margin-top: 36px;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.amhs-track {
  position: relative;
  height: 36px;
}

.amhs-track__rail {
  position: absolute;
  inset: 14px 0 auto;
  height: 2px;
  background: linear-gradient(90deg, transparent, rgb(45 212 191 / 50%), transparent);
}

.amhs-track__pulse {
  position: absolute;
  top: 13px;
  left: -20%;
  width: 40%;
  height: 4px;
  border-radius: 2px;
  background: linear-gradient(90deg, transparent, #2dd4bf, transparent);
  opacity: 0.7;
}

.amhs-amr {
  position: absolute;
  top: 4px;
  left: 20%;
  display: flex;
  align-items: center;
  gap: 8px;
  will-change: transform;
}

.amhs-amr__body {
  width: 28px;
  height: 18px;
  border-radius: 4px;
  background: linear-gradient(180deg, #3dd9c4, #0f766e);
  box-shadow: 0 0 12px rgb(45 212 191 / 45%);
}

.amhs-amr__label {
  font-size: 11px;
  font-family: ui-monospace, monospace;
  opacity: 0.8;
}

.amhs-nodes {
  margin-top: 28px;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.amhs-node {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.amhs-scene.is-dark .amhs-node {
  border: 1px solid rgb(255 255 255 / 10%);
  background: rgb(255 255 255 / 4%);
}

.amhs-scene.is-light .amhs-node {
  border: 1px solid rgb(13 148 136 / 12%);
  background: rgb(255 255 255 / 62%);
}

@media (prefers-reduced-motion: reduce) {
  .amhs-track__pulse,
  .amhs-amr,
  .amhs-node {
    animation: none !important;
  }
}
</style>
