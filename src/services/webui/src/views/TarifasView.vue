<template>
  <div class="tarifas-view">

    <!-- ── Header ──────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">Tarifas</h1>
        <p class="page-subtitle">Valores praticados por tipo de vaga</p>
      </div>
      <Button
        v-if="authStore.isAdmin"
        label="Nova Tarifa"
        icon="pi pi-plus"
        @click="abrirDialogNova"
      />
    </div>

    <!-- ── Loading ─────────────────────────────────────────────────────── -->
    <div v-if="loading" class="loading-state">
      <ProgressSpinner />
    </div>

    <!-- ── Grid de cards ───────────────────────────────────────────────── -->
    <div v-else class="tarifas-grid">

      <!-- Card por tipo cadastrado -->
      <div
        v-for="tarifa in tarifas"
        :key="tarifa.id"
        class="tarifa-card"
        :style="{ '--tipo-color': tipoColor(tarifa.tipo_vaga) }"
      >
        <div class="card-header">
          <div class="card-icon-wrap">
            <i :class="`pi ${tipoIcon(tarifa.tipo_vaga)}`" />
          </div>
          <div class="card-titulo-wrap">
            <span class="card-tipo">{{ TIPO_VAGA_LABEL[tarifa.tipo_vaga] }}</span>
            <span v-if="tarifa.vigente_ate" class="card-validade">
              Válida até {{ formatarData(tarifa.vigente_ate) }}
            </span>
            <span v-else class="card-validade">Sem validade definida</span>
          </div>
        </div>

        <div class="card-valores">
          <div class="valor-row">
            <span class="valor-label">Por hora</span>
            <span class="valor-num">{{ moeda(tarifa.valor_hora) }}</span>
          </div>
          <div class="valor-sep" />
          <div class="valor-row valor-row-destaque">
            <span class="valor-label">Diária</span>
            <span class="valor-num">{{ moeda(tarifa.valor_diaria) }}</span>
          </div>
          <div class="valor-sep" />
          <div class="valor-row">
            <span class="valor-label">Mensal</span>
            <span class="valor-num">{{ moeda(tarifa.valor_mensal) }}</span>
          </div>
        </div>

        <div class="card-footer">
          <span class="card-data">Criada em {{ formatarData(tarifa.created_at) }}</span>
          <Button
            v-if="authStore.isAdmin"
            icon="pi pi-pencil"
            severity="secondary"
            text
            size="small"
            label="Editar"
            @click="abrirDialogEditar(tarifa)"
          />
        </div>
      </div>

      <!-- Cards fantasmas para tipos sem tarifa (Admin) -->
      <div
        v-if="authStore.isAdmin"
        v-for="tipo in tiposSemTarifa"
        :key="`sem-${tipo}`"
        class="tarifa-card tarifa-card-vazia"
        :style="{ '--tipo-color': tipoColor(tipo) }"
      >
        <div class="card-header">
          <div class="card-icon-wrap card-icon-wrap-vazio">
            <i :class="`pi ${tipoIcon(tipo)}`" />
          </div>
          <div class="card-titulo-wrap">
            <span class="card-tipo">{{ TIPO_VAGA_LABEL[tipo] }}</span>
            <span class="card-validade card-sem-tarifa">Sem tarifa cadastrada</span>
          </div>
        </div>

        <div class="card-vazio-body">
          <p>Nenhuma tarifa configurada para este tipo de vaga.</p>
          <Button
            label="Configurar tarifa"
            icon="pi pi-plus"
            size="small"
            outlined
            @click="abrirDialogNovaPara(tipo)"
          />
        </div>
      </div>

    </div>

    <!-- ── Tabela de histórico ────────────────────────────────────────── -->
    <div v-if="!loading" class="historico-card">
      <h2 class="historico-title">Histórico de Tarifas</h2>
      <DataTable :value="tarifas" size="small">
        <template #empty>
          <div class="table-empty">
            <i class="pi pi-dollar" />
            <span>Nenhuma tarifa cadastrada.</span>
          </div>
        </template>

        <Column header="Tipo">
          <template #body="{ data }">
            <div class="tipo-cell">
              <i :class="`pi ${tipoIcon(data.tipo_vaga)}`" class="tipo-icon" :style="{ color: tipoColor(data.tipo_vaga) }" />
              <span>{{ TIPO_VAGA_LABEL[data.tipo_vaga as TipoVaga] }}</span>
            </div>
          </template>
        </Column>

        <Column header="Valor/Hora" style="width:130px">
          <template #body="{ data }">{{ moeda(data.valor_hora) }}</template>
        </Column>

        <Column header="Diária" style="width:130px">
          <template #body="{ data }">{{ moeda(data.valor_diaria) }}</template>
        </Column>

        <Column header="Mensal" style="width:130px">
          <template #body="{ data }">{{ moeda(data.valor_mensal) }}</template>
        </Column>

        <Column header="Vigência" style="width:160px">
          <template #body="{ data }">
            <span v-if="data.vigente_ate" class="vigencia-data">{{ formatarData(data.vigente_ate) }}</span>
            <span v-else class="vigencia-sem">Indeterminada</span>
          </template>
        </Column>

        <Column header="Status" style="width:110px">
          <template #body="{ data }">
            <Tag
              :value="isAtiva(data) ? 'Ativa' : 'Expirada'"
              :severity="isAtiva(data) ? 'success' : 'secondary'"
              rounded
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- ── Dialog: Nova / Editar Tarifa ───────────────────────────────── -->
    <Dialog
      v-model:visible="dialogForm"
      :header="editando ? `Editar Tarifa — ${TIPO_VAGA_LABEL[form.tipoVaga]}` : 'Nova Tarifa'"
      :modal="true"
      :style="{ width: '440px' }"
      @hide="resetForm"
    >
      <div class="dialog-form">

        <div v-if="!editando" class="field">
          <label>Tipo de Vaga</label>
          <Select
            v-model="form.tipoVaga"
            :options="tipoOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>

        <div class="valores-grid">
          <div class="field">
            <label>Valor por Hora</label>
            <InputNumber
              v-model="form.valorHora"
              mode="currency"
              currency="BRL"
              locale="pt-BR"
              class="w-full"
              :min="0"
              :max-fraction-digits="2"
            />
          </div>
          <div class="field">
            <label>Valor Diária</label>
            <InputNumber
              v-model="form.valorDiaria"
              mode="currency"
              currency="BRL"
              locale="pt-BR"
              class="w-full"
              :min="0"
              :max-fraction-digits="2"
            />
          </div>
          <div class="field">
            <label>Valor Mensal</label>
            <InputNumber
              v-model="form.valorMensal"
              mode="currency"
              currency="BRL"
              locale="pt-BR"
              class="w-full"
              :min="0"
              :max-fraction-digits="2"
            />
          </div>
        </div>

        <div class="field">
          <label>Válida até <span class="optional">(opcional)</span></label>
          <DatePicker
            v-model="form.vigenteAte"
            date-format="dd/mm/yy"
            placeholder="Sem data de vencimento"
            class="w-full"
            show-clear
            :min-date="amanha"
          />
        </div>

        <Message v-if="erroForm" severity="error" :closable="false">{{ erroForm }}</Message>
      </div>

      <template #footer>
        <Button label="Cancelar" severity="secondary" text @click="dialogForm = false" />
        <Button
          :label="editando ? 'Salvar Alterações' : 'Cadastrar'"
          icon="pi pi-check"
          :loading="salvando"
          @click="salvar"
        />
      </template>
    </Dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { TipoVaga, TIPO_VAGA_LABEL } from '@/types'
