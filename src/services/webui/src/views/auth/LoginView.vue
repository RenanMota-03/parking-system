<template>
  <div class="auth-page">
    <div class="auth-card">
      <div class="auth-header">
        <i class="pi pi-lock auth-header-icon" />
        <h1 class="auth-header-title">ParkSystem</h1>
        <p class="auth-header-sub">Sistema de Gestão de Estacionamento</p>
      </div>

      <div class="auth-body">
        <h2 class="auth-title">Acessar Conta</h2>

        <form @submit.prevent="handleLogin" class="auth-form">
          <div class="field">
            <label for="email">E-mail</label>
            <InputText
              id="email"
              v-model="form.email"
              type="email"
              placeholder="admin@parking.com"
              :disabled="loading"
              class="w-full"
              autocomplete="email"
            />
          </div>

          <div class="field">
            <div class="field-label-row">
              <label for="senha">Senha</label>
              <a href="#" class="forgot-link" tabindex="-1" @click.prevent>Esqueceu a senha?</a>
            </div>
            <Password
              id="senha"
              v-model="form.senha"
              placeholder="••••••••"
              :disabled="loading"
              :feedback="false"
              toggleMask
              class="w-full"
              inputClass="w-full"
              autocomplete="current-password"
            />
          </div>

          <div class="remember-row">
            <input id="remember" type="checkbox" class="remember-checkbox" />
            <label for="remember" class="remember-label">Lembrar de mim</label>
          </div>

          <Message v-if="errorMsg" severity="error" :closable="false">
            {{ errorMsg }}
          </Message>

          <Button
            type="submit"
            label="Entrar no Sistema"
            icon-pos="right"
            icon="pi pi-arrow-right"
            class="w-full"
            :loading="loading"
          />
        </form>

        <div class="ssl-badge">
          <i class="pi pi-lock ssl-icon" />
          <span>Conexão segura criptografada (SSL)</span>
        </div>

        <p class="auth-footer-link">
          Não tem conta?
          <RouterLink to="/registro">Criar conta</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/authStore'
import { Role } from '@/types'

const router = useRouter()
const auth   = useAuthStore()

const form = ref({ email: '', senha: '' })
const loading  = ref(false)
const errorMsg = ref('')

async function handleLogin() {
  errorMsg.value = ''
  loading.value  = true
  try {
    await auth.login(form.value.email, form.value.senha)
    const role = auth.user?.role
    if (role === Role.Admin)    await router.push('/')
    else if (role === Role.Operador) await router.push('/fluxo')
    else                             await router.push('/vagas')
  } catch {
    errorMsg.value = 'E-mail ou senha inválidos.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  background: var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
  background-image: radial-gradient(circle, #d4d9ed 1px, transparent 1px);
  background-size: 24px 24px;
}

.auth-card {
  width: 100%;
  max-width: 448px;
  background: var(--color-surface-card);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-modal);
  overflow: hidden;
}

/* ── Header escuro ───────────────────────────────── */
.auth-header {
  background: var(--color-inverse-surface);
  padding: var(--space-xl) var(--space-xl) var(--space-lg);
  text-align: center;
}

.auth-header-icon {
  font-size: 2.5rem;
  color: #fff;
  display: block;
  margin-bottom: var(--space-sm);
}

.auth-header-title {
  font-size: var(--font-headline);
  font-weight: 700;
  color: #fff;
  margin: 0 0 var(--space-xs);
}

.auth-header-sub {
  font-size: var(--font-xs);
  color: rgba(255,255,255,0.6);
  margin: 0;
}

/* ── Body ─────────────────────────────────────────── */
.auth-body {
  padding: var(--space-xl);
}

.auth-title {
  font-size: var(--font-title);
  font-weight: 600;
  color: var(--p-surface-800);
  margin: 0 0 var(--space-lg);
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.field {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.field label {
  font-size: var(--font-xs);
  font-weight: 600;
  color: var(--p-surface-600);
}

.field-label-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.forgot-link {
  font-size: var(--font-xs);
  color: var(--color-primary);
  text-decoration: none;
}

.forgot-link:hover { text-decoration: underline; }

.remember-row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-top: 2px;
}

.remember-checkbox {
  width: 16px;
  height: 16px;
  accent-color: var(--color-primary);
  cursor: pointer;
}

.remember-label {
  font-size: var(--font-xs);
  color: var(--p-surface-600);
  cursor: pointer;
  user-select: none;
}

.ssl-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-xs);
  padding: var(--space-md) 0 var(--space-sm);
  border-top: 1px solid var(--p-surface-200);
  font-size: var(--font-xs);
  color: var(--p-surface-400);
  margin-top: var(--space-md);
}

.ssl-icon { font-size: 0.75rem; }

.auth-footer-link {
  text-align: center;
  font-size: var(--font-sm);
  color: var(--p-surface-500);
  margin-top: var(--space-md);
}

.auth-footer-link a {
  color: var(--color-primary);
  font-weight: 600;
  text-decoration: none;
}

.auth-footer-link a:hover {
  text-decoration: underline;
}

/* garantir que Password ocupe 100% */
:deep(.p-password) { width: 100%; }
:deep(.p-password-input) { width: 100%; }
</style>
