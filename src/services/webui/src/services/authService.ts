import api from './api'
import type { LoginRequest, RegistroRequest, AuthResponse } from '@/types'

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/api/auth/login', data)
    return res.data
  },

  async registrar(data: RegistroRequest): Promise<void> {
    await api.post('/api/auth/registro', data)
  },
}
