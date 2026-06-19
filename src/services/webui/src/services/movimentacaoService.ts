import api from './api'
import type { PagedResult, MovimentacaoDto, RegistrarEntradaRequest, RegistrarSaidaRequest, PagarRequest } from '@/types'

export const movimentacaoService = {
  async listar(page = 1, pageSize = 20): Promise<PagedResult<MovimentacaoDto>> {
    const { data } = await api.get<PagedResult<MovimentacaoDto>>('/api/movimentacoes', {
      params: { aberta: true, page, pageSize },
    })
    return data
  },

  async listarTodas(page = 1, pageSize = 20): Promise<PagedResult<MovimentacaoDto>> {
    const { data } = await api.get<PagedResult<MovimentacaoDto>>('/api/movimentacoes', {
      params: { page, pageSize },
    })
    return data
  },

  async buscarPorId(id: number): Promise<MovimentacaoDto> {
    const { data } = await api.get<MovimentacaoDto>(`/api/movimentacoes/${id}`)
    return data
  },

  async registrarEntrada(body: RegistrarEntradaRequest) {
    const { data } = await api.post('/api/fluxo/entrada', body)
    return data
  },

  async registrarSaida(body: RegistrarSaidaRequest): Promise<MovimentacaoDto> {
    const { data } = await api.post<MovimentacaoDto>('/api/fluxo/saida', body)
    return data
  },

  async pagar(body: PagarRequest) {
    const { data } = await api.post('/api/fluxo/pagamento', body)
    return data
  },
}
