<template>
  <div class="fluxo-view">
    <div class="paineis">

      <!-- ══════════════════════ ENTRADA ══════════════════════ -->
      <div class="painel">
        <div class="painel-header">
          <div>
            <h2 class="painel-titulo">
              <i class="pi pi-sign-in icon-entrada" />
              Entrada Rápida
            </h2>
            <p class="painel-sub">Registrar entrada de veículo</p>
          </div>
        </div>

        <Transition name="fade">
          <div v-if="entradaSucesso" class="sucesso-box">
            <i class="pi pi-check-circle sucesso-icon" />
            <p class="sucesso-titulo">Entrada registrada!</p>
            <div class="sucesso-info">
              <span class="placa-chip">{{ entradaConfirmada.placa }}</span>
              <span class="sep">·</span>
              <span>Vaga {{ entradaConfirmada.vaga }}</span>
            </div>
            <Button label="Nova Entrada" icon="pi pi-plus" severity="secondary" outlined @click="resetEntrada" />
          </div>
        </Transition>

        <Transition name="fade">
          <div v-if="!entradaSucesso" class="painel-body">
            <div class="painel-fields">
              <div class="field">
                <label>Placa</label>
                <InputText
                  v-model="entradaPlaca"
                  placeholder="ABC-1234"
                  class="input-placa w-full"
                  :class="{ 'p-invalid': erroEntrada }"
                  maxlength="8"
                  @input="entradaPlaca = entradaPlaca.toUpperCase().replace(/[^A-Z0-9]/g, '')"
                  @keyup.enter="!entradaLoading && registrarEntrada()"
                />
              </div>

              <div class="dois-campos">
                <div class="field">
                  <label>Tipo de Veículo</label>
                  <Select
                    v-model="tipoVeiculo"
                    :options="tipoOptions"
                    option-label="label"
                    option-value="value"
                    class="w-full"
                  />
                </div>
                <div class="field">
                  <label>Atribuir Vaga</label>
                  <Select
                    v-model="entradaVagaId"
                    :options="vagasDisponiveis"
                    option-label="label"
                    option-value="value"
                    placeholder="Automático"
                    class="w-full"
                    filter
                    :loading="vagaStore.loading"
                    empty-message="Nenhuma vaga disponível"
                  />
                </div>
              </div>

              <Message v-if="erroEntrada" severity="error" :closable="false">{{ erroEntrada }}</Message>
            </div>

            <div class="painel-footer">
              <Button
                label="Registrar Entrada"
                icon="pi pi-plus-circle"
                class="w-full btn-lg"
                :loading="entradaLoading"
                :disabled="!entradaPlaca"
                @click="registrarEntrada"
              />
            </div>
          </div>
        </Transition>
      </div>

      <!-- ══════════════════════ SAÍDA ══════════════════════ -->
      <div class="painel painel-saida">
        <div class="painel-header">
          <div>
            <h2 class="painel-titulo">
              <i class="pi pi-sign-out icon-saida" />
              Fechamento de Ticket
            </h2>
            <p class="painel-sub">Processar saída e pagamento</p>
          </div>

          <div v-if="saidaStep === 'busca'" class="search-box">
            <i class="pi pi-search search-icon" />
            <InputText
              v-model="saidaPlaca"
              placeholder="Buscar placa..."
              class="search-input"
              :class="{ 'p-invalid': erroSaida }"
              maxlength="8"
              @input="saidaPlaca = saidaPlaca.toUpperCase().replace(/[^A-Z0-9]/g, '')"
              @keyup.enter="!saidaLoading && registrarSaida()"
            />
          </div>
          <div v-else class="placa-chip-header">
            {{ movSaida?.placa_veiculo ?? saidaConfirmada.placa }}
          </div>
        </div>

        <!-- Step 1: busca -->
        <Transition name="fade">
          <div v-if="saidaStep === 'busca'" class="painel-body">
            <div class="painel-fields busca-vazia">
              <i class="pi pi-car busca-icon" />
              <p>Digite a placa acima e pressione <strong>Enter</strong></p>
              <Message v-if="erroSaida" severity="error" :closable="false" class="w-full">{{ erroSaida }}</Message>
            </div>
            <div class="painel-footer">
              <Button
                label="Registrar Saída"
                icon="pi pi-sign-out"
                severity="danger"
                class="w-full btn-lg"
                :loading="saidaLoading"
                :disabled="!saidaPlaca"
                @click="registrarSaida"
              />
            </div>
          </div>
        </Transition>

        <!-- Step 2: pagamento -->
        <Transition name="fade">
          <div v-if="saidaStep === 'pagamento' && movSaida" class="pagamento-body">
            <div class="ticket-cols">
              <div class="sessao-card">
                <div class="sessao-row">
                  <span class="sessao-key">HORA DE ENTRADA</span>
                  <span class="sessao-val">{{ formatarHora(movSaida.data_entrada) }}</span>
                </div>
                <div class="sessao-row">
                  <span class="sessao-key">HORA ATUAL</span>
                  <span class="sessao-val">{{ horaAtual }}</span>
                </div>
                <div class="sessao-row last">
                  <span class="sessao-key">TEMPO DECORRIDO</span>
                  <span class="tempo-counter">{{ tempoDecorrido }}</span>
                </div>
                <div class="sessao-total">
                  <span class="sessao-key">TOTAL CALCULADO</span>
                  <span class="valor-total">{{ formatarMoeda(movSaida.valor_total) }}</span>
                </div>
              </div>

              <div class="pagamento-col">
                <p class="pagamento-label">MÉTODO DE PAGAMENTO</p>
                <div class="pagamento-grid">
                  <button
                    v-for="opt in formaOptions"
                    :key="opt.key"
                    class="pagamento-card"
                    :class="{ selected: formaPagamento === opt.value }"
                    @click="formaPagamento = opt.value"
                  >
                    <i :class="`pi ${opt.icon} pag-icon`" />
                    <span>{{ opt.label }}</span>
                  </button>
                </div>
              </div>
            </div>

            <Message v-if="erroPagamento" severity="error" :closable="false">{{ erroPagamento }}</Message>

            <div class="botoes-saida">
              <Button label="Imprimir Recibo" icon="pi pi-print" severity="secondary" outlined @click="() => {}" />
              <Button
                label="Concluir Saída"
                icon="pi pi-check-circle"
                severity="danger"
                :loading="pagamentoLoading"
                :disabled="formaPagamento === null"
                @click="confirmarPagamento"
              />
            </div>
          </div>
        </Transition>

        <!-- Step 3: sucesso -->
        <Transition name="fade">
          <div v-if="saidaStep === 'sucesso'" class="sucesso-box">
            <i class="pi pi-check-circle sucesso-icon" />
            <p class="sucesso-titulo">Pagamento confirmado!</p>
            <div class="sucesso-info">
              <span class="placa-chip">{{ saidaConfirmada.placa }}</span>
              <span class="sep">·</span>
              <span class="sucesso-valor">{{ formatarMoeda(saidaConfirmada.valor) }}</span>
            </div>
            <p class="sucesso-forma">{{ FORMA_PAGAMENTO_LABEL[saidaConfirmada.forma] }}</p>
            <Button label="Nova Saída" icon="pi pi-plus" severity="secondary" outlined @click="resetSaida" />
          </div>
        </Transition>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { StatusVaga, FormaPagamento, TIPO_VAGA_LABEL, FORMA_PAGAMENTO_LABEL } from '@/types'
