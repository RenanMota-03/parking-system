import api from './api'
import type { PagedResult, ReservaDto, CriarReservaRequest } from '@/types'

export const reservaService = {
  async listar(page = 1, pageSize = 20): Promise<PagedResult<ReservaDto>> {
    const { data } = await api.get<PagedResult<ReservaDto>>('/api/reservas', {
      params: { page, pageSize },
    })
    return data
  },

  async criar(body: CriarReservaRequest): Promise<ReservaDto> {
    const { data } = await api.post<ReservaDto>('/api/reservas', body)
    return data
  },

  async cancelar(id: number): Promise<void> {
    await api.delete(`/api/reservas/${id}`)
  },
}
