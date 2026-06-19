<template>
  <div class="dashboard-view">

    <!-- ── KPIs ────────────────────────────────────────────────────────── -->
    <div class="kpi-grid">

      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">TAXA DE OCUPAÇÃO</span>
          <i class="pi pi-chart-pie kpi-icon" />
        </div>
        <div class="kpi-bottom">
          <span class="kpi-value">{{ taxaOcupacao }}%</span>
          <span class="kpi-sub">{{ vagasOcupadas }} de {{ totalVagas }} vagas</span>
        </div>
      </div>

      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">RECEITA DIÁRIA</span>
          <i class="pi pi-wallet kpi-icon" />
        </div>
        <div class="kpi-bottom">
          <span class="kpi-value kpi-value--currency">{{ formatCurrency(resumo?.receita_hoje ?? 0) }}</span>
        </div>
      </div>

      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">RESERVAS ATIVAS</span>
          <i class="pi pi-bookmark kpi-icon" />
        </div>
        <div class="kpi-bottom">
          <span class="kpi-value">{{ resumo?.reservas_ativas ?? 0 }}</span>
        </div>
      </div>

      <div class="kpi-card kpi-card--green">
        <div class="kpi-top">
          <span class="kpi-label">VAGAS DISPONÍVEIS</span>
          <i class="pi pi-check-circle kpi-icon kpi-icon--green" />
        </div>
        <div class="kpi-bottom">
          <span class="kpi-value">{{ vagasDisponiveis }}</span>
          <span class="kpi-sub">/ {{ totalVagas }} total</span>
        </div>
      </div>

    </div>

    <!-- ── Charts ──────────────────────────────────────────────────────── -->
    <div class="charts-grid">

      <!-- Line: tendência de ocupação 7 dias (2/3) -->
      <div class="chart-card">
        <div class="chart-card-header">
          <h3 class="chart-title">Tendência de Ocupação</h3>
          <span class="chart-badge">Últimos 7 dias</span>
        </div>
        <div class="line-chart-body">
          <div v-if="loadingMov" class="chart-center">
            <ProgressSpinner style="width:36px;height:36px" />
          </div>
          <Chart
            v-else
            type="line"
            :data="lineData"
            :options="lineOptions"
            class="chart-fill"
          />
        </div>
      </div>

      <!-- Doughnut: métodos de pagamento (1/3) -->
      <div class="chart-card">
        <h3 class="chart-title">Métodos de Pagamento</h3>
        <div class="doughnut-body">
          <div v-if="loadingMov" class="chart-center">
            <ProgressSpinner style="width:36px;height:36px" />
          </div>
          <template v-else-if="totalPago > 0">
            <div class="doughnut-wrap">
              <Chart
                type="doughnut"
                :data="doughnutData"
                :options="doughnutOptions"
                class="chart-fill"
              />
            </div>
            <div class="doughnut-legend">
              <div v-for="item in paymentLegend" :key="item.label" class="legend-row">
                <span class="legend-dot" :style="{ background: item.color }" />
                <span class="legend-text">{{ item.label }} ({{ item.pct }}%)</span>
              </div>
            </div>
          </template>
          <div v-else class="chart-center">
            <span class="chart-empty">Sem pagamentos registrados</span>
          </div>
        </div>
      </div>

    </div>

    <!-- ── Transações Recentes ─────────────────────────────────────────── -->
    <div class="table-card">
      <div class="table-card-header">
        <h3 class="chart-title">Transações Recentes</h3>
        <span class="table-badge">{{ transacoes.length }} registros</span>
      </div>
      <div class="table-scroll">
        <div v-if="loadingMov" class="table-loading">
          <ProgressSpinner style="width:36px;height:36px" />
        </div>
        <table v-else class="trans-table">
          <thead>
            <tr>
              <th>PLACA</th>
              <th>HORÁRIO</th>
              <th>VAGA</th>
              <th>VALOR</th>
              <th>STATUS</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="transacoes.length === 0">
              <td colspan="5" class="td-empty">Nenhuma transação registrada.</td>
            </tr>
            <tr v-for="(m, i) in transacoes" :key="m.id" :class="i % 2 === 0 ? 'tr-even' : 'tr-odd'">
              <td class="td-placa">{{ m.placa_veiculo }}</td>
              <td class="td-time">{{ formatarDataHora(m.data_entrada) }}</td>
              <td class="td-vaga"><span class="vaga-mono">{{ m.numero_vaga }}</span></td>
              <td class="td-valor">{{ m.valor_total != null ? formatCurrency(m.valor_total) : '—' }}</td>
              <td class="td-status">
                <span class="status-pill" :class="m.pago ? 'pill-pago' : 'pill-pendente'">
                  {{ m.pago ? 'Pago' : 'Pendente' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { StatusVaga, FormaPagamento, FORMA_PAGAMENTO_LABEL } from '@/types'
import type { MovimentacaoDto, ResumoFinanceiroDto } from '@/types'
import { useVagaStore } from '@/stores/vagaStore'
import { movimentacaoService } from '@/services/movimentacaoService'
import { relatorioService } from '@/services/relatorioService'

const vagaStore = useVagaStore()

// ── KPIs de vagas ─────────────────────────────────────────────────────────
const totalVagas       = computed(() => vagaStore.vagas.length)
const vagasOcupadas    = computed(() => vagaStore.vagas.filter(v => v.status === StatusVaga.Ocupada).length)
const vagasDisponiveis = computed(() => vagaStore.vagas.filter(v => v.status === StatusVaga.Disponivel).length)
const taxaOcupacao     = computed(() => {
  if (totalVagas.value === 0) return 0
  return Math.round((vagasOcupadas.value / totalVagas.value) * 100)
})

// ── Resumo financeiro ─────────────────────────────────────────────────────
const resumo = ref<ResumoFinanceiroDto | null>(null)

async function fetchResumo() {
  try { resumo.value = await relatorioService.getResumo() } catch { /* noop */ }
}

// ── Movimentações (todas) ─────────────────────────────────────────────────
const transacoes = ref<MovimentacaoDto[]>([])
const loadingMov = ref(false)

async function fetchMovimentacoes() {
  loadingMov.value = true
  try {
    const result = await movimentacaoService.listarTodas(1, 20)
    transacoes.value = result.items
  } catch {
    transacoes.value = []
  } finally {
    loadingMov.value = false
  }
}

// ── Line chart: entradas por dia (últimos 7 dias) ─────────────────────────
const lineData = computed(() => {
  const days: string[] = []
  const counts: number[] = []
  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    const dateStr = d.toISOString().slice(0, 10)
    days.push(d.toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit' }))
    counts.push(transacoes.value.filter(m => m.data_entrada.startsWith(dateStr)).length)
  }
  return {
    labels: days,
    datasets: [{
      label: 'Entradas',
      data: counts,
      borderColor: '#003c90',
      backgroundColor: 'rgba(0,60,144,0.08)',
      borderWidth: 2,
      tension: 0.4,
      fill: true,
      pointBackgroundColor: '#003c90',
      pointRadius: 4,
      pointHoverRadius: 6,
    }],
  }
})

const lineOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: { mode: 'index', intersect: false },
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { font: { size: 11 }, color: '#64748b' },
    },
    y: {
      beginAtZero: true,
      ticks: { stepSize: 1, font: { size: 11 }, color: '#64748b' },
      grid: { color: '#f1f5f9' },
    },
  },
}

