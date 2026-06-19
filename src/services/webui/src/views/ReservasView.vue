<template>
  <div class="reservas-view">

    <!-- ── Header ──────────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">Gerenciamento de Reservas</h1>
        <p class="page-subtitle">Visualize e gerencie agendamentos de vagas no sistema.</p>
      </div>
      <button class="btn-nova" @click="focarForm">
        <i class="pi pi-plus" />
        Nova Reserva
      </button>
    </div>

    <!-- ── Grid principal ──────────────────────────────────────────────────── -->
    <div class="content-grid">

      <!-- ── Coluna esquerda: filtros + tabela ─────────────────────────────── -->
      <div class="col-lista">

        <!-- Filtros -->
        <div class="filtros-card">
          <div class="filtro-field">
            <label class="filtro-label">Buscar Reserva</label>
            <div class="search-wrap">
              <i class="pi pi-search search-icon" />
              <input
                v-model="searchVaga"
                type="text"
                placeholder="Nome, CPF ou Placa..."
                class="filtro-input search-field"
              />
            </div>
          </div>

          <div class="filtro-field">
            <label class="filtro-label">Data</label>
            <input v-model="filtroData" type="date" class="filtro-input" />
          </div>

          <div class="filtro-field">
            <label class="filtro-label">Status</label>
            <select v-model="filtroStatus" class="filtro-input filtro-select">
              <option :value="null">Todos</option>
              <option v-for="opt in statusFiltroOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>

          <button class="btn-icon-filter" title="Mais filtros">
            <i class="pi pi-sliders-h" />
          </button>
        </div>

        <!-- Tabela -->
        <div class="tabela-card">
          <div class="tabela-header">
            <span class="tabela-titulo">Próximas Reservas</span>
            <div class="tabela-toggle">
              <button class="toggle-btn active" title="Lista"><i class="pi pi-list" /></button>
              <button class="toggle-btn" title="Grade"><i class="pi pi-th-large" /></button>
            </div>
          </div>

          <div v-if="loading" class="tabela-loading">
            <ProgressSpinner style="width:32px;height:32px" />
          </div>

          <div v-else class="tabela-scroll">
            <table class="reservas-table">
              <thead>
                <tr>
                  <th>Cliente</th>
                  <th>Vaga</th>
                  <th>Entrada Prevista</th>
                  <th>Saída Prevista</th>
                  <th>Status</th>
                  <th class="th-actions">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="reservasPaginadas.length === 0">
                  <td colspan="6" class="td-empty">
                    <i class="pi pi-inbox" />
                    Nenhuma reserva encontrada.
                  </td>
                </tr>
                <tr v-for="r in reservasPaginadas" :key="r.id" class="reserva-row">
                  <td>
                    <div class="cliente-cell">
                      <span class="cliente-nome">Usuário</span>
                      <span class="cliente-id">{{ r.usuario_id.slice(0, 8) }}…</span>
                    </div>
                  </td>
                  <td>
                    <span class="vaga-badge" :class="vagaBadgeClass(r.numero_vaga)">
                      {{ r.numero_vaga }}
                    </span>
                  </td>
                  <td class="td-data">{{ formatarDataHora(r.data_agendada) }}</td>
                  <td class="td-data">{{ formatarDataHora(r.data_limite) }}</td>
                  <td>
                    <span class="status-pill" :class="statusClass(r.status)">
                      <span class="status-dot" />
                      {{ STATUS_RESERVA_LABEL[r.status] }}
                    </span>
                  </td>
                  <td class="td-actions">
                    <div class="action-menu" @click.stop="toggleMenu(r.id)">
                      <button class="btn-menu">⋮</button>
                      <div v-if="menuAberto === r.id" class="dropdown-menu">
                        <button
                          v-if="podeCancelar(r.status)"
                          class="menu-item danger"
                          @click="confirmarCancelamento(r); menuAberto = null"
                        >
                          <i class="pi pi-times" /> Cancelar
                        </button>
                        <button class="menu-item" @click="menuAberto = null">
                          <i class="pi pi-eye" /> Ver detalhes
                        </button>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Paginação -->
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

      <!-- ── Coluna direita: KPIs + form ────────────────────────────────────── -->
      <div class="col-form">

        <!-- KPI cards -->
        <div class="kpi-row">
          <div class="kpi-card">
            <span class="kpi-label">HOJE CONFIRMADAS</span>
            <span class="kpi-num">{{ hojeConfirmadas }}</span>
          </div>
          <div class="kpi-card kpi-card--warn">
            <span class="kpi-label">AÇÕES PENDENTES</span>
            <span class="kpi-num">{{ acoesPendentes }}</span>
          </div>
        </div>

        <!-- Form Nova Reserva Rápida -->
        <div ref="formRef" class="form-panel" :class="{ 'form-highlight': formHighlighted }">
          <div class="form-panel-header">
            <span class="form-panel-icon"><i class="pi pi-plus-circle" /></span>
            <span class="form-panel-titulo">Nova Reserva Rápida</span>
          </div>

          <div class="form-body">
            <div class="field">
              <label>Vaga</label>
              <Select
                v-model="form.vagaId"
                :options="vagasDisponiveis"
                option-label="label"
                option-value="value"
                placeholder="Selecionar vaga disponível..."
                class="w-full"
                filter
                empty-message="Nenhuma vaga disponível"
              />
            </div>

            <div class="grid-2">
              <div class="field">
                <label>Data/Hora Entrada</label>
                <DatePicker
                  v-model="form.dataAgendada"
                  show-time
                  hour-format="24"
                  date-format="dd/mm/yy"
                  placeholder="dd/mm/aaaa hh:mm"
                  class="w-full"
                  :min-date="minDate"
                />
              </div>
              <div class="field">
                <label>Data/Hora Saída</label>
                <DatePicker
                  v-model="form.dataLimite"
                  show-time
                  hour-format="24"
                  date-format="dd/mm/yy"
                  placeholder="dd/mm/aaaa hh:mm"
                  class="w-full"
                  :min-date="form.dataAgendada ?? minDate"
                />
              </div>
            </div>

            <div class="field">
              <label>Observações (Opcional)</label>
              <textarea
                v-model="form.observacoes"
                rows="3"
                class="obs-textarea"
                placeholder="Informações adicionais..."
              />
            </div>

            <Message v-if="erroNova" severity="error" :closable="false">{{ erroNova }}</Message>
          </div>

          <div class="form-footer">
            <button class="btn-limpar" @click="resetForm">Limpar</button>
            <button class="btn-confirmar" :disabled="salvando" @click="salvarReserva">
              <i class="pi pi-check-circle" />
              {{ salvando ? 'Salvando...' : 'Confirmar Reserva' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Dialog: Confirmar Cancelamento ───────────────────────────────────── -->
    <Dialog
      v-model:visible="dialogCancelar"
      header="Cancelar Reserva"
      :modal="true"
      :style="{ width: '380px' }"
    >
      <div v-if="reservaParaCancelar" class="confirm-content">
        <i class="pi pi-exclamation-triangle confirm-icone" />
        <p>
          Confirma o cancelamento da reserva da vaga
          <strong>{{ reservaParaCancelar.numero_vaga }}</strong>
          agendada para
          <strong>{{ formatarDataHora(reservaParaCancelar.data_agendada) }}</strong>?
        </p>
      </div>
      <template #footer>
        <Button label="Não" severity="secondary" text @click="dialogCancelar = false" />
        <Button label="Sim, cancelar" icon="pi pi-times" severity="danger" :loading="cancelando" @click="executarCancelamento" />
      </template>
    </Dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { StatusReserva, StatusVaga, TIPO_VAGA_LABEL, STATUS_RESERVA_LABEL } from '@/types'
import type { ReservaDto } from '@/types'
import { useVagaStore } from '@/stores/vagaStore'
import { useAuthStore } from '@/stores/authStore'
import { reservaService } from '@/services/reservaService'

const vagaStore = useVagaStore()
const authStore = useAuthStore()
const toast     = useToast()

// ── Lista de reservas ────────────────────────────────────────────────────────
const reservas     = ref<ReservaDto[]>([])
const loading      = ref(false)
const searchVaga   = ref('')
const filtroData   = ref('')
const filtroStatus = ref<StatusReserva | null>(null)
const menuAberto   = ref<number | null>(null)

const statusFiltroOptions = [
  StatusReserva.Pendente, StatusReserva.Confirmada, StatusReserva.Cancelada,
  StatusReserva.Expirada, StatusReserva.Utilizada,
].map(v => ({ value: v, label: STATUS_RESERVA_LABEL[v] }))

const hojeStr = new Date().toISOString().slice(0, 10)

const hojeConfirmadas = computed(() =>
  reservas.value.filter(r =>
    r.status === StatusReserva.Confirmada &&
    r.data_agendada.startsWith(hojeStr)
  ).length
)

const acoesPendentes = computed(() =>
  reservas.value.filter(r => r.status === StatusReserva.Pendente).length
)

const reservasFiltradas = computed(() =>
  reservas.value.filter(r => {
    if (searchVaga.value && !r.numero_vaga.toLowerCase().includes(searchVaga.value.toLowerCase())) return false
    if (filtroData.value && !r.data_agendada.startsWith(filtroData.value)) return false
    if (filtroStatus.value !== null && r.status !== filtroStatus.value) return false
    return true
  })
)

// ── Paginação ────────────────────────────────────────────────────────────────
const paginaAtual    = ref(1)
const itensPorPagina = 10

const totalPaginas = computed(() =>
  Math.max(1, Math.ceil(reservasFiltradas.value.length / itensPorPagina))
)

const reservasPaginadas = computed(() => {
  const start = (paginaAtual.value - 1) * itensPorPagina
  return reservasFiltradas.value.slice(start, start + itensPorPagina)
})

async function carregar() {
  loading.value = true
  try {
    const result = await reservaService.listar(1, 200)
    reservas.value = result.items
  } catch {
    reservas.value = []
  } finally {
    loading.value = false
  }
}

// ── Vagas disponíveis ────────────────────────────────────────────────────────
const vagasDisponiveis = computed(() =>
  vagaStore.vagas
    .filter(v => v.status === StatusVaga.Disponivel)
    .map(v => ({
      value: v.id,
      label: `${v.numero}  —  ${TIPO_VAGA_LABEL[v.tipo_vaga]}`,
    }))
)

// ── Form Nova Reserva ────────────────────────────────────────────────────────
const formRef        = ref<HTMLElement | null>(null)
const formHighlighted = ref(false)
const salvando       = ref(false)
const erroNova       = ref<string | null>(null)
const minDate        = new Date()

const form = ref({
  vagaId:       null as number | null,
  dataAgendada: null as Date | null,
  dataLimite:   null as Date | null,
  observacoes:  '',
})

function focarForm() {
  formRef.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  formHighlighted.value = true
  setTimeout(() => { formHighlighted.value = false }, 1200)
}

function resetForm() {
  form.value = { vagaId: null, dataAgendada: null, dataLimite: null, observacoes: '' }
  erroNova.value = null
}

async function salvarReserva() {
  const { vagaId, dataAgendada, dataLimite } = form.value
  if (!vagaId)       { erroNova.value = 'Selecione uma vaga.'; return }
  if (!dataAgendada) { erroNova.value = 'Informe a data de entrada.'; return }
  if (!dataLimite)   { erroNova.value = 'Informe a data de saída.'; return }
  if (dataLimite <= dataAgendada) { erroNova.value = 'Saída deve ser após a entrada.'; return }

  const usuarioId = authStore.user?.id ?? ''
  if (!usuarioId) { erroNova.value = 'Usuário não identificado.'; return }

  salvando.value = true
  erroNova.value = null
  try {
    await reservaService.criar({
      vagaId,
      usuarioId,
      dataAgendada: dataAgendada.toISOString(),
      dataLimite:   dataLimite.toISOString(),
    })
    resetForm()
    toast.add({ severity: 'success', summary: 'Reserva criada', life: 3000 })
    carregar()
    vagaStore.fetchAll()
  } catch (e: any) {
    erroNova.value = e?.response?.data?.errors?.[0] ?? 'Erro ao criar reserva.'
  } finally {
    salvando.value = false
  }
}

// ── Cancelamento ─────────────────────────────────────────────────────────────
const dialogCancelar      = ref(false)
const cancelando          = ref(false)
const reservaParaCancelar = ref<ReservaDto | null>(null)

function confirmarCancelamento(reserva: ReservaDto) {
  reservaParaCancelar.value = reserva
  dialogCancelar.value      = true
}

async function executarCancelamento() {
  if (!reservaParaCancelar.value) return
  cancelando.value = true
  try {
    await reservaService.cancelar(reservaParaCancelar.value.id)
    dialogCancelar.value = false
    toast.add({ severity: 'info', summary: 'Reserva cancelada', life: 3000 })
    carregar()
    vagaStore.fetchAll()
  } catch (e: any) {
    toast.add({ severity: 'error', summary: 'Erro', detail: e?.response?.data?.errors?.[0] ?? 'Não foi possível cancelar.', life: 4000 })
    dialogCancelar.value = false
  } finally {
    cancelando.value = false
  }
}

function toggleMenu(id: number) {
  menuAberto.value = menuAberto.value === id ? null : id
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function podeCancelar(s: StatusReserva) {
  return s === StatusReserva.Pendente || s === StatusReserva.Confirmada
}

const BADGE_PALETTES: Record<string, string> = {
  A: 'badge-blue', B: 'badge-teal', C: 'badge-green',
  D: 'badge-orange', E: 'badge-purple', F: 'badge-rose',
}
function vagaBadgeClass(numero: string): string {
  return BADGE_PALETTES[numero[0].toUpperCase()] ?? 'badge-blue'
}

function statusClass(s: StatusReserva): string {
  const map: Record<StatusReserva, string> = {
    [StatusReserva.Pendente]:   'status-pendente',
    [StatusReserva.Confirmada]: 'status-confirmada',
    [StatusReserva.Cancelada]:  'status-cancelada',
    [StatusReserva.Expirada]:   'status-expirada',
    [StatusReserva.Utilizada]:  'status-utilizada',
  }
  return map[s]
}

function formatarDataHora(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(() => {
  carregar()
  if (vagaStore.vagas.length === 0) vagaStore.fetchAll()
  document.addEventListener('click', () => { menuAberto.value = null })
})
</script>

<style scoped>
.reservas-view {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Header ─────────────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
}

.page-title {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--p-surface-900);
  letter-spacing: -0.02em;
}

.page-subtitle {
  margin: 4px 0 0;
  font-size: var(--font-sm);
  color: var(--p-surface-500);
}

.btn-nova {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 18px;
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s;
  flex-shrink: 0;
}
.btn-nova:hover { background: #0f52ba; }

/* ── Grid principal ─────────────────────────────────────────────────────────── */
.content-grid {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: var(--space-lg);
  align-items: start;
}

@media (max-width: 1000px) {
  .content-grid { grid-template-columns: 1fr; }
}

/* ── Filtros ────────────────────────────────────────────────────────────────── */
.filtros-card {
  display: flex;
  align-items: flex-end;
  gap: var(--space-md);
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: 12px var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  margin-bottom: var(--space-md);
  flex-wrap: wrap;
}

.filtro-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  min-width: 120px;
}

.filtro-field:first-child { flex: 1.8; }

.filtro-label {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  color: var(--p-surface-500);
  text-transform: uppercase;
}

.filtro-input {
  height: 38px;
  padding: 0 10px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: var(--p-surface-50);
  font-size: 13px;
  color: var(--p-surface-800);
  width: 100%;
  box-sizing: border-box;
}
.filtro-input:focus { outline: 2px solid var(--color-primary); outline-offset: -1px; }

.search-wrap { position: relative; }
.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--p-surface-400);
  font-size: 0.85rem;
  pointer-events: none;
}
.search-field { padding-left: 32px !important; }

.filtro-select { cursor: pointer; appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6'%3E%3Cpath d='M0 0l5 6 5-6z' fill='%2364748b'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 10px center; padding-right: 28px !important; }

.btn-icon-filter {
  width: 38px;
  height: 38px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: var(--p-surface-50);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--p-surface-500);
  flex-shrink: 0;
  align-self: flex-end;
}
.btn-icon-filter:hover { background: var(--p-surface-100); }

/* ── Tabela card ────────────────────────────────────────────────────────────── */
.tabela-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  overflow: hidden;
}