import type { MovimentacaoDto } from '@/types'
import { useVagaStore } from '@/stores/vagaStore'
import { movimentacaoService } from '@/services/movimentacaoService'

const vagaStore = useVagaStore()
const toast     = useToast()

const vagasDisponiveis = computed(() =>
  vagaStore.vagas
    .filter(v => v.status === StatusVaga.Disponivel)
    .map(v => ({
      value: v.id,
      label: `${v.numero}  —  ${TIPO_VAGA_LABEL[v.tipo_vaga]}`,
    }))
)

// ── Tipo de Veículo (cosmético) ───────────────────────────────────────────
const tipoVeiculo = ref('passeio')
const tipoOptions = [
  { label: 'Carro de Passeio', value: 'passeio' },
  { label: 'SUV / Utilitário', value: 'suv' },
  { label: 'Moto',             value: 'moto' },
  { label: 'Van',              value: 'van' },
]

// ════════════════ ENTRADA ════════════════
const entradaPlaca      = ref('')
const entradaVagaId     = ref<number | null>(null)
const entradaLoading    = ref(false)
const erroEntrada       = ref<string | null>(null)
const entradaSucesso    = ref(false)
const entradaConfirmada = ref({ placa: '', vaga: '' })

async function registrarEntrada() {
  if (!entradaPlaca.value) return
  const vagaId = entradaVagaId.value ?? vagasDisponiveis.value[0]?.value
  if (!vagaId) {
    erroEntrada.value = 'Nenhuma vaga disponível no momento.'
    return
  }
  entradaLoading.value = true
  erroEntrada.value    = null
  try {
    await movimentacaoService.registrarEntrada({
      vagaId,
      placaVeiculo: entradaPlaca.value,
    })
    const vagaLabel = vagasDisponiveis.value.find(v => v.value === vagaId)?.label ?? ''
    entradaConfirmada.value = {
      placa: entradaPlaca.value,
      vaga:  vagaLabel.split('—')[0].trim(),
    }
    entradaSucesso.value = true
    await vagaStore.fetchAll()
    toast.add({ severity: 'success', summary: 'Entrada registrada', life: 3000 })
  } catch (e: any) {
    erroEntrada.value = e?.response?.data?.errors?.[0] ?? 'Erro ao registrar entrada.'
  } finally {
    entradaLoading.value = false
  }
}