import type { TarifaDto } from '@/types'
import { useAuthStore } from '@/stores/authStore'
import { tarifaService } from '@/services/tarifaService'

const authStore = useAuthStore()
const toast     = useToast()

// ── Estado ────────────────────────────────────────────────────────────────
const tarifas = ref<TarifaDto[]>([])
const loading = ref(false)

async function carregar() {
  loading.value = true
  try {
    tarifas.value = await tarifaService.listar()
  } catch {
    tarifas.value = []
  } finally {
    loading.value = false
  }
}

// ── Tipos sem tarifa ──────────────────────────────────────────────────────
const tiposTodos = [TipoVaga.Carro, TipoVaga.Moto, TipoVaga.Caminhonete, TipoVaga.DeficienteOuIdoso]

const tiposSemTarifa = computed(() => {
  const tiposComTarifa = new Set(tarifas.value.map(t => t.tipo_vaga))
  return tiposTodos.filter(t => !tiposComTarifa.has(t))
})

const tipoOptions = computed(() =>
  tiposSemTarifa.value.map(t => ({ value: t, label: TIPO_VAGA_LABEL[t] }))
)

// ── Dialog ────────────────────────────────────────────────────────────────
const dialogForm = ref(false)
const editando   = ref(false)
const salvando   = ref(false)
const erroForm   = ref<string | null>(null)
const tarifaId   = ref<number | null>(null)
const amanha     = new Date(Date.now() + 86400000)

