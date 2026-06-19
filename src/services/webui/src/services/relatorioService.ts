import api from './api'
import type { ResumoFinanceiroDto, PagedResult, MovimentacaoDto } from '@/types'

export const relatorioService = {
  async getResumo(): Promise<ResumoFinanceiroDto> {
    const { data } = await api.get<ResumoFinanceiroDto>('/api/relatorios/resumo')
    return data
  },

  async listarTransacoes(page = 1, pageSize = 50): Promise<PagedResult<MovimentacaoDto>> {
    const { data } = await api.get<PagedResult<MovimentacaoDto>>('/api/movimentacoes', {
      params: { page, pageSize },
    })
    return data
  },
}
