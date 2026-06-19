import api from './api'
import type { PagedResult, UsuarioListDto } from '@/types'

export const usuarioService = {
  async listar(page = 1, pageSize = 20): Promise<PagedResult<UsuarioListDto>> {
    const { data } = await api.get<PagedResult<UsuarioListDto>>('/api/usuarios', {
      params: { page, pageSize },
    })
    return data
  },
}