const form = ref({
  tipoVaga:    TipoVaga.Carro,
  valorHora:   0,
  valorDiaria: 0,
  valorMensal: 0,
  vigenteAte:  null as Date | null,
})

function abrirDialogNova() {
  editando.value  = false
  tarifaId.value  = null
  form.value = {
    tipoVaga:    tiposSemTarifa.value[0] ?? TipoVaga.Carro,
    valorHora:   0,
    valorDiaria: 0,
    valorMensal: 0,
    vigenteAte:  null,
  }
  dialogForm.value = true
}

function abrirDialogNovaPara(tipo: TipoVaga) {
  editando.value  = false
  tarifaId.value  = null
  form.value = { tipoVaga: tipo, valorHora: 0, valorDiaria: 0, valorMensal: 0, vigenteAte: null }
  dialogForm.value = true
}

function abrirDialogEditar(tarifa: TarifaDto) {
  editando.value  = true
  tarifaId.value  = tarifa.id
  form.value = {
    tipoVaga:    tarifa.tipo_vaga,
    valorHora:   tarifa.valor_hora,
    valorDiaria: tarifa.valor_diaria,
    valorMensal: tarifa.valor_mensal,
    vigenteAte:  tarifa.vigente_ate ? new Date(tarifa.vigente_ate) : null,
  }
  dialogForm.value = true
}

function resetForm() {
  erroForm.value = null
}

async function salvar() {
  if (form.value.valorHora <= 0)   { erroForm.value = 'Valor por hora deve ser maior que zero.'; return }
  if (form.value.valorDiaria <= 0) { erroForm.value = 'Valor diária deve ser maior que zero.'; return }
  if (form.value.valorMensal <= 0) { erroForm.value = 'Valor mensal deve ser maior que zero.'; return }

  salvando.value = true
  erroForm.value = null

  const vigenteAte = form.value.vigenteAte?.toISOString() ?? null

  try {
    if (editando.value && tarifaId.value !== null) {
      await tarifaService.atualizar(tarifaId.value, {
        valorHora:   form.value.valorHora,
        valorDiaria: form.value.valorDiaria,
        valorMensal: form.value.valorMensal,
        vigenteAte,
      })
      toast.add({ severity: 'success', summary: 'Tarifa atualizada', life: 3000 })
    } else {
      await tarifaService.cadastrar({
        tipoVaga:    form.value.tipoVaga,
        valorHora:   form.value.valorHora,
        valorDiaria: form.value.valorDiaria,
        valorMensal: form.value.valorMensal,
        vigenteAte,
      })
      toast.add({ severity: 'success', summary: 'Tarifa cadastrada', life: 3000 })
    }
    dialogForm.value = false
    carregar()
  } catch (e: any) {
    erroForm.value = e?.response?.data?.errors?.[0] ?? 'Erro ao salvar tarifa.'
  } finally {
    salvando.value = false
  }
}

// ── Helpers visuais ───────────────────────────────────────────────────────
const TIPO_ICONS: Record<TipoVaga, string> = {
  [TipoVaga.Carro]:             'pi-car',
  [TipoVaga.Moto]:              'pi-bolt',
  [TipoVaga.Caminhonete]:       'pi-truck',
  [TipoVaga.DeficienteOuIdoso]: 'pi-heart',
}

