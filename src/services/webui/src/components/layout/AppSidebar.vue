<template>
  <nav class="sidebar" :class="{ expanded }">
    <div class="sidebar-logo">
      <i class="pi pi-car logo-icon" />
      <span class="logo-text">ParkSystem</span>
    </div>

    <ul class="sidebar-menu">
      <li v-for="item in visibleItems" :key="item.name">
        <RouterLink :to="item.to" class="menu-item" :class="{ active: isActive(item.to) }">
          <i :class="`pi ${item.icon}`" class="menu-icon" />
          <span class="menu-label">{{ item.label }}</span>
        </RouterLink>
      </li>
    </ul>

    <div class="sidebar-footer">
      <div class="user-profile">
        <div class="profile-avatar">{{ userInitials }}</div>
        <div class="profile-info">
          <span class="profile-name">{{ auth.user?.name }}</span>
          <span class="profile-role">{{ userRoleLabel }}</span>
        </div>
      </div>
      <button class="menu-item logout-btn" @click="handleLogout">
        <i class="pi pi-sign-out menu-icon" />
        <span class="menu-label">Sair</span>
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { Role, ROLE_LABEL } from '@/types'

const route  = useRoute()
const router = useRouter()
const auth   = useAuthStore()

defineProps<{ expanded: boolean }>()

const allItems = [
  { name: 'dashboard',    label: 'Dashboard',      icon: 'pi-chart-bar',    to: '/',               roles: [Role.Admin] },
  { name: 'vagas',        label: 'Mapa de Vagas',  icon: 'pi-map',          to: '/vagas',          roles: [Role.Admin, Role.Operador, Role.Cliente] },
  { name: 'fluxo',        label: 'Entrada / Saída',icon: 'pi-arrows-h',     to: '/fluxo',          roles: [Role.Admin, Role.Operador] },
  { name: 'movimentacoes',label: 'Movimentações',  icon: 'pi-list',         to: '/movimentacoes',  roles: [Role.Admin, Role.Operador] },
  { name: 'reservas',     label: 'Reservas',       icon: 'pi-calendar',     to: '/reservas',       roles: [Role.Admin, Role.Cliente] },
  { name: 'tarifas',      label: 'Tarifas',        icon: 'pi-dollar',       to: '/tarifas',        roles: [Role.Admin, Role.Operador] },
  { name: 'relatorios',   label: 'Relatórios',     icon: 'pi-chart-line',   to: '/relatorios',     roles: [Role.Admin] },
  { name: 'configuracoes',label: 'Configurações',  icon: 'pi-cog',          to: '/configuracoes',  roles: [Role.Admin] },
]

const visibleItems = computed(() =>
  allItems.filter(item => auth.user && item.roles.includes(auth.user.role))
)

function isActive(to: string) {
  return to === '/' ? route.path === '/' : route.path.startsWith(to)
}

const userInitials = computed(() => {
  const name = auth.user?.name ?? ''
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
})

const userRoleLabel = computed(() =>
  auth.user ? ROLE_LABEL[auth.user.role] : ''
)

async function handleLogout() {
  auth.logout()
  await router.push('/login')
}
</script>

<style scoped>
.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: var(--sidebar-collapsed);
  background: var(--color-inverse-surface);
  display: flex;
  flex-direction: column;
  transition: width 0.25s ease;
  overflow: hidden;
  z-index: 100;
}

.sidebar:hover,
.sidebar.expanded {
  width: var(--sidebar-expanded);
}

/* ── Logo ─────────────────────────────────────────── */
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-md);
  border-bottom: 1px solid rgba(255,255,255,0.08);
  min-height: var(--topnav-height);
}

.logo-icon {
  font-size: 1.5rem;
  color: #fff;
  flex-shrink: 0;
  width: 40px;
  text-align: center;
}

.logo-text {
  color: #fff;
  font-size: var(--font-sm);
  font-weight: 700;
  white-space: nowrap;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.sidebar:hover .logo-text,
.sidebar.expanded .logo-text {
  opacity: 1;
}

/* ── Menu ─────────────────────────────────────────── */
.sidebar-menu {
  list-style: none;
  margin: 0;
  padding: var(--space-sm) 0;
  flex: 1;
  overflow-y: auto;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: 0.75rem var(--space-md);
  color: rgba(255,255,255,0.65);
  text-decoration: none;
  cursor: pointer;
  background: none;
  border: none;
  width: 100%;
  font-size: var(--font-sm);
  font-family: inherit;
  transition: background 0.15s, color 0.15s;
  border-left: 3px solid transparent;
}

.menu-item:hover {
  background: rgba(255,255,255,0.08);
  color: #fff;
}

.menu-item.active {
  background: rgba(255,255,255,0.12);
  color: #fff;
  border-left-color: var(--color-primary-light);
}

.menu-icon {
  font-size: 1.1rem;
  flex-shrink: 0;
  width: 24px;
  text-align: center;
}

.menu-label {
  white-space: nowrap;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.sidebar:hover .menu-label,
.sidebar.expanded .menu-label {
  opacity: 1;
}

/* ── Footer ───────────────────────────────────────── */
.sidebar-footer {
  border-top: 1px solid rgba(255,255,255,0.08);
  padding: var(--space-sm) 0;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: 0.75rem var(--space-md);
  overflow: hidden;
}

.profile-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--color-primary-light);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
  font-weight: 700;
  flex-shrink: 0;
}

.profile-info {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.sidebar:hover .profile-info,
.sidebar.expanded .profile-info {
  opacity: 1;
}

.profile-name {
  font-size: var(--font-xs);
  font-weight: 600;
  color: rgba(255,255,255,0.9);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.profile-role {
  font-size: 0.65rem;
  color: rgba(255,255,255,0.45);
}

.logout-btn {
  color: rgba(255,255,255,0.5);
}

.logout-btn:hover {
  color: #ef4444;
  background: rgba(239,68,68,0.1);
}
</style>
