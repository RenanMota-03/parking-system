import api from './api'
import type { PagedResult, VagaDto, CadastrarVagaRequest, AlterarStatusVagaRequest } from '@/types'

export interface ListarVagasParams {
  status?:   number
  page?:     number
  pageSize?: number
}

export const vagaService = {
  async listar(params?: ListarVagasParams): Promise<PagedResult<VagaDto>> {
    const { data } = await api.get<PagedResult<VagaDto>>('/api/vagas', { params })
    return data
  },

  async cadastrar(body: CadastrarVagaRequest): Promise<{ id: number; numero: string }> {
    const { data } = await api.post('/api/vagas', body)
    return data
  },

  async alterarStatus(id: number, body: AlterarStatusVagaRequest): Promise<{ id: number }> {
    const { data } = await api.patch(`/api/vagas/${id}/status`, body)
    return data
  },
}
