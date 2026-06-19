import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { Role } from '@/types'
import type { AuthUser } from '@/types'
import { authService } from '@/services/authService'

function parseJwt(token: string): Record<string, unknown> {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(atob(base64))
  } catch {
    return {}
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('auth_token'))
  const user  = ref<AuthUser | null>(
    JSON.parse(localStorage.getItem('auth_user') ?? 'null')
  )

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin         = computed(() => user.value?.role === Role.Admin)
  const isOperador      = computed(() => user.value?.role === Role.Operador)
  const isCliente       = computed(() => user.value?.role === Role.Cliente)
  const isAdminOrOp     = computed(() => isAdmin.value || isOperador.value)

  async function login(email: string, senha: string) {
    const response = await authService.login({ email, senha })

    const payload = parseJwt(response.access_token)

    token.value = response.access_token
    user.value  = {
      id:    String(payload['sub'] ?? ''),
      name:  response.name,
      email,
      role:  Role[response.role as keyof typeof Role],
    }

    localStorage.setItem('auth_token', token.value)
    localStorage.setItem('auth_user', JSON.stringify(user.value))
  }

  async function registrar(nome: string, email: string, senha: string) {
    await authService.registrar({ nome, email, senha })
  }

  function logout() {
    token.value = null
    user.value  = null
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_user')
  }

  return { token, user, isAuthenticated, isAdmin, isOperador, isCliente, isAdminOrOp, login, registrar, logout }
})