function resetEntrada() {
  entradaSucesso.value = false
  entradaPlaca.value   = ''
  entradaVagaId.value  = null
  erroEntrada.value    = null
}

// ════════════════ SAÍDA ════════════════
type SaidaStep = 'busca' | 'pagamento' | 'sucesso'

const saidaStep     = ref<SaidaStep>('busca')
const saidaPlaca    = ref('')
const saidaLoading  = ref(false)
const erroSaida     = ref<string | null>(null)
const movSaida      = ref<MovimentacaoDto | null>(null)

const formaOptions = [
  { key: 'dinheiro', value: FormaPagamento.Dinheiro, label: 'Dinheiro', icon: 'pi-money-bill' },
  { key: 'credito',  value: FormaPagamento.Cartao,   label: 'Crédito',  icon: 'pi-credit-card' },
  { key: 'debito',   value: FormaPagamento.Cartao,   label: 'Débito',   icon: 'pi-wallet' },
  { key: 'pix',      value: FormaPagamento.Pix,      label: 'PIX',      icon: 'pi-qrcode' },
]
const formaPagamento   = ref<FormaPagamento | null>(null)
const pagamentoLoading = ref(false)
const erroPagamento    = ref<string | null>(null)
const saidaConfirmada  = ref({ placa: '', valor: 0, forma: FormaPagamento.Dinheiro })

// ── Timer ─────────────────────────────────────────────────────────────────
const horaAtual      = ref('')
const tempoDecorrido = ref('00:00:00')
let timerInterval: ReturnType<typeof setInterval> | null = null

function updateTimer() {
  const now = new Date()
  horaAtual.value = now.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
  if (movSaida.value) {
    const ms        = now.getTime() - new Date(movSaida.value.data_entrada).getTime()
    const totalSecs = Math.floor(ms / 1000)
    const h = Math.floor(totalSecs / 3600)
    const m = Math.floor((totalSecs % 3600) / 60)
    const s = totalSecs % 60
    tempoDecorrido.value = [h, m, s].map(n => String(n).padStart(2, '0')).join(':')
  }
}

function startTimer() { updateTimer(); timerInterval = setInterval(updateTimer, 1000) }
function stopTimer()  { if (timerInterval) { clearInterval(timerInterval); timerInterval = null } }

