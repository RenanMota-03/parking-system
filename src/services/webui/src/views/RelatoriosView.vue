<template>
  <div class="relatorios-view">

    <!-- ── Header ──────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">Relatórios Financeiros</h1>
        <p class="page-subtitle">Visão abrangente dos fluxos de receita e transações operacionais.</p>
      </div>
      <div class="header-actions">
        <button class="btn-outline">
          <i class="pi pi-download" />
          Exportar PDF
        </button>
        <button class="btn-primary" :disabled="loading" @click="carregar">
          <i class="pi" :class="loading ? 'pi-spin pi-spinner' : 'pi-refresh'" />
          Atualizar
        </button>
      </div>
    </div>

    <!-- ── Grid: sidebar + conteúdo ──────────────────────────────────── -->
    <div class="content-grid">

      <!-- ── Sidebar de filtros ──────────────────────────────────────── -->
      <aside class="sidebar">
        <h2 class="sidebar-title">Critérios de Filtro</h2>

        <div class="filter-group">
          <label class="filter-label">Período</label>
          <input v-model="filtros.dataInicio" type="date" class="filter-input" />
          <input v-model="filtros.dataFim"    type="date" class="filter-input" style="margin-top:6px" />
        </div>

        <div class="filter-group">
          <label class="filter-label">Método de Pagamento</label>
          <div class="check-list">
            <label class="check-row">
              <input v-model="filtros.metodos" type="checkbox" :value="FormaPagamento.Dinheiro" />
              <span>Dinheiro</span>
            </label>
            <label class="check-row">
              <input v-model="filtros.metodos" type="checkbox" :value="FormaPagamento.Cartao" />
              <span>Cartão</span>
            </label>
            <label class="check-row">
              <input v-model="filtros.metodos" type="checkbox" :value="FormaPagamento.Pix" />
              <span>PIX</span>
            </label>
          </div>
        </div>

        <div class="filter-group">
          <label class="filter-label">Status</label>
          <div class="check-list">
            <label class="check-row">
              <input v-model="filtros.status" type="radio" value="todos" />
              <span>Todos</span>
            </label>
            <label class="check-row">
              <input v-model="filtros.status" type="radio" value="pago" />
              <span>Pago</span>
            </label>
            <label class="check-row">
              <input v-model="filtros.status" type="radio" value="aberto" />
              <span>Em Aberto</span>
            </label>
          </div>
        </div>

        <button class="btn-apply" @click="aplicarFiltros">Aplicar Filtros</button>
      </aside>

      <!-- ── Conteúdo principal ───────────────────────────────────────── -->
      <div class="main-content">

        <!-- KPIs (3 cards com stripe colorida no topo) -->
        <div class="kpi-row">

          <div class="kpi-card">
            <div class="kpi-stripe kpi-stripe--blue" />
            <span class="kpi-label">RECEITA TOTAL</span>
            <div class="kpi-bottom">
              <span class="kpi-value">{{ moeda(resumo?.receita_total ?? 0) }}</span>
              <span class="kpi-trend kpi-trend--up">
                <i class="pi pi-arrow-up" />
              </span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-stripe kpi-stripe--gray" />
            <span class="kpi-label">TRANSAÇÕES</span>
            <div class="kpi-bottom">
              <span class="kpi-value kpi-value--num">{{ transacoesFiltradas.length }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-stripe kpi-stripe--orange" />
            <span class="kpi-label">TICKET MÉDIO</span>
            <div class="kpi-bottom">
              <span class="kpi-value">{{ moeda(resumo?.ticket_medio ?? 0) }}</span>
            </div>
          </div>

        </div>

        <!-- Gráfico de barras: receita por dia -->
        <div class="chart-card">
          <div class="chart-card-header">
            <h3 class="chart-title">Receita por Dia</h3>
            <div class="chart-legend">
              <span class="legend-item">
                <span class="legend-dot" style="background:#003c90" />
                Pago
              </span>
              <span class="legend-item">
                <span class="legend-dot" style="background:#d0e1fb" />
                Em Aberto
              </span>
            </div>
          </div>
          <div class="chart-body">
            <div v-if="loadingTrans" class="chart-loading">
              <ProgressSpinner style="width:36px;height:36px" />
            </div>
            <Chart
              v-else
              type="bar"
              :data="barData"
              :options="barOptions"
              class="chart-fill"
            />
          </div>
        </div>

        <!-- Log de transações -->
        <div class="table-card">
          <div class="table-card-header">
            <div class="table-header-left">
              <span class="table-title">Log de Transações</span>
              <span class="table-badge">{{ transacoesFiltradas.length }} Registros</span>
            </div>
            <div class="table-header-actions">
              <button class="btn-sm-outline">
                <i class="pi pi-file-export" />
                Exportar CSV
              </button>
            </div>
          </div>

          <div class="table-scroll">
            <div v-if="loadingTrans" class="table-loading">
              <ProgressSpinner style="width:36px;height:36px" />
            </div>
            <table v-else class="log-table">
              <thead>
                <tr>
                  <th>Data/Hora</th>
                  <th>Placa</th>
                  <th>Vaga</th>
                  <th>Pagamento</th>
                  <th class="th-right">Valor</th>
                  <th class="th-center">Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="transacoesFiltradas.length === 0">
                  <td colspan="6" class="td-empty">Nenhuma transação encontrada.</td>
                </tr>
                <tr
                  v-for="(m, i) in transacoesPaginadas"
                  :key="m.id"
                  :class="i % 2 !== 0 ? 'tr-alt' : ''"
                >
                  <td class="td-time">{{ formatarDataHora(m.data_entrada) }}</td>
                  <td class="td-placa">{{ m.placa_veiculo }}</td>
                  <td><span class="vaga-mono">{{ m.numero_vaga }}</span></td>
                  <td class="td-muted">
                    {{ m.forma_pagamento != null ? FORMA_PAGAMENTO_LABEL[m.forma_pagamento as FormaPagamento] : '—' }}
                  </td>
                  <td class="td-right td-valor">{{ m.valor_total != null ? moeda(m.valor_total) : '—' }}</td>
                  <td class="td-center">
                    <span class="status-pill" :class="m.pago ? 'pill-pago' : 'pill-pendente'">
                      {{ m.pago ? 'Liquidado' : 'Pendente' }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Paginação simples -->
          <div v-if="totalPaginas > 1" class="pagination">
            <button :disabled="paginaAtual === 1" class="page-btn" @click="paginaAtual--">
              <i class="pi pi-chevron-left" />
            </button>
            <span class="page-info">{{ paginaAtual }} / {{ totalPaginas }}</span>
            <button :disabled="paginaAtual === totalPaginas" class="page-btn" @click="paginaAtual++">
              <i class="pi pi-chevron-right" />
            </button>
          </div>
        </div>

      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { FormaPagamento, FORMA_PAGAMENTO_LABEL } from '@/types'
import type { MovimentacaoDto, ResumoFinanceiroDto } from '@/types'
import { relatorioService } from '@/services/relatorioService'

// ── Estado ────────────────────────────────────────────────────────────────
const resumo       = ref<ResumoFinanceiroDto | null>(null)
const transacoes   = ref<MovimentacaoDto[]>([])
const loadingTrans = ref(false)
const loadingRes   = ref(false)
const loading      = computed(() => loadingTrans.value || loadingRes.value)

const filtros = reactive({
  dataInicio: '',
  dataFim: '',
  metodos: [FormaPagamento.Dinheiro, FormaPagamento.Cartao, FormaPagamento.Pix] as FormaPagamento[],
  status: 'todos' as 'todos' | 'pago' | 'aberto',
})

const filtrosAtivos = reactive({ ...filtros })

function aplicarFiltros() {
  Object.assign(filtrosAtivos, { ...filtros })
  paginaAtual.value = 1
}

// ── Filtros client-side ───────────────────────────────────────────────────
const transacoesFiltradas = computed(() => {
  return transacoes.value.filter(m => {
    if (filtrosAtivos.dataInicio && m.data_entrada < filtrosAtivos.dataInicio) return false
    if (filtrosAtivos.dataFim && m.data_entrada > filtrosAtivos.dataFim + 'T23:59:59') return false
    if (m.forma_pagamento != null && !filtrosAtivos.metodos.includes(m.forma_pagamento)) return false
    if (filtrosAtivos.status === 'pago' && !m.pago) return false
    if (filtrosAtivos.status === 'aberto' && m.pago) return false
    return true
  })
})

// ── Paginação ─────────────────────────────────────────────────────────────
const paginaAtual  = ref(1)
const itensPorPagina = 15

const totalPaginas = computed(() =>
  Math.max(1, Math.ceil(transacoesFiltradas.value.length / itensPorPagina))
)

const transacoesPaginadas = computed(() => {
  const start = (paginaAtual.value - 1) * itensPorPagina
  return transacoesFiltradas.value.slice(start, start + itensPorPagina)
})

// ── Carregar dados ────────────────────────────────────────────────────────
async function carregar() {
  loadingRes.value = true
  relatorioService.getResumo()
    .then(r => { resumo.value = r })
    .catch(() => {})
    .finally(() => { loadingRes.value = false })

  loadingTrans.value = true
  relatorioService.listarTransacoes(1, 200)
    .then(r => { transacoes.value = r.items })
    .catch(() => { transacoes.value = [] })
    .finally(() => { loadingTrans.value = false })
}

// ── Bar chart: receita por dia (últimos 7 dias) ───────────────────────────
const barData = computed(() => {
  // Ticket médio das pagas (usado como estimativa para em-aberto sem valor)
  const pagas = transacoesFiltradas.value.filter(m => m.pago && (m.valor_total ?? 0) > 0)
  const avgTicket = pagas.length > 0
    ? pagas.reduce((s, m) => s + m.valor_total!, 0) / pagas.length
    : 15

  const days: string[] = []
  const pagos:   number[] = []
  const abertos: number[] = []

  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    const dateStr = d.toISOString().slice(0, 10)
    days.push(d.toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit' }))

    pagos.push(
      transacoesFiltradas.value
        .filter(m => m.pago && m.data_entrada.startsWith(dateStr))
        .reduce((s, m) => s + (m.valor_total ?? 0), 0)
    )

    // valor_total definido = veículo saiu mas não pagou (valor real)
    // valor_total null    = veículo ainda no pátio (usa ticket médio como estimativa)
    abertos.push(
      transacoesFiltradas.value
        .filter(m => !m.pago && m.data_entrada.startsWith(dateStr))
        .reduce((s, m) => s + (m.valor_total ?? avgTicket), 0)
    )
  }

  return {
    labels: days,
    datasets: [
      {
        label: 'Pago',
        data: pagos,
        backgroundColor: '#003c90',
        borderRadius: 4,
        borderSkipped: false,
      },
      {
        label: 'Em Aberto',
        data: abertos,
        backgroundColor: '#d0e1fb',
        borderRadius: 4,
        borderSkipped: false,
      },
    ],
  }
})

const barOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (ctx: any) => {
          const brl = (v: number) =>
            new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
          if (ctx.dataset.label === 'Em Aberto')
            return ` Em Aberto: ${brl(ctx.raw)} (estimado)`
          return ` Pago: ${brl(ctx.raw)}`
        },
      },
    },
  },
  scales: {
    x: { stacked: true, grid: { display: false }, ticks: { font: { size: 11 }, color: '#64748b' } },
    y: { stacked: true, beginAtZero: true, grid: { color: '#f1f5f9' }, ticks: { font: { size: 11 }, color: '#64748b', callback: (v: any) => `R$${v}` } },
  },
}