.tabela-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--p-surface-100);
}

.tabela-titulo {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.tabela-toggle { display: flex; gap: 4px; }
.toggle-btn {
  width: 30px;
  height: 30px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--p-surface-400);
  font-size: 0.75rem;
  transition: all 0.15s;
}
.toggle-btn.active { background: var(--color-primary); color: #fff; border-color: var(--color-primary); }
.toggle-btn:hover:not(.active) { background: var(--p-surface-50); }

.tabela-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 0;
}

.tabela-scroll { overflow-x: auto; }

/* ── Tabela ─────────────────────────────────────────────────────────────────── */
.reservas-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.reservas-table th {
  padding: 8px 12px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--p-surface-500);
  background: var(--p-surface-50);
  border-bottom: 1px solid var(--p-surface-200);
  text-align: left;
  text-transform: uppercase;
  white-space: nowrap;
}

.reservas-table td {
  padding: 12px 12px;
  border-bottom: 1px solid var(--p-surface-100);
  color: var(--p-surface-800);
  vertical-align: middle;
}

.reserva-row:hover td { background: #f8fafc; }
.reserva-row:last-child td { border-bottom: none; }

.th-actions, .td-actions { text-align: center; width: 60px; }

.td-empty {
  text-align: center;
  padding: 40px;
  color: var(--p-surface-400);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
}

.td-data { white-space: nowrap; color: var(--p-surface-700); }

/* Cliente cell */
.cliente-cell { display: flex; flex-direction: column; gap: 1px; }
.cliente-nome { font-size: 13px; font-weight: 600; color: var(--p-surface-800); }
.cliente-id { font-size: 11px; color: var(--p-surface-400); font-family: monospace; }

/* Vaga badge */
.vaga-badge {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.06em;
}
.badge-blue   { background: #dbeafe; color: #1d4ed8; }
.badge-teal   { background: #ccfbf1; color: #0f766e; }
.badge-green  { background: #dcfce7; color: #166534; }
.badge-orange { background: #fed7aa; color: #c2410c; }
.badge-purple { background: #ede9fe; color: #6d28d9; }
.badge-rose   { background: #ffe4e6; color: #be123c; }

/* Status pill */
.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 10px;
  border-radius: 100px;
  font-size: 12px;
  font-weight: 600;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.status-confirmada { background: #dcfce7; color: #166534; }
.status-confirmada .status-dot { background: #16a34a; }

.status-pendente { background: #fef9c3; color: #854d0e; }
.status-pendente .status-dot { background: #ca8a04; }

.status-cancelada { background: var(--p-surface-100); color: var(--p-surface-500); }
.status-cancelada .status-dot { background: #ef4444; }

.status-expirada { background: #fee2e2; color: #991b1b; }
.status-expirada .status-dot { background: #dc2626; }

.status-utilizada { background: #dbeafe; color: #1d4ed8; }
.status-utilizada .status-dot { background: #3b82f6; }

/* Menu de ações */
.action-menu { position: relative; display: inline-block; }

.btn-menu {
  width: 28px;
  height: 28px;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 1.1rem;
  color: var(--p-surface-500);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}
.btn-menu:hover { background: var(--p-surface-100); color: var(--p-surface-700); }

.dropdown-menu {
  position: absolute;
  right: 0;
  top: 32px;
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  box-shadow: 0 4px 12px rgba(0,0,0,0.12);
  z-index: 100;
  min-width: 150px;
  overflow: hidden;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  padding: 9px 14px;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 13px;
  color: var(--p-surface-700);
  text-align: left;
  transition: background 0.12s;
}
.menu-item:hover { background: var(--p-surface-50); }
.menu-item.danger { color: #dc2626; }
.menu-item.danger:hover { background: #fee2e2; }

/* ── Paginação ──────────────────────────────────────────────────────────────── */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-top: 1px solid var(--p-surface-100);
}

.page-btn {
  width: 30px;
  height: 30px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  color: var(--p-surface-600);
}
.page-btn:hover:not(:disabled) { background: var(--p-surface-100); }
.page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.page-info { font-size: 12px; color: var(--p-surface-500); min-width: 50px; text-align: center; }

/* ── Coluna direita ─────────────────────────────────────────────────────────── */
.col-form { display: flex; flex-direction: column; gap: var(--space-md); }

/* KPI row */
.kpi-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-sm); }

.kpi-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: 12px var(--space-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.kpi-card--warn { background: #fefce8; border-color: #fef08a; }

.kpi-label {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.07em;
  color: var(--p-surface-500);
  text-transform: uppercase;
}

.kpi-card--warn .kpi-label { color: #92400e; }

.kpi-num {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--color-primary);
  line-height: 1;
}

.kpi-card--warn .kpi-num { color: #ca8a04; }

/* Form panel */
.form-panel {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  overflow: hidden;
  transition: box-shadow 0.3s, border-color 0.3s;
}

.form-panel.form-highlight {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(0,60,144,0.12);
}

.form-panel-header {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--p-surface-100);
  background: var(--p-surface-50);
}

.form-panel-icon {
  width: 28px;
  height: 28px;
  background: rgba(0,60,144,0.08);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  font-size: 0.85rem;
}

.form-panel-titulo {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.form-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-md);
}

.field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.field label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--p-surface-700);
}

.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-sm); }

.obs-textarea {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: var(--p-surface-50);
  font-size: 13px;
  color: var(--p-surface-800);
  resize: vertical;
  box-sizing: border-box;
  font-family: inherit;
}
.obs-textarea:focus { outline: 2px solid var(--color-primary); outline-offset: -1px; }

.form-footer {
  display: flex;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md) var(--space-md);
}

.btn-limpar {
  flex: 1;
  padding: 9px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--p-surface-600);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-limpar:hover { background: var(--p-surface-50); }

.btn-confirmar {
  flex: 2;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 9px;
  border: none;
  border-radius: var(--radius-sm);
  background: var(--color-primary);
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-confirmar:hover:not(:disabled) { background: #0f52ba; }
.btn-confirmar:disabled { opacity: 0.7; cursor: not-allowed; }

/* ── Dialog confirm ─────────────────────────────────────────────────────────── */
.confirm-content {
  display: flex;
  align-items: flex-start;
  gap: var(--space-md);
  padding: var(--space-sm) 0;
}

.confirm-icone {
  font-size: 1.75rem;
  color: #f59e0b;
  flex-shrink: 0;
  margin-top: 2px;
}

.confirm-content p {
  margin: 0;
  font-size: var(--font-sm);
  color: var(--p-surface-700);
  line-height: 1.6;
}

.w-full { width: 100%; }
</style>