// ── Doughnut: métodos de pagamento ────────────────────────────────────────
const PAYMENT_COLORS = ['#003c90', '#10b981', '#f59e0b']

const totalPago = computed(() =>
  transacoes.value.filter(m => m.pago && m.forma_pagamento != null).length
)

const paymentCounts = computed(() => {
  const map = new Map<FormaPagamento, number>()
  transacoes.value.forEach(m => {
    if (m.pago && m.forma_pagamento != null) {
      map.set(m.forma_pagamento, (map.get(m.forma_pagamento) ?? 0) + 1)
    }
  })
  return map
})

const paymentLegend = computed(() => {
  const total = totalPago.value || 1
  return [FormaPagamento.Dinheiro, FormaPagamento.Cartao, FormaPagamento.Pix].map((fp, i) => ({
    label: FORMA_PAGAMENTO_LABEL[fp],
    color: PAYMENT_COLORS[i],
    pct: Math.round(((paymentCounts.value.get(fp) ?? 0) / total) * 100),
  }))
})

const doughnutData = computed(() => ({
  labels: paymentLegend.value.map(p => p.label),
  datasets: [{
    data: [FormaPagamento.Dinheiro, FormaPagamento.Cartao, FormaPagamento.Pix]
      .map(fp => paymentCounts.value.get(fp) ?? 0),
    backgroundColor: PAYMENT_COLORS,
    borderWidth: 2,
    borderColor: '#fff',
    hoverOffset: 6,
  }],
}))

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
  },
  cutout: '70%',
}

