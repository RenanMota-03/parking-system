<template>
  <header class="topnav">
    <div class="topnav-left">
      <h1 class="page-title">{{ pageTitle }}</h1>
    </div>

    <div class="topnav-right">
      <div class="topnav-icons">
        <i class="pi pi-clock nav-icon" title="Horário do sistema" />
        <i class="pi pi-wifi nav-icon" title="Sistema online" />
        <i class="pi pi-bell nav-icon" title="Notificações" />
      </div>
      <div class="user-info">
        <div class="user-avatar" :title="auth.user?.name">
          {{ initials }}
        </div>
        <div class="user-details">
          <span class="user-name">{{ auth.user?.name }}</span>
          <span class="user-role">{{ roleLabel }}</span>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { ROLE_LABEL } from '@/types'

const auth  = useAuthStore()
const route = useRoute()

const pageTitles: Record<string, string> = {
  '/':               'Dashboard',
  '/vagas':          'Mapa de Vagas',
  '/fluxo':          'Controle de Entrada / Saída',
  '/movimentacoes':  'Movimentações',
  '/reservas':       'Reservas',
  '/tarifas':        'Configuração de Tarifas',
  '/relatorios':     'Relatórios Financeiros',
  '/configuracoes':  'Configurações do Sistema',
}

const pageTitle = computed(() => pageTitles[route.path] ?? 'ParkSystem')

const initials = computed(() => {
  const name = auth.user?.name ?? ''
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
})

const roleLabel = computed(() =>
  auth.user ? ROLE_LABEL[auth.user.role] : ''
)
</script>

<style scoped>
.topnav {
  position: fixed;
  top: 0;
  left: var(--sidebar-collapsed);
  right: 0;
  height: var(--topnav-height);
  background: var(--color-surface-card);
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-xl);
  z-index: 90;
  transition: left 0.25s ease;
}

.page-title {
  font-size: var(--font-title);
  font-weight: 600;
  color: var(--p-surface-800);
  margin: 0;
}

.topnav-right {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.topnav-icons {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.nav-icon {
  font-size: 1.1rem;
  color: var(--p-surface-400);
  cursor: default;
  padding: 6px;
  border-radius: var(--radius-sm);
  transition: color 0.15s, background 0.15s;
}

.nav-icon:hover {
  color: var(--p-surface-700);
  background: var(--color-surface-container);
}

.user-info {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--color-primary);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-xs);
  font-weight: 700;
  flex-shrink: 0;
  cursor: default;
}

.user-details {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--p-surface-800);
  line-height: 1.2;
}

.user-role {
  font-size: var(--font-xs);
  color: var(--p-surface-500);
}
</style>