async function registrarSaida() {
  if (!saidaPlaca.value) return
  saidaLoading.value = true
  erroSaida.value    = null
  try {
    const mov = await movimentacaoService.registrarSaida({ placaVeiculo: saidaPlaca.value })
    movSaida.value       = mov
    formaPagamento.value = FormaPagamento.Dinheiro
    saidaStep.value      = 'pagamento'
    startTimer()
    await vagaStore.fetchAll()
  } catch (e: any) {
    erroSaida.value = e?.response?.data?.errors?.[0] ?? 'Nenhuma entrada em aberto para esta placa.'
  } finally {
    saidaLoading.value = false
  }
}

async function confirmarPagamento() {
  if (!movSaida.value || formaPagamento.value === null) return
  pagamentoLoading.value = true
  erroPagamento.value    = null
  try {
    await movimentacaoService.pagar({
      movimentacaoId: movSaida.value.id,
      formaPagamento: formaPagamento.value,
    })
    saidaConfirmada.value = {
      placa: movSaida.value.placa_veiculo,
      valor: movSaida.value.valor_total ?? 0,
      forma: formaPagamento.value,
    }
    saidaStep.value = 'sucesso'
    stopTimer()
    toast.add({ severity: 'success', summary: 'Pagamento confirmado', life: 3000 })
  } catch (e: any) {
    erroPagamento.value = e?.response?.data?.errors?.[0] ?? 'Erro ao processar pagamento.'
  } finally {
    pagamentoLoading.value = false
  }
}

function resetSaida() {
  stopTimer()
  saidaStep.value      = 'busca'
  saidaPlaca.value     = ''
  movSaida.value       = null
  formaPagamento.value = null
  erroSaida.value      = null
  erroPagamento.value  = null
  tempoDecorrido.value = '00:00:00'
}

function formatarHora(iso: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  }).format(new Date(iso))
}

function formatarMoeda(valor: number | null): string {
  if (valor === null) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor)
}

onMounted(() => { if (vagaStore.vagas.length === 0) vagaStore.fetchAll() })
onUnmounted(() => stopTimer())
</script>

<style scoped>
.fluxo-view {
  display: flex;
  flex-direction: column;
  height: 100%;
}

/* ── Layout dos painéis ─────────────────────────────────────────────────── */
.paineis {
  display: grid;
  grid-template-columns: 1fr 1.5fr;
  gap: var(--space-lg);
  align-items: stretch;
  flex: 1;
  min-height: 0;
}

@media (max-width: 900px) {
  .paineis { grid-template-columns: 1fr; }
}

.painel {
  background: var(--color-surface-card);
  border-radius: var(--radius-xl, 12px);
  border: 1px solid var(--p-surface-200);
  box-shadow: var(--shadow-card);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 560px;
}

.painel-saida {
  border-top: 4px solid #ba1a1a;
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.10), 0 0 0 1px var(--p-surface-200);
}

/* ── Cabeçalho do painel ──────────────────────────────────────────────── */
.painel-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-lg) var(--space-md);
  border-bottom: 1px solid var(--p-surface-100);
}

.painel-titulo {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--p-surface-800);
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.painel-sub {
  margin: 4px 0 0;
  font-size: var(--font-sm);
  color: var(--p-surface-500);
}

.icon-entrada { color: #22c55e; font-size: 1.1rem; }
.icon-saida   { color: #ba1a1a; font-size: 1.1rem; }

/* ── Search box no cabeçalho da saída ───────────────────────────────── */
.search-box {
  position: relative;
  width: 220px;
  flex-shrink: 0;
}

.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--p-surface-400);
  font-size: 0.85rem;
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding-left: 30px !important;
  font-size: var(--font-sm) !important;
}

.placa-chip-header {
  font-family: 'Courier New', monospace;
  font-size: 0.9rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  background: var(--p-surface-100);
  color: var(--p-surface-700);
  padding: 4px 12px;
  border-radius: 6px;
  border: 1px solid var(--p-surface-200);
}

/* ── Body dos painéis ──────────────────────────────────────────────────── */
.painel-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: var(--space-lg);
}

.painel-fields {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  flex: 1;
}

.painel-footer {
  margin-top: auto;
  padding-top: var(--space-lg);
}

/* ── Campo de placa grande ─────────────────────────────────────────────── */
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field label {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  color: var(--p-surface-600);
  text-transform: uppercase;
}

