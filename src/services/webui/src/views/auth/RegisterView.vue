<template>
  <div class="auth-page">
    <div class="auth-card">
      <div class="auth-header">
        <i class="pi pi-user-plus auth-header-icon" />
        <h1 class="auth-header-title">ParkSystem</h1>
        <p class="auth-header-sub">Criar nova conta de cliente</p>
      </div>

      <div class="auth-body">
        <h2 class="auth-title">Criar Conta</h2>

        <form @submit.prevent="handleRegistro" class="auth-form">
          <div class="field">
            <label for="nome">Nome completo</label>
            <InputText
              id="nome"
              v-model="form.nome"
              placeholder="João Silva"
              :disabled="loading"
              class="w-full"
              autocomplete="name"
            />
          </div>

          <div class="field">
            <label for="email">E-mail</label>
            <InputText
              id="email"
              v-model="form.email"
              type="email"
              placeholder="joao@email.com"
              :disabled="loading"
              class="w-full"
              autocomplete="email"
            />
          </div>

          <div class="field">
            <label for="senha">Senha</label>
            <Password
              id="senha"
              v-model="form.senha"
              placeholder="Mínimo 6 caracteres"
              :disabled="loading"
              toggleMask
              class="w-full"
              inputClass="w-full"
              autocomplete="new-password"
            />
          </div>

          <Message v-if="errorMsg" severity="error" :closable="false">
            {{ errorMsg }}
          </Message>

          <Message v-if="successMsg" severity="success" :closable="false">
            {{ successMsg }}
          </Message>

          <Button
            type="submit"
            label="Criar Conta"
            icon="pi pi-check"
            class="w-full"
            :loading="loading"
          />
        </form>

        <p class="auth-footer-link">
          Já tem conta?
          <RouterLink to="/login">Fazer login</RouterLink>
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

const router = useRouter()
const auth   = useAuthStore()

const form = ref({ nome: '', email: '', senha: '' })
const loading    = ref(false)
const errorMsg   = ref('')
const successMsg = ref('')

async function handleRegistro() {
  errorMsg.value   = ''
  successMsg.value = ''
  loading.value    = true
  try {
    await auth.registrar(form.value.nome, form.value.email, form.value.senha)
    successMsg.value = 'Conta criada com sucesso! Redirecionando...'
    setTimeout(() => router.push('/login'), 1500)
  } catch (err: any) {
    const errors = err?.response?.data
    if (Array.isArray(errors) && errors.length > 0) {
      errorMsg.value = errors[0].errorMessage ?? 'Erro ao criar conta.'
    } else {
      errorMsg.value = 'Erro ao criar conta. Verifique os dados e tente novamente.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* reutiliza o mesmo visual do LoginView */
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

.auth-footer-link a:hover { text-decoration: underline; }

:deep(.p-password)       { width: 100%; }
:deep(.p-password-input) { width: 100%; }
</style>