// ── Helpers ───────────────────────────────────────────────────────────────
function moeda(v: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
}

function formatarDataHora(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: 'short',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  }).format(new Date(iso))
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(carregar)
</script>

<style scoped>
.relatorios-view {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Header ──────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
}

.page-title {
  margin: 0;
  font-size: 2rem;
  font-weight: 700;
  color: var(--p-surface-900);
  letter-spacing: -0.02em;
}

.page-subtitle {
  margin: 4px 0 0;
  font-size: 14px;
  color: var(--p-surface-500);
}

.header-actions {
  display: flex;
  gap: var(--space-sm);
  flex-shrink: 0;
}

.btn-outline {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border: 1px solid var(--p-surface-300);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--p-surface-600);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-outline:hover { background: var(--p-surface-50); }

.btn-primary {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border: none;
  border-radius: var(--radius-sm);
  background: var(--color-primary);
  color: #fff;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-primary:hover:not(:disabled) { background: #0f52ba; }
.btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

/* ── Layout ───────────────────────────────────────────────────────────── */
.content-grid {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: var(--space-lg);
  align-items: start;
}

@media (max-width: 900px) {
  .content-grid { grid-template-columns: 1fr; }
}

/* ── Sidebar ──────────────────────────────────────────────────────────── */
.sidebar {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.sidebar-title {
  margin: 0 0 var(--space-sm);
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--p-surface-800);
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--p-surface-100);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--p-surface-500);
  text-transform: uppercase;
}