.input-placa {
  font-family: 'Courier New', monospace !important;
  font-size: 2rem !important;
  font-weight: 700 !important;
  letter-spacing: 0.2em !important;
  text-align: center !important;
  text-transform: uppercase !important;
  padding: var(--space-md) !important;
}

/* ── Dois campos lado a lado ───────────────────────────────────────────── */
.dois-campos {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}

/* ── Botão grande ─────────────────────────────────────────────────────── */
.btn-lg {
  font-size: 1rem !important;
  padding: 0.75rem 1rem !important;
}

/* ── Estado vazio (busca) ─────────────────────────────────────────────── */
.busca-vazia {
  align-items: center;
  justify-content: center;
  text-align: center;
  color: var(--p-surface-400);
  font-size: var(--font-sm);
  gap: var(--space-sm);
}

.busca-icon {
  font-size: 2.5rem;
  color: var(--p-surface-300);
}

/* ── Sucesso ──────────────────────────────────────────────────────────── */
.sucesso-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  gap: var(--space-sm);
  text-align: center;
  padding: var(--space-xl);
}

.sucesso-icon {
  font-size: 3rem;
  color: #22c55e;
}

.sucesso-titulo {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--p-surface-800);
}

.sucesso-info {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  color: var(--p-surface-600);
  font-size: var(--font-sm);
}

.sucesso-valor {
  font-weight: 700;
  color: var(--color-primary);
}

.sucesso-forma {
  margin: 0;
  font-size: var(--font-sm);
  color: var(--p-surface-400);
}

.sep { color: var(--p-surface-300); }

.placa-chip {
  font-family: 'Courier New', monospace;
  font-weight: 700;
  letter-spacing: 0.1em;
  background: var(--p-surface-100);
  padding: 2px 8px;
  border-radius: 4px;
}

/* ── Pagamento body ────────────────────────────────────────────────────── */
.pagamento-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: var(--space-lg);
  gap: var(--space-md);
}

.ticket-cols {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-lg);
  flex: 1;
}

@media (max-width: 900px) {
  .ticket-cols { grid-template-columns: 1fr; }
}

/* ── Sessão card ──────────────────────────────────────────────────────── */
.sessao-card {
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: 0;
}

.sessao-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid var(--p-surface-100);
  font-size: var(--font-sm);
}

.sessao-row.last { border-bottom: none; }

.sessao-key {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  color: var(--p-surface-500);
  text-transform: uppercase;
}

.sessao-val {
  font-weight: 600;
  color: var(--p-surface-800);
}

.tempo-counter {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--color-primary);
  font-feature-settings: "tnum";
}

.sessao-total {
  margin-top: auto;
  padding-top: var(--space-md);
  border-top: 1px solid var(--p-surface-200);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 6px;
}

.valor-total {
  font-size: 2.5rem;
  font-weight: 700;
  color: var(--p-surface-900);
  line-height: 1;
}

/* ── Pagamento col ────────────────────────────────────────────────────── */
.pagamento-col {
  display: flex;
  flex-direction: column;
}

.pagamento-label {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  color: var(--p-surface-500);
  text-transform: uppercase;
  margin: 0 0 var(--space-sm);
}

.pagamento-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-sm);
  flex: 1;
}

.pagamento-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  background: var(--color-surface-card);
  cursor: pointer;
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--p-surface-600);
  transition: border-color 0.15s, color 0.15s, background 0.15s;
  min-height: 88px;
}

.pagamento-card:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
  background: rgba(0,60,144,0.04);
}

.pagamento-card.selected {
  border-color: var(--color-primary);
  border-width: 2px;
  color: var(--color-primary);
  background: rgba(0,60,144,0.07);
}

.pag-icon { font-size: 1.6rem; }

/* ── Botões de saída ──────────────────────────────────────────────────── */
.botoes-saida {
  display: flex;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--p-surface-100);
}

.botoes-saida > :first-child { flex: 1; }
.botoes-saida > :last-child  { flex: 2; }

/* ── Utilitários ─────────────────────────────────────────────────────── */
.w-full { width: 100%; }

/* ── Transitions ─────────────────────────────────────────────────────── */
.fade-enter-active,
.fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from,
.fade-leave-to     { opacity: 0; }
</style>
