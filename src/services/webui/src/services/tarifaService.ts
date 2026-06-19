import api from './api'
import type { TarifaDto, CadastrarTarifaRequest, AtualizarTarifaRequest } from '@/types'

export const tarifaService = {
  async listar(): Promise<TarifaDto[]> {
    const { data } = await api.get<TarifaDto[]>('/api/tarifas')
    return data
  },

  async cadastrar(body: CadastrarTarifaRequest): Promise<TarifaDto> {
    const { data } = await api.post<TarifaDto>('/api/tarifas', body)
    return data
  },

  async atualizar(id: number, body: AtualizarTarifaRequest): Promise<TarifaDto> {
    const { data } = await api.put<TarifaDto>(`/api/tarifas/${id}`, body)
    return data
  },
}
