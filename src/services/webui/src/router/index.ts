import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { Role } from '@/types'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // ── Públicas ──────────────────────────────────────────────────
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/registro',
      name: 'registro',
      component: () => import('@/views/auth/RegisterView.vue'),
      meta: { public: true },
    },

    // ── App (com layout sidebar) ──────────────────────────────────
    {
      path: '/',
      component: () => import('@/components/layout/AppLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardView.vue'),
          meta: { roles: [Role.Admin] },
        },
        {
          path: 'vagas',
          name: 'vagas',
          component: () => import('@/views/VagasView.vue'),
          meta: { roles: [Role.Admin, Role.Operador, Role.Cliente] },
        },
        {
          path: 'fluxo',
          name: 'fluxo',
          component: () => import('@/views/FluxoView.vue'),
          meta: { roles: [Role.Admin, Role.Operador] },
        },
        {
          path: 'movimentacoes',
          name: 'movimentacoes',
          component: () => import('@/views/MovimentacoesView.vue'),
          meta: { roles: [Role.Admin, Role.Operador] },
        },
        {
          path: 'reservas',
          name: 'reservas',
          component: () => import('@/views/ReservasView.vue'),
          meta: { roles: [Role.Admin, Role.Cliente] },
        },
        {
          path: 'tarifas',
          name: 'tarifas',
          component: () => import('@/views/TarifasView.vue'),
          meta: { roles: [Role.Admin, Role.Operador] },
        },
        {
          path: 'relatorios',
          name: 'relatorios',
          component: () => import('@/views/RelatoriosView.vue'),
          meta: { roles: [Role.Admin] },
        },
        {
          path: 'configuracoes',
          name: 'configuracoes',
          component: () => import('@/views/ConfiguracoesView.vue'),
          meta: { roles: [Role.Admin] },
        },
      ],
    },
  ],
})

router.beforeEach(to => {
  const auth = useAuthStore()

  if (to.meta.public) return true

  if (!auth.isAuthenticated) return { name: 'login' }

  const allowedRoles = to.meta.roles as Role[] | undefined
  if (allowedRoles && !allowedRoles.includes(auth.user!.role)) {
    return { name: 'vagas' }
  }

  return true
})

export default router
