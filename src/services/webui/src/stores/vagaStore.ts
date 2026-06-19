import { defineStore } from 'pinia'
import { ref } from 'vue'
import { StatusVaga } from '@/types'
import type { VagaDto, CadastrarVagaRequest } from '@/types'
import { vagaService } from '@/services/vagaService'

export const useVagaStore = defineStore('vaga', () => {
  const vagas   = ref<VagaDto[]>([])
  const loading = ref(false)
  const error   = ref<string | null>(null)

  async function fetchAll() {
    loading.value = true
    error.value   = null
    try {
      const result = await vagaService.listar({ pageSize: 200 })
      vagas.value  = result.items
    } catch (e: any) {
      error.value = e?.response?.data?.errors?.[0] ?? 'Erro ao carregar vagas.'
    } finally {
      loading.value = false
    }
  }

  async function cadastrar(body: CadastrarVagaRequest): Promise<string | null> {
    try {
      await vagaService.cadastrar(body)
      await fetchAll()
      return null
    } catch (e: any) {
      return e?.response?.data?.errors?.[0] ?? 'Erro ao cadastrar vaga.'
    }
  }

  async function alterarStatus(id: number, novoStatus: StatusVaga): Promise<string | null> {
    try {
      await vagaService.alterarStatus(id, { novoStatus })
      const vaga = vagas.value.find(v => v.id === id)
      if (vaga) vaga.status = novoStatus
      return null
    } catch (e: any) {
      return e?.response?.data?.errors?.[0] ?? 'Erro ao alterar status.'
    }
  }

  return { vagas, loading, error, fetchAll, cadastrar, alterarStatus }
})