// ── Helpers ───────────────────────────────────────────────────────────────
function formatarDataHora(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: '2-digit',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(() => {
  if (vagaStore.vagas.length === 0) vagaStore.fetchAll()
  fetchMovimentacoes()
  fetchResumo()
})
</script>

<style scoped>
.dashboard-view {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── KPIs ──────────────────────────────────────────────────────────────── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-md);
}

@media (max-width: 1024px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 640px)  { .kpi-grid { grid-template-columns: 1fr; } }

.kpi-card {
  background: #ffffff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  height: 128px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  transition: box-shadow 0.2s;
}

.kpi-card:hover {
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
}

.kpi-card--green {
  border-top: 4px solid #22c55e;
}

.kpi-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.kpi-label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: var(--p-surface-500);
  text-transform: uppercase;
  line-height: 1.3;
}

.kpi-icon {
  font-size: 1.25rem;
  color: var(--color-primary);
}

.kpi-icon--green {
  color: #22c55e;
}

.kpi-bottom {
  display: flex;
  align-items: flex-end;
  gap: var(--space-sm);
}

.kpi-value {
  font-size: 48px;
  font-weight: 700;
  line-height: 1;
  letter-spacing: -0.02em;
  color: var(--p-surface-900);
}

.kpi-value--currency {
  font-size: 32px;
}

.kpi-sub {
  font-size: 12px;
  color: var(--p-surface-500);
  padding-bottom: 4px;
  white-space: nowrap;
}

/* ── Charts ────────────────────────────────────────────────────────────── */
.charts-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: var(--space-md);
}

@media (max-width: 900px) { .charts-grid { grid-template-columns: 1fr; } }

.chart-card {
  background: #ffffff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.08);
  display: flex;
  flex-direction: column;
}

.chart-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-md);
}

.chart-title {
  margin: 0 0 var(--space-md);
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.chart-card-header .chart-title {
  margin: 0;
}

.chart-badge {
  font-size: 11px;
  font-weight: 600;
  color: var(--p-surface-500);
  background: var(--p-surface-100);
  padding: 3px 10px;
  border-radius: 20px;
}

.line-chart-body {
  flex: 1;
  height: 240px;
  position: relative;
}

.chart-fill {
  width: 100% !important;
  height: 100% !important;
}

.doughnut-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  min-height: 240px;
}

.doughnut-wrap {
  width: 160px;
  height: 160px;
  position: relative;
}

.chart-center {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 240px;
}

.chart-empty {
  font-size: 13px;
  color: var(--p-surface-400);
}

.doughnut-legend {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}

.legend-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.legend-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}

.legend-text {
  font-size: 12px;
  color: var(--p-surface-500);
}

/* ── Table ─────────────────────────────────────────────────────────────── */
.table-card {
  background: #ffffff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.08);
  overflow: hidden;
}

.table-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md);
  border-bottom: 1px solid var(--p-surface-200);
  background: #faf8ff;
}

.table-card-header .chart-title { margin: 0; }

.table-badge {
  font-size: 11px;
  font-weight: 600;
  color: var(--p-surface-500);
  background: var(--p-surface-100);
  padding: 3px 10px;
  border-radius: 20px;
}

.table-scroll {
  overflow-x: auto;
}

.table-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 0;
}

.trans-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.trans-table th {
  padding: 12px 16px;
  text-align: left;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: var(--p-surface-500);
  background: var(--p-surface-100);
  border-bottom: 2px solid var(--p-surface-200);
  text-transform: uppercase;
  white-space: nowrap;
}

.trans-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--p-surface-100);
  color: var(--p-surface-800);
}

.tr-even { background: #ffffff; }
.tr-odd  { background: #f8fafc; }

.trans-table tr:hover td { background: #f1f5f9; }

.td-empty {
  text-align: center;
  padding: 32px;
  color: var(--p-surface-400);
}

.td-placa {
  font-family: 'Courier New', monospace;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.td-time { color: var(--p-surface-500); }

.vaga-mono {
  display: inline-block;
  background: var(--p-surface-100);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-family: monospace;
  font-weight: 600;
}

.td-valor { font-weight: 600; color: var(--color-primary); }

.status-pill {
  display: inline-block;
  padding: 2px 10px;
  border-radius: 20px;
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.pill-pago    { background: #dcfce7; color: #166534; }
.pill-pendente { background: #fef08a; color: #854d0e; }
</style>
