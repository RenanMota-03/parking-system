<template>
  <div class="vagas-view">

    <!-- ── Header ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">Mapa de Vagas</h1>
        <p class="page-subtitle">{{ totalLabel }}</p>
      </div>
      <div class="header-actions">
        <Button
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          :loading="vagaStore.loading"
          title="Atualizar"
          @click="vagaStore.fetchAll()"
        />
        <Button
          v-if="authStore.isAdmin"
          label="Nova Vaga"
          icon="pi pi-plus"
          @click="abrirDialogNova"
        />
      </div>
    </div>

    <!-- ── Filtros ─────────────────────────────────────────────────────── -->
    <div class="filtros-bar">
      <InputText
        v-model="searchText"
        placeholder="Buscar por número..."
        class="filtro-search"
      />

      <Select
        v-model="filtroTipo"
        :options="tipoOptions"
        option-label="label"
        option-value="value"
        placeholder="Tipo de vaga"
        class="filtro-tipo"
        show-clear
      />

      <div class="filtro-status-group">
        <button
          v-for="opt in statusOptions"
          :key="opt.value"
          class="status-btn"
          :class="{ active: filtroStatus === opt.value }"
          :style="filtroStatus === opt.value ? { '--btn-color': opt.color } : {}"
          @click="toggleStatus(opt.value)"
        >
          <span class="status-dot" :style="{ background: opt.color }" />
          {{ opt.label }}
        </button>
      </div>
    </div>

    <!-- ── Resumo de contagens ─────────────────────────────────────────── -->
    <div class="stats-row">
      <div v-for="s in statsSummary" :key="s.label" class="stat-chip">
        <span class="stat-dot" :style="{ background: s.color }" />
        <span class="stat-count">{{ s.count }}</span>
        <span class="stat-label">{{ s.label }}</span>
      </div>
    </div>

    <!-- ── Grade de vagas ─────────────────────────────────────────────── -->
    <div v-if="vagaStore.loading" class="loading-state">
      <ProgressSpinner />
    </div>

    <div v-else-if="vagasFiltradas.length === 0" class="empty-state">
      <i class="pi pi-inbox" />
      <p>Nenhuma vaga encontrada.</p>
    </div>

    <div v-else class="vagas-grid">
      <VagaCard
        v-for="vaga in vagasFiltradas"
        :key="vaga.id"
        :vaga="vaga"
        :clickable="authStore.isAdmin"
        @click="authStore.isAdmin && abrirDialogStatus(vaga)"
      />
    </div>

    <!-- ── Dialog: Nova Vaga ───────────────────────────────────────────── -->
    <Dialog
      v-model:visible="dialogNova"
      header="Nova Vaga"
      :modal="true"
      :style="{ width: '380px' }"
      @hide="resetForm"
    >
      <div class="dialog-form">
        <div class="field">
          <label>Número da Vaga</label>
          <InputText
            v-model="novaVaga.numero"
            placeholder="Ex: A1, B12..."
            class="w-full"
            :class="{ 'p-invalid': erroNova }"
            @keyup.enter="salvarNovaVaga"
          />
        </div>
        <div class="field">
          <label>Tipo</label>
          <Select
            v-model="novaVaga.tipoVaga"
            :options="tipoOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <Message v-if="erroNova" severity="error" :closable="false">{{ erroNova }}</Message>
      </div>

      <template #footer>
        <Button label="Cancelar" severity="secondary" text @click="dialogNova = false" />
        <Button label="Cadastrar" icon="pi pi-check" :loading="salvando" @click="salvarNovaVaga" />
      </template>
    </Dialog>

    <!-- ── Dialog: Alterar Status ─────────────────────────────────────── -->
    <Dialog
      v-model:visible="dialogStatus"
      :header="`Vaga ${vagaSelecionada?.numero}`"
      :modal="true"
      :style="{ width: '360px' }"
    >
      <div v-if="vagaSelecionada" class="dialog-form">
        <p class="status-info">
          Status atual:
          <Tag :value="STATUS_VAGA_LABEL[vagaSelecionada.status]" :severity="statusSeverity(vagaSelecionada.status)" />
        </p>
        <div class="field">
          <label>Novo Status</label>
          <Select
            v-model="novoStatus"
            :options="statusAlterarOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <Message v-if="erroStatus" severity="error" :closable="false">{{ erroStatus }}</Message>
      </div>

      <template #footer>
        <Button label="Cancelar" severity="secondary" text @click="dialogStatus = false" />
        <Button label="Confirmar" icon="pi pi-check" :loading="alterando" @click="confirmarAlterarStatus" />
      </template>
    </Dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { TipoVaga, StatusVaga, TIPO_VAGA_LABEL, STATUS_VAGA_LABEL } from '@/types'
import type { VagaDto } from '@/types'
import { useVagaStore } from '@/stores/vagaStore'
import { useAuthStore } from '@/stores/authStore'
import VagaCard from '@/components/shared/VagaCard.vue'

const vagaStore = useVagaStore()
const authStore = useAuthStore()
const toast     = useToast()

// ── Filtros ──────────────────────────────────────────────────────────────
const searchText   = ref('')
const filtroTipo   = ref<TipoVaga | null>(null)
const filtroStatus = ref<StatusVaga | null>(null)

const tipoOptions = Object.entries(TIPO_VAGA_LABEL).map(([v, label]) => ({
  value: Number(v) as TipoVaga,
  label,
}))

