<template>
  <div class="mov-view">

    <!-- ── Header ──────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">Movimentações</h1>
        <p class="page-subtitle">Histórico completo de entradas e saídas</p>
      </div>
      <Button
        icon="pi pi-refresh"
        severity="secondary"
        outlined
        :loading="loading"
        aria-label="Atualizar"
        @click="carregar"
      />
    </div>

    <!-- ── Barra de busca ───────────────────────────────────────────────── -->
    <div class="filtros-bar">
      <IconField class="search-input">
        <InputIcon class="pi pi-search" />
        <InputText
          v-model="searchPlaca"
          placeholder="Buscar por placa..."
          class="w-full"
        />
      </IconField>

      <div class="contagem-chip">
        <i class="pi pi-list" />
        <span>{{ movsFiltradas.length }} registro(s)</span>
      </div>
    </div>

    <!-- ── Tabela ───────────────────────────────────────────────────────── -->
    <div class="tabela-card">
      <DataTable
        :value="movsFiltradas"
        :loading="loading"
        paginator
        :rows="15"
        :rows-per-page-options="[15, 30, 50]"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        current-page-report-template="{first}–{last} de {totalRecords}"
        sort-mode="multiple"
        removable-sort
        size="small"
      >
        <template #empty>
          <div class="table-empty">
            <i class="pi pi-inbox" />
            <span>{{ searchPlaca ? 'Nenhuma placa encontrada.' : 'Nenhuma movimentação registrada.' }}</span>
          </div>
        </template>

        <template #loading>
          <div class="table-loading">
            <ProgressSpinner style="width:36px;height:36px" />
          </div>
        </template>

        <!-- Placa -->
        <Column field="placa_veiculo" header="Placa" sortable style="width:130px">
          <template #body="{ data }">
            <span class="placa-mono">{{ data.placa_veiculo }}</span>
          </template>
        </Column>

        <!-- Vaga -->
        <Column field="numero_vaga" header="Vaga" sortable style="width:80px">
          <template #body="{ data }">
            <span class="vaga-badge">{{ data.numero_vaga }}</span>
          </template>
        </Column>

        <!-- Entrada -->
        <Column field="data_entrada" header="Entrada" sortable style="width:160px">
          <template #body="{ data }">
            {{ formatarDataHora(data.data_entrada) }}
          </template>
        </Column>

        <!-- Saída -->
        <Column field="data_saida" header="Saída" sortable style="width:160px">
          <template #body="{ data }">
            <span v-if="data.data_saida">{{ formatarDataHora(data.data_saida) }}</span>
            <span v-else class="em-aberto-text">—</span>
          </template>
        </Column>

        <!-- Valor -->
        <Column field="valor_total" header="Valor" style="width:120px">
          <template #body="{ data }">
            <span v-if="data.valor_total != null" class="valor-cell">
              {{ formatarMoeda(data.valor_total) }}
            </span>
            <span v-else class="em-aberto-text">—</span>
          </template>
        </Column>

        <!-- Status -->
        <Column header="Status" style="width:120px">
          <template #body="{ data }">
            <Tag
              :value="data.pago ? 'Pago' : 'Em Aberto'"
              :severity="data.pago ? 'success' : 'warn'"
              rounded
            />
          </template>
        </Column>

      </DataTable>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { MovimentacaoDto } from '@/types'
import { movimentacaoService } from '@/services/movimentacaoService'

// ── Estado ────────────────────────────────────────────────────────────────
const movimentacoes = ref<MovimentacaoDto[]>([])
const loading       = ref(false)
const searchPlaca   = ref('')

// ── Filtro client-side ────────────────────────────────────────────────────
const movsFiltradas = computed(() => {
  const q = searchPlaca.value.trim().toUpperCase()
  if (!q) return movimentacoes.value
  return movimentacoes.value.filter(m => m.placa_veiculo.includes(q))
})

// ── Carregar dados ────────────────────────────────────────────────────────
async function carregar() {
  loading.value = true
  try {
    const result = await movimentacaoService.listarTodas(1, 200)
    movimentacoes.value = result.items
  } catch {
    movimentacoes.value = []
  } finally {
    loading.value = false
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────
function formatarDataHora(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function formatarMoeda(v: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
}

function calcularTempo(entrada: string): string {
  const ms  = Date.now() - new Date(entrada).getTime()
  const min = Math.floor(ms / 60000)
  if (min < 60) return `${min}min`
  const h = Math.floor(min / 60)
  const m = min % 60
  return m > 0 ? `${h}h ${m}min` : `${h}h`
}

function tempoClass(entrada: string): string {
  const horas = (Date.now() - new Date(entrada).getTime()) / 3600000
  if (horas >= 8)  return 'tempo-alto'
  if (horas >= 2)  return 'tempo-medio'
  return 'tempo-baixo'
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(carregar)
</script>

<style scoped>
.mov-view {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-md);
}

.page-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--p-surface-900);
}

.page-subtitle {
  margin: 4px 0 0;
  font-size: var(--font-sm);
  color: var(--p-surface-500);
}

/* Filtros */
.filtros-bar {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.search-input {
  min-width: 240px;
  flex: 1;
  max-width: 400px;
}

.contagem-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
  border-radius: 20px;
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--p-surface-600);
  white-space: nowrap;
}

.contagem-chip .pi { font-size: 0.875rem; color: var(--color-primary); }

/* Tabela */
.tabela-card {
  background: var(--color-surface-card);
  border-radius: var(--radius-md);
  border: 1px solid var(--p-surface-200);
  box-shadow: var(--shadow-card);
  overflow: hidden;
}

.table-empty,
.table-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-2xl) 0;
  color: var(--p-surface-400);
  font-size: var(--font-sm);
}

.table-empty .pi { font-size: 1.5rem; }

/* Células */
.placa-mono {
  font-family: 'Courier New', monospace;
  font-size: var(--font-sm);
  font-weight: 700;
  letter-spacing: 0.08em;
  color: var(--p-surface-800);
}

.vaga-badge {
  display: inline-block;
  background: var(--color-surface-container);
  color: var(--color-primary);
  font-weight: 700;
  font-size: var(--font-xs);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  letter-spacing: 0.04em;
}

.tempo-badge {
  font-size: var(--font-xs);
  font-weight: 700;
  padding: 2px 8px;
  border-radius: var(--radius-full);
}

.tempo-baixo  { background: #dcfce7; color: #16a34a; }
.tempo-medio  { background: #fef9c3; color: #ca8a04; }
.tempo-alto   { background: #fee2e2; color: #dc2626; }

.valor-cell {
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--color-primary);
}

.em-aberto-text {
  font-size: var(--font-sm);
  color: var(--p-surface-400);
}
</style>
