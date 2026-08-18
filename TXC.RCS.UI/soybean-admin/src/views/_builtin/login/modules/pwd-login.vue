<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useAuthStore } from '@/store/modules/auth';
import { useRouterPush } from '@/hooks/common/router';
import { useFormRules, useNaiveForm } from '@/hooks/common/form';
import { useRcsConfigStore } from '@/store/modules/rcs-config';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';

defineOptions({ name: 'PwdLogin' });

const authStore = useAuthStore();
const rcsConfigStore = useRcsConfigStore();
const themeStore = useThemeStore();
const { config } = storeToRefs(rcsConfigStore);
const { toggleLoginModule } = useRouterPush();
const { formRef, validate } = useNaiveForm();

interface FormModel {
  userName: string;
  password: string;
}

const model: FormModel = reactive({
  userName: config.value.defaultLoginUser,
  password: config.value.defaultLoginPassword
});

const rememberMe = ref(true);

const rules = computed<Record<keyof FormModel, App.Global.FormRule[]>>(() => {
  const { formRules } = useFormRules();
  return {
    userName: formRules.userName,
    password: formRules.pwd
  };
});

async function handleSubmit() {
  await validate();
  await authStore.login(model.userName, model.password);
}
</script>

<template>
  <NForm
    ref="formRef"
    class="amhs-login-form"
    :class="{ 'is-dark': themeStore.darkMode, 'is-light': !themeStore.darkMode }"
    :model="model"
    :rules="rules"
    size="large"
    :show-label="false"
    @keyup.enter="handleSubmit"
  >
    <NFormItem path="userName">
      <NInput
        v-model:value="model.userName"
        class="amhs-login-form__input"
        :placeholder="$t('page.login.common.userNamePlaceholder')"
      >
        <template #prefix>
          <icon-ic-round-person class="amhs-login-form__icon" />
        </template>
      </NInput>
    </NFormItem>
    <NFormItem path="password">
      <NInput
        v-model:value="model.password"
        class="amhs-login-form__input"
        type="password"
        show-password-on="click"
        :placeholder="$t('page.login.common.passwordPlaceholder')"
      >
        <template #prefix>
          <icon-ic-round-lock class="amhs-login-form__icon" />
        </template>
      </NInput>
    </NFormItem>
    <NSpace vertical :size="20">
      <div class="flex-y-center justify-between">
        <NCheckbox v-model:checked="rememberMe" class="amhs-login-form__checkbox">
          {{ $t('page.login.pwdLogin.rememberMe') }}
        </NCheckbox>
        <NButton quaternary class="amhs-login-form__link" @click="toggleLoginModule('reset-pwd')">
          {{ $t('page.login.pwdLogin.forgetPassword') }}
        </NButton>
      </div>
      <NButton
        class="amhs-login-form__submit"
        type="primary"
        size="large"
        round
        block
        :loading="authStore.loginLoading"
        @click="handleSubmit"
      >
        {{ $t('page.login.pwdLogin.submit') }}
      </NButton>
    </NSpace>
  </NForm>
</template>

<style scoped>
.amhs-login-form {
  transition: color 0.25s ease;
}

.amhs-login-form__icon {
  font-size: 18px;
  opacity: 0.55;
  color: #2dd4bf;
}

.amhs-login-form.is-dark :deep(.n-input) {
  --n-border: 1px solid rgb(45 212 191 / 22%);
  --n-border-hover: 1px solid rgb(45 212 191 / 45%);
  --n-border-focus: 1px solid rgb(45 212 191 / 70%);
  --n-box-shadow-focus: 0 0 0 2px rgb(45 212 191 / 18%);
  --n-color: rgb(255 255 255 / 6%);
  --n-color-focus: rgb(255 255 255 / 8%);
  --n-text-color: #e8f4f8;
  --n-placeholder-color: rgb(232 244 248 / 45%);
  border-radius: 10px;
}

.amhs-login-form.is-light :deep(.n-input) {
  --n-border: 1px solid rgb(13 148 136 / 18%);
  --n-border-hover: 1px solid rgb(13 148 136 / 30%);
  --n-border-focus: 1px solid rgb(13 148 136 / 48%);
  --n-box-shadow-focus: 0 0 0 2px rgb(45 212 191 / 14%);
  --n-color: rgb(255 255 255 / 92%);
  --n-color-focus: rgb(255 255 255 / 98%);
  --n-text-color: #102a43;
  --n-placeholder-color: rgb(16 42 67 / 42%);
  border-radius: 10px;
}

.amhs-login-form :deep(.n-input .n-input__input-el) {
  height: 44px;
}

.amhs-login-form__checkbox :deep(.n-checkbox-box) {
  border-color: rgb(45 212 191 / 40%);
}

.amhs-login-form__checkbox :deep(.n-checkbox-box--checked) {
  background: #0d9488;
  border-color: #2dd4bf;
}

.amhs-login-form__link {
  color: #2dd4bf !important;
}

.amhs-login-form__submit {
  height: 46px;
  font-weight: 600;
  letter-spacing: 0.04em;
  border: none !important;
  background: linear-gradient(135deg, #2dd4bf 0%, #0d9488 100%) !important;
  box-shadow: 0 4px 20px rgb(45 212 191 / 28%);
}

.amhs-login-form__submit:hover {
  background: linear-gradient(135deg, #5eead4 0%, #14b8a6 100%) !important;
  box-shadow: 0 6px 24px rgb(45 212 191 / 36%);
}
</style>