const statusOptions = [
  { value: StatusVaga.Disponivel, label: 'Disponível', color: 'var(--color-available)'   },
  { value: StatusVaga.Ocupada,    label: 'Ocupada',    color: 'var(--color-occupied)'    },
  { value: StatusVaga.Reservada,  label: 'Reservada',  color: 'var(--color-reserved)'    },
  { value: StatusVaga.Manutencao, label: 'Manutenção', color: 'var(--color-maintenance)' },
]

const statusAlterarOptions = statusOptions.map(o => ({ value: o.value, label: o.label }))

function toggleStatus(v: StatusVaga) {
  filtroStatus.value = filtroStatus.value === v ? null : v
}

// ── Vagas filtradas ───────────────────────────────────────────────────────
const vagasFiltradas = computed(() =>
  vagaStore.vagas.filter(v => {
    if (searchText.value && !v.numero.toLowerCase().includes(searchText.value.toLowerCase())) return false
    if (filtroTipo.value !== null && v.tipo_vaga !== filtroTipo.value) return false
    if (filtroStatus.value !== null && v.status !== filtroStatus.value) return false
    return true
  })
)

const totalLabel = computed(() => {
  const t = vagaStore.vagas.length
  const f = vagasFiltradas.value.length
  return t === f ? `${t} vagas no total` : `${f} de ${t} vagas`
})

// ── Resumo de contagens ───────────────────────────────────────────────────
const statsSummary = computed(() =>
  statusOptions.map(opt => ({
    label: opt.label,
    color: opt.color,
    count: vagaStore.vagas.filter(v => v.status === opt.value).length,
  }))
)

// ── Dialog Nova Vaga ──────────────────────────────────────────────────────
const dialogNova = ref(false)
const salvando   = ref(false)
const erroNova   = ref<string | null>(null)
const novaVaga   = ref({ numero: '', tipoVaga: TipoVaga.Carro })

function abrirDialogNova() {
  dialogNova.value = true
}

function resetForm() {
  novaVaga.value = { numero: '', tipoVaga: TipoVaga.Carro }
  erroNova.value = null
}

async function salvarNovaVaga() {
  if (!novaVaga.value.numero.trim()) {
    erroNova.value = 'Número da vaga é obrigatório.'
    return
  }
  salvando.value = true
  erroNova.value = null
  const err = await vagaStore.cadastrar({
    numero:   novaVaga.value.numero.trim().toUpperCase(),
    tipoVaga: novaVaga.value.tipoVaga,
  })
  salvando.value = false
  if (err) {
    erroNova.value = err
  } else {
    dialogNova.value = false
    toast.add({ severity: 'success', summary: 'Vaga cadastrada', life: 3000 })
  }
}

// ── Dialog Alterar Status ─────────────────────────────────────────────────
const dialogStatus    = ref(false)
const alterando       = ref(false)
const erroStatus      = ref<string | null>(null)
const vagaSelecionada = ref<VagaDto | null>(null)
const novoStatus      = ref<StatusVaga>(StatusVaga.Disponivel)

function abrirDialogStatus(vaga: VagaDto) {
  vagaSelecionada.value = vaga
  novoStatus.value      = vaga.status
  erroStatus.value      = null
  dialogStatus.value    = true
}

async function confirmarAlterarStatus() {
  if (!vagaSelecionada.value) return
  alterando.value  = true
  erroStatus.value = null
  const err = await vagaStore.alterarStatus(vagaSelecionada.value.id, novoStatus.value)
  alterando.value = false
  if (err) {
    erroStatus.value = err
  } else {
    dialogStatus.value = false
    toast.add({ severity: 'success', summary: 'Status atualizado', life: 3000 })
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────
function statusSeverity(s: StatusVaga): string {
  const map: Record<StatusVaga, string> = {
    [StatusVaga.Disponivel]: 'success',
    [StatusVaga.Ocupada]:    'danger',
    [StatusVaga.Reservada]:  'warn',
    [StatusVaga.Manutencao]: 'secondary',
  }
  return map[s]
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(() => vagaStore.fetchAll())
</script>

<style scoped>
.vagas-view {
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

.header-actions {
  display: flex;
  gap: var(--space-sm);
  align-items: center;
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

.filtros-bar {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  align-items: center;
}

.filtro-search { min-width: 220px; flex: 1; }
.filtro-tipo   { min-width: 180px; }

.filtro-status-group {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.status-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border: 1px solid var(--p-surface-200);
  border-radius: 20px;
  background: #fff;
  font-size: var(--font-sm);
  font-weight: 500;
  color: var(--p-surface-600);
  cursor: pointer;
  transition: all 0.15s;
}

.status-btn:hover {
  border-color: var(--btn-color, var(--color-primary));
  color: var(--btn-color, var(--color-primary));
}

.status-btn.active {
  background: color-mix(in srgb, var(--btn-color) 12%, white);
  border-color: var(--btn-color);
  color: var(--btn-color);
  font-weight: 600;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.stats-row {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.stat-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 14px;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
  border-radius: 20px;
  font-size: var(--font-sm);
}

.stat-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.stat-count {
  font-weight: 700;
  color: var(--p-surface-800);
}

.stat-label { color: var(--p-surface-500); }

.vagas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: var(--space-sm);
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: var(--space-2xl) 0;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-2xl) 0;
  color: var(--p-surface-400);
}

.empty-state i { font-size: 2.5rem; }

.dialog-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding-top: var(--space-sm);
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field label {
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--p-surface-700);
}

.status-info {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin: 0;
  font-size: var(--font-sm);
  color: var(--p-surface-600);
}

.w-full { width: 100%; }
</style>
