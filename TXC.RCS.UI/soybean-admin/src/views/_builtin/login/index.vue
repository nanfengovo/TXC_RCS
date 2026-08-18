<script setup lang="ts">
import { computed } from 'vue';
import type { Component } from 'vue';
import { loginModuleRecord } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';
import AmhsLoginScene from './modules/amhs-scene.vue';
import PwdLogin from './modules/pwd-login.vue';
import CodeLogin from './modules/code-login.vue';
import Register from './modules/register.vue';
import ResetPwd from './modules/reset-pwd.vue';
import BindWechat from './modules/bind-wechat.vue';

interface Props {
  module?: UnionKey.LoginModule;
}

const props = defineProps<Props>();

const appStore = useAppStore();
const themeStore = useThemeStore();

interface LoginModule {
  label: App.I18n.I18nKey;
  component: Component;
}

const moduleMap: Record<UnionKey.LoginModule, LoginModule> = {
  'pwd-login': { label: loginModuleRecord['pwd-login'], component: PwdLogin },
  'code-login': { label: loginModuleRecord['code-login'], component: CodeLogin },
  register: { label: loginModuleRecord.register, component: Register },
  'reset-pwd': { label: loginModuleRecord['reset-pwd'], component: ResetPwd },
  'bind-wechat': { label: loginModuleRecord['bind-wechat'], component: BindWechat }
};

const activeModule = computed(() => moduleMap[props.module || 'pwd-login']);
const isDark = computed(() => themeStore.darkMode);
</script>

<template>
  <div class="amhs-login" :class="{ 'is-dark': isDark, 'is-light': !isDark }">
    <aside class="amhs-login__scene lt-md:hidden">
      <AmhsLoginScene :dark="isDark" />
    </aside>

    <main class="amhs-login__panel">
      <div class="amhs-login__panel-inner">
        <header class="amhs-login__header">
          <SystemLogo class="amhs-login__logo" />
          <div class="amhs-login__toolbar">
            <ThemeSchemaSwitch
              :theme-schema="themeStore.themeScheme"
              :show-tooltip="false"
              class="amhs-login__tool-btn text-20px"
              @switch="themeStore.toggleThemeScheme"
            />
            <LangSwitch
              v-if="themeStore.header.multilingual.visible"
              :lang="appStore.locale"
              :lang-options="appStore.localeOptions"
              :show-tooltip="false"
              class="amhs-login__tool-btn"
              @change-lang="appStore.changeLocale"
            />
          </div>
        </header>

        <div class="amhs-login__form-card">
          <p class="amhs-login__eyebrow">TXC RCS · AMHS Control</p>
          <h2 class="amhs-login__title">{{ $t(activeModule.label) }}</h2>
          <p class="amhs-login__hint">{{ $t('page.login.amhsHint') }}</p>

          <Transition :name="themeStore.page.animateMode" mode="out-in" appear>
            <component :is="activeModule.component" :key="activeModule.label" />
          </Transition>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.amhs-login {
  --amhs-accent: #2dd4bf;
  --amhs-accent-deep: #0d9488;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(400px, 0.85fr);
  min-height: 100vh;
  color: var(--amhs-text);
  transition:
    background-color 0.25s ease,
    color 0.25s ease;
}

.amhs-login.is-dark {
  --amhs-text: #e8f4f8;
  --amhs-muted: rgb(232 244 248 / 65%);
  background: #050a0f;
}

.amhs-login.is-light {
  --amhs-text: #102a43;
  --amhs-muted: rgb(16 42 67 / 66%);
  background: #f4f7fb;
}

.amhs-login__scene {
  min-height: 100vh;
}

.amhs-login__panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 28px;
  transition:
    background 0.25s ease,
    border-color 0.25s ease;
}

.amhs-login.is-dark .amhs-login__panel {
  background:
    radial-gradient(ellipse 80% 60% at 50% 0%, rgb(45 212 191 / 8%), transparent),
    linear-gradient(180deg, rgb(10 16 22 / 96%), rgb(6 10 14 / 98%));
  border-left: 1px solid rgb(45 212 191 / 12%);
}

.amhs-login.is-light .amhs-login__panel {
  background:
    radial-gradient(ellipse 80% 60% at 50% 0%, rgb(45 212 191 / 10%), transparent),
    linear-gradient(180deg, rgb(255 255 255 / 96%), rgb(243 247 252 / 98%));
  border-left: 1px solid rgb(15 118 110 / 10%);
}

.amhs-login__panel-inner {
  width: min(100%, 420px);
}

.amhs-login__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 28px;
}

.amhs-login__logo {
  height: 36px;
  width: auto;
  max-width: 200px;
}

.amhs-login.is-dark .amhs-login__logo {
  filter: brightness(1.05);
}

.amhs-login__toolbar {
  display: flex;
  align-items: center;
  gap: 4px;
}

.amhs-login__tool-btn {
  color: var(--amhs-muted) !important;
}

.amhs-login__tool-btn:hover {
  color: var(--amhs-accent) !important;
}

.amhs-login__form-card {
  padding: 28px 24px;
  border-radius: 16px;
  backdrop-filter: blur(16px);
  transition:
    background 0.25s ease,
    border-color 0.25s ease,
    box-shadow 0.25s ease;
}

.amhs-login.is-dark .amhs-login__form-card {
  border: 1px solid rgb(45 212 191 / 14%);
  background: rgb(255 255 255 / 4%);
  box-shadow:
    0 0 0 1px rgb(255 255 255 / 4%) inset,
    0 16px 48px rgb(0 0 0 / 35%);
}

.amhs-login.is-light .amhs-login__form-card {
  border: 1px solid rgb(15 118 110 / 10%);
  background: rgb(255 255 255 / 78%);
  box-shadow:
    0 0 0 1px rgb(255 255 255 / 55%) inset,
    0 18px 42px rgb(15 23 42 / 8%);
}

.amhs-login__eyebrow {
  margin: 0;
  font-size: 11px;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: var(--amhs-accent);
}

.amhs-login__title {
  margin: 10px 0 0;
  font-size: 22px;
  font-weight: 600;
  color: var(--amhs-text);
}

.amhs-login__hint {
  margin: 8px 0 22px;
  font-size: 13px;
  color: var(--amhs-muted);
  line-height: 1.55;
}

@media (max-width: 768px) {
  .amhs-login {
    grid-template-columns: 1fr;
  }

  .amhs-login__panel {
    min-height: 100vh;
    border-left: none;
  }
}
</style>