.filter-input {
  height: 38px;
  padding: 0 10px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #faf8ff;
  font-size: 13px;
  color: var(--p-surface-800);
  width: 100%;
  box-sizing: border-box;
}
.filter-input:focus { outline: 2px solid var(--color-primary); outline-offset: -1px; }

.check-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.check-row {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 13px;
  color: var(--p-surface-700);
}
.check-row input { accent-color: var(--color-primary); }

.btn-apply {
  width: 100%;
  padding: 9px;
  border: none;
  border-radius: var(--radius-sm);
  background: var(--p-surface-100);
  color: var(--color-primary);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  cursor: pointer;
  transition: background 0.15s;
  margin-top: auto;
}
.btn-apply:hover { background: var(--p-surface-200); }

/* ── Main content ─────────────────────────────────────────────────────── */
.main-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── KPIs ─────────────────────────────────────────────────────────────── */
.kpi-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
}

@media (max-width: 700px) { .kpi-row { grid-template-columns: 1fr; } }

.kpi-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  position: relative;
  overflow: hidden;
}

.kpi-stripe {
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 4px;
}
.kpi-stripe--blue   { background: #1d59c1; }
.kpi-stripe--gray   { background: #b7c8e1; }
.kpi-stripe--orange { background: #993900; }

.kpi-label {
  display: block;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: var(--p-surface-500);
  text-transform: uppercase;
  margin-bottom: var(--space-sm);
  margin-top: 6px;
}

.kpi-bottom {
  display: flex;
  align-items: baseline;
  gap: var(--space-sm);
}

.kpi-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--p-surface-900);
  letter-spacing: -0.02em;
  line-height: 1;
}

.kpi-value--num { font-size: 2.5rem; }

.kpi-trend--up {
  font-size: 12px;
  color: #16a34a;
  font-weight: 600;
  display: flex;
  align-items: center;
}

/* ── Chart ────────────────────────────────────────────────────────────── */
.chart-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.chart-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-md);
}