const TIPO_COLORS: Record<TipoVaga, string> = {
  [TipoVaga.Carro]:             '#003c90',
  [TipoVaga.Moto]:              '#0f52ba',
  [TipoVaga.Caminhonete]:       '#5b8af0',
  [TipoVaga.DeficienteOuIdoso]: '#10B981',
}

const tipoIcon  = (t: TipoVaga) => TIPO_ICONS[t]  ?? 'pi-car'
const tipoColor = (t: TipoVaga) => TIPO_COLORS[t] ?? '#003c90'

function isAtiva(t: TarifaDto): boolean {
  if (!t.vigente_ate) return true
  return new Date(t.vigente_ate) > new Date()
}

function moeda(v: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
}

function formatarData(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(iso))
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(carregar)
</script>

<style scoped>
.tarifas-view {
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

.loading-state {
  display: flex;
  justify-content: center;
  padding: var(--space-2xl) 0;
}

/* Grid */
.tarifas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-md);
}

/* Card */
.tarifa-card {
  background: var(--color-surface-card);
  border-radius: var(--radius-lg);
  border: 1px solid var(--p-surface-200);
  border-top: 4px solid var(--tipo-color);
  box-shadow: var(--shadow-card);
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow: hidden;
  transition: box-shadow 0.2s;
}

.tarifa-card:hover {
  box-shadow: 0 4px 16px rgba(0,0,0,0.1);
}

.tarifa-card-vazia {
  opacity: 0.7;
  border-top-style: dashed;
}

/* Cabeçalho do card */
.card-header {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-lg);
  padding-bottom: var(--space-md);
}

.card-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--tipo-color) 12%, white);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 1.25rem;
  color: var(--tipo-color);
}

.card-icon-wrap-vazio {
  background: var(--p-surface-100);
  color: var(--p-surface-400);
}

.card-titulo-wrap {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.card-tipo {
  font-size: 1rem;
  font-weight: 700;
  color: var(--p-surface-800);
}

.card-validade {
  font-size: var(--font-xs);
  color: var(--p-surface-400);
}

.card-sem-tarifa {
  color: var(--color-occupied);
  font-weight: 600;
}

/* Valores */
.card-valores {
  padding: 0 var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: 0;
}

.valor-sep {
  height: 1px;
  background: var(--p-surface-100);
}

.valor-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 0;
}

.valor-row-destaque .valor-num {
  font-size: 1.1rem;
  color: var(--tipo-color);
}

.valor-label {
  font-size: var(--font-sm);
  color: var(--p-surface-500);
  font-weight: 500;
}

.valor-num {
  font-size: var(--font-sm);
  font-weight: 700;
  color: var(--p-surface-800);
}

/* Rodapé */
.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md) var(--space-lg);
  border-top: 1px solid var(--p-surface-100);
  margin-top: auto;
}

.card-data {
  font-size: var(--font-xs);
  color: var(--p-surface-400);
}

/* Card vazio */
.card-vazio-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  padding: var(--space-xl);
  text-align: center;
}

.card-vazio-body p {
  margin: 0;
  font-size: var(--font-sm);
  color: var(--p-surface-400);
}

/* Dialog */
.dialog-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding-top: var(--space-sm);
}

.valores-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-sm);
}

.valores-grid .field:last-child {
  grid-column: 1 / -1;
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

.optional {
  font-weight: 400;
  color: var(--p-surface-400);
  font-size: var(--font-xs);
}

.historico-card {
  background: var(--color-surface-card);
  border-radius: var(--radius-md);
  border: 1px solid var(--p-surface-200);
  box-shadow: var(--shadow-card);
  overflow: hidden;
  padding: var(--space-lg);
}

.historico-title {
  margin: 0 0 var(--space-md);
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.tipo-cell {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--font-sm);
  font-weight: 500;
}

.tipo-icon { font-size: 1rem; }

.vigencia-data {
  font-size: var(--font-sm);
  color: var(--color-reserved);
  font-weight: 600;
}

.vigencia-sem {
  font-size: var(--font-sm);
  color: var(--p-surface-400);
  font-style: italic;
}

.table-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-xl) 0;
  color: var(--p-surface-400);
  font-size: var(--font-sm);
}

.w-full { width: 100%; }
</style>