.chart-title {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.chart-legend {
  display: flex;
  gap: var(--space-md);
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  color: var(--p-surface-500);
}

.legend-dot {
  width: 12px;
  height: 12px;
  border-radius: 3px;
  flex-shrink: 0;
}

.chart-body {
  height: 240px;
  position: relative;
}

.chart-fill {
  width: 100% !important;
  height: 100% !important;
}

.chart-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 240px;
}

/* ── Table ────────────────────────────────────────────────────────────── */
.table-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  overflow: hidden;
}

.table-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px var(--space-md);
  border-bottom: 1px solid var(--p-surface-200);
  background: #faf8ff;
}

.table-header-left {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.table-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.table-badge {
  background: var(--p-surface-100);
  color: var(--color-primary);
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.03em;
  padding: 2px 8px;
  border-radius: 4px;
}

.table-header-actions { display: flex; gap: var(--space-sm); }

.btn-sm-outline {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 12px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: var(--p-surface-100);
  color: var(--color-primary);
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-sm-outline:hover { background: var(--p-surface-200); }

.table-scroll { overflow-x: auto; }

.table-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 0;
}

.log-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.log-table thead { position: sticky; top: 0; z-index: 1; }

.log-table th {
  padding: 8px 12px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--p-surface-500);
  background: #faf8ff;
  border-bottom: 1px solid var(--p-surface-200);
  white-space: nowrap;
  text-transform: uppercase;
}

.log-table td {
  padding: 12px 12px;
  border-bottom: 1px solid var(--p-surface-100);
  color: var(--p-surface-800);
  height: 48px;
  box-sizing: border-box;
}

.tr-alt td { background: #f8fafc; }
.log-table tr:hover td { background: #f1f3ff; cursor: pointer; }

.th-right { text-align: right; }
.th-center { text-align: center; }
.td-right { text-align: right; }
.td-center { text-align: center; }

.td-empty {
  text-align: center;
  padding: 40px;
  color: var(--p-surface-400);
}

.td-time { color: var(--p-surface-500); white-space: nowrap; }

.td-placa {
  font-family: 'Courier New', monospace;
  font-weight: 700;
  letter-spacing: 0.06em;
}

.vaga-mono {
  display: inline-block;
  background: var(--p-surface-100);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-family: monospace;
  font-weight: 600;
}

.td-muted { color: var(--p-surface-500); }

.td-valor {
  font-weight: 600;
  color: var(--p-surface-900);
  font-size: 14px;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.pill-pago     { background: #dcfce7; color: #166534; }
.pill-pendente { background: #fef08a; color: #854d0e; }

/* ── Paginação ────────────────────────────────────────────────────────── */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-top: 1px solid var(--p-surface-100);
}

.page-btn {
  width: 32px;
  height: 32px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: var(--p-surface-600);
  transition: background 0.15s;
}
.page-btn:hover:not(:disabled) { background: var(--p-surface-100); }
.page-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.page-info {
  font-size: 13px;
  color: var(--p-surface-600);
  min-width: 60px;
  text-align: center;
}
</style>
