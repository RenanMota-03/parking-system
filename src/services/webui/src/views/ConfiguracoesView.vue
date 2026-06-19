<template>
  <div class="config-view">

    <div class="settings-shell">

      <!-- ── Sidebar de categorias ───────────────────────────────────────────── -->
      <aside class="settings-nav">
        <h3 class="nav-titulo">Configurações</h3>
        <nav class="nav-lista">
          <button
            class="nav-item"
            :class="{ active: abaAtiva === 'perfil' }"
            @click="abaAtiva = 'perfil'"
          >
            <i class="pi pi-user nav-icon" />
            <span>Perfil</span>
          </button>
          <button
            class="nav-item"
            :class="{ active: abaAtiva === 'usuarios' }"
            @click="abaAtiva = 'usuarios'"
          >
            <i class="pi pi-users nav-icon" />
            <span>Usuários</span>
          </button>
          <button
            class="nav-item"
            :class="{ active: abaAtiva === 'notificacoes' }"
            @click="abaAtiva = 'notificacoes'"
          >
            <i class="pi pi-bell nav-icon" />
            <span>Notificações</span>
          </button>
          <button
            class="nav-item"
            :class="{ active: abaAtiva === 'backup' }"
            @click="abaAtiva = 'backup'"
          >
            <i class="pi pi-cloud-download nav-icon" />
            <span>Backup</span>
          </button>
        </nav>
      </aside>

      <!-- ── Workspace principal ────────────────────────────────────────────── -->
      <section class="settings-workspace">

        <!-- ──────────────── ABA: USUÁRIOS ──────────────────────────────────── -->
        <template v-if="abaAtiva === 'usuarios'">

          <div class="page-header">
            <div>
              <h1 class="page-title">Gerenciamento de Sistema</h1>
              <p class="page-subtitle">Configure parâmetros do estacionamento e administre acessos.</p>
            </div>
            <button class="btn-primary" @click="abrirDialogNovo">
              <i class="pi pi-plus" /> Novo Usuário
            </button>
          </div>

          <div class="bento-grid">

            <!-- Tabela de usuários -->
            <div class="tabela-card">
              <div class="card-header">
                <span class="card-titulo">Gerenciar Usuários</span>
                <button class="btn-icon" title="Filtrar">
                  <i class="pi pi-filter" />
                </button>
              </div>

              <div class="table-scroll">
                <table class="usuarios-table">
                  <thead>
                    <tr>
                      <th>Nome</th>
                      <th>Papel</th>
                      <th>Status</th>
                      <th class="th-acoes">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="loading" class="tr-empty">
                      <td colspan="4">
                        <ProgressSpinner style="width:28px;height:28px" />
                      </td>
                    </tr>
                    <tr v-else-if="usuariosPaginados.length === 0" class="tr-empty">
                      <td colspan="4">
                        <i class="pi pi-users" /> Nenhum usuário encontrado.
                      </td>
                    </tr>
                    <tr
                      v-for="u in usuariosPaginados"
                      :key="u.id"
                      class="usuario-row"
                    >
                      <td>
                        <div class="user-cell">
                          <div class="user-avatar" :class="avatarClass(u.role)">
                            {{ initials(u.nome) }}
                          </div>
                          <div class="user-info">
                            <span class="user-name">{{ u.nome }}</span>
                            <span class="user-email">{{ u.email }}</span>
                          </div>
                        </div>
                      </td>
                      <td class="td-papel">{{ ROLE_LABEL[u.role] }}</td>
                      <td>
                        <span class="status-pill status-ativo">Ativo</span>
                      </td>
                      <td class="td-acoes">
                        <button class="btn-row-action" title="Editar" @click="onEditarUsuario(u)">
                          <i class="pi pi-pencil" />
                        </button>
                        <button class="btn-row-action danger" title="Excluir" @click="onExcluirUsuario(u)">
                          <i class="pi pi-trash" />
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div class="card-footer">
                <span class="footer-count">
                  Mostrando {{ inicioFaixa }}–{{ fimFaixa }} de {{ usuarios.length }} usuários
                </span>
                <div class="footer-pager">
                  <button
                    class="pager-btn"
                    :disabled="paginaAtual === 1"
                    @click="paginaAtual--"
                  >Anterior</button>
                  <button
                    class="pager-btn"
                    :disabled="paginaAtual === totalPaginas"
                    @click="paginaAtual++"
                  >Próximo</button>
                </div>
              </div>
            </div>

            <!-- Card de configurações do sistema -->
            <div class="system-card">
              <div class="card-header">
                <i class="pi pi-sliders-h card-header-icon" />
                <span class="card-titulo">Configurações do Sistema</span>
              </div>

              <div class="card-body">
                <div class="field">
                  <label>Nome do Estacionamento</label>
                  <input v-model="settings.nome" type="text" class="field-input" />
                </div>

                <div class="grid-2">
                  <div class="field">
                    <label>Total de Vagas</label>
                    <input
                      :value="totalVagas"
                      type="number"
                      class="field-input"
                      disabled
                    />
                  </div>
                  <div class="field">
                    <label>Vagas VIP/PCD</label>
                    <input v-model.number="settings.vagasVip" type="number" class="field-input" />
                  </div>
                </div>

                <div class="field timeout-field">
                  <label>Timeout Automático (Reservas)</label>
                  <div class="timeout-row">
                    <input
                      v-model.number="settings.timeoutReservas"
                      type="number"
                      :min="5"
                      :max="120"
                      class="field-input timeout-input"
                    />
                    <span class="timeout-unit">minutos</span>
                  </div>
                  <small class="field-hint">
                    Reservas não confirmadas serão canceladas após este período.
                  </small>
                </div>
              </div>

              <div class="card-footer card-footer--buttons">
                <button class="btn-cancel" @click="cancelarConfiguracoes">Cancelar</button>
                <button class="btn-primary btn-save" @click="salvarConfiguracoes">
                  Salvar Alterações
                </button>
              </div>
            </div>
          </div>
        </template>

        <!-- ──────────────── ABA: PERFIL ────────────────────────────────────── -->
        <template v-else-if="abaAtiva === 'perfil'">
          <div class="page-header">
            <div>
              <h1 class="page-title">Perfil</h1>
              <p class="page-subtitle">Informações da sua conta.</p>
            </div>
          </div>

          <div class="perfil-card">
            <div class="perfil-avatar">{{ iniciais }}</div>
            <div class="perfil-info">
              <span class="perfil-name">{{ authStore.user?.name }}</span>
              <span class="perfil-email">{{ authStore.user?.email }}</span>
              <Tag
                :value="ROLE_LABEL[authStore.user?.role ?? Role.Cliente]"
                :severity="roleSeverity(authStore.user?.role ?? Role.Cliente)"
              />
            </div>
          </div>
        </template>

        <!-- ──────────────── ABA: NOTIFICAÇÕES ──────────────────────────────── -->
        <template v-else-if="abaAtiva === 'notificacoes'">
          <div class="page-header">
            <div>
              <h1 class="page-title">Notificações</h1>
              <p class="page-subtitle">Preferências de alertas e avisos.</p>
            </div>
          </div>
          <div class="placeholder-card">
            <i class="pi pi-bell placeholder-icon" />
            <span>Configurações de notificações em breve.</span>
          </div>
        </template>

        <!-- ──────────────── ABA: BACKUP ─────────────────────────────────────── -->
        <template v-else-if="abaAtiva === 'backup'">
          <div class="page-header">
            <div>
              <h1 class="page-title">Backup</h1>
              <p class="page-subtitle">Exportação e restauração de dados.</p>
            </div>
          </div>
          <div class="placeholder-card">
            <i class="pi pi-cloud-download placeholder-icon" />
            <span>Funcionalidade de backup em breve.</span>
          </div>
        </template>

      </section>
    </div>

    <!-- ── Dialog: Novo Usuário ──────────────────────────────────────────────── -->
    <Dialog
      v-model:visible="dialogNovo"
      header="Cadastrar Novo Usuário"
      :modal="true"
      :style="{ width: '420px' }"
      @hide="resetNovoForm"
    >
      <div class="dialog-form">
        <div class="dialog-field">
          <label>Nome Completo</label>
          <InputText v-model="novoUsuario.nome" placeholder="João da Silva" class="w-full" />
        </div>
        <div class="dialog-field">
          <label>E-mail</label>
          <InputText v-model="novoUsuario.email" type="email" placeholder="joao@exemplo.com" class="w-full" />
        </div>
        <div class="dialog-field">
          <label>Senha</label>
          <Password
            v-model="novoUsuario.senha"
            placeholder="Mínimo 6 caracteres"
            class="w-full"
            :feedback="false"
            toggle-mask
          />
        </div>
        <div class="dialog-field">
          <label>Papel</label>
          <Select
            v-model="novoUsuario.role"
            :options="rolesDisponiveis"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <Message v-if="erroUsuario" severity="error" :closable="false">{{ erroUsuario }}</Message>
      </div>

      <template #footer>
        <Button label="Cancelar" severity="secondary" text @click="dialogNovo = false" />
        <Button
          label="Cadastrar"
          icon="pi pi-check"
          :loading="salvandoUsuario"
          :disabled="!novoUsuario.nome || !novoUsuario.email || !novoUsuario.senha"
          @click="cadastrarUsuario"
        />
      </template>
    </Dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { Role, ROLE_LABEL } from '@/types'
import type { UsuarioListDto } from '@/types'
import { useAuthStore } from '@/stores/authStore'
import { useVagaStore } from '@/stores/vagaStore'
import { usuarioService } from '@/services/usuarioService'
import api from '@/services/api'

const authStore = useAuthStore()
const vagaStore = useVagaStore()
const toast     = useToast()

// ── Aba ativa ────────────────────────────────────────────────────────────────
const abaAtiva = ref<'perfil' | 'usuarios' | 'notificacoes' | 'backup'>('usuarios')

// ── Usuários ──────────────────────────────────────────────────────────────────
const usuarios = ref<UsuarioListDto[]>([])
const loading  = ref(false)

async function carregarUsuarios() {
  loading.value = true
  try {
    const result = await usuarioService.listar(1, 200)
    usuarios.value = result.items
  } catch {
    usuarios.value = []
  } finally {
    loading.value = false
  }
}

// ── Paginação ─────────────────────────────────────────────────────────────────
const paginaAtual    = ref(1)
const itensPorPagina = 8

const totalPaginas = computed(() =>
  Math.max(1, Math.ceil(usuarios.value.length / itensPorPagina))
)

const inicioFaixa = computed(() => Math.min((paginaAtual.value - 1) * itensPorPagina + 1, usuarios.value.length))
const fimFaixa    = computed(() => Math.min(paginaAtual.value * itensPorPagina, usuarios.value.length))

const usuariosPaginados = computed(() => {
  const start = (paginaAtual.value - 1) * itensPorPagina
  return usuarios.value.slice(start, start + itensPorPagina)
})

// ── Perfil ────────────────────────────────────────────────────────────────────
const totalVagas = computed(() => vagaStore.vagas.length)

const iniciais = computed(() => {
  const nome = authStore.user?.name ?? ''
  return nome.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase()
})

function initials(nome: string): string {
  return nome.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase()
}

function avatarClass(role: Role): string {
  return role === Role.Admin ? 'avatar-admin' : role === Role.Operador ? 'avatar-operador' : 'avatar-cliente'
}

function roleSeverity(role: Role): string {
  return role === Role.Admin ? 'danger' : role === Role.Operador ? 'warn' : 'info'
}

// ── Configurações locais ──────────────────────────────────────────────────────
const STORAGE_KEY = 'parksystem_settings'

function carregarDoStorage() {
  return {
    nome:             localStorage.getItem(`${STORAGE_KEY}_nome`)    ?? 'ParkSystem',
    timeoutReservas:  Number(localStorage.getItem(`${STORAGE_KEY}_timeout`) ?? 15),
    vagasVip:         Number(localStorage.getItem(`${STORAGE_KEY}_vagasVip`) ?? 0),
  }
}

const settings = ref(carregarDoStorage())

function salvarConfiguracoes() {
  localStorage.setItem(`${STORAGE_KEY}_nome`,     settings.value.nome)
  localStorage.setItem(`${STORAGE_KEY}_timeout`,  String(settings.value.timeoutReservas))
  localStorage.setItem(`${STORAGE_KEY}_vagasVip`, String(settings.value.vagasVip))
  toast.add({ severity: 'success', summary: 'Configurações salvas', life: 3000 })
}

function cancelarConfiguracoes() {
  settings.value = carregarDoStorage()
}

// ── Cadastro de usuário ────────────────────────────────────────────────────────
const dialogNovo      = ref(false)
const novoUsuario     = ref({ nome: '', email: '', senha: '', role: Role.Operador })
const salvandoUsuario = ref(false)
const erroUsuario     = ref<string | null>(null)

const rolesDisponiveis = [
  { label: 'Operador', value: Role.Operador },
  { label: 'Cliente',  value: Role.Cliente  },
]

function abrirDialogNovo() {
  resetNovoForm()
  dialogNovo.value = true
}

function resetNovoForm() {
  novoUsuario.value = { nome: '', email: '', senha: '', role: Role.Operador }
  erroUsuario.value = null
}

async function cadastrarUsuario() {
  erroUsuario.value = null
  if (novoUsuario.value.senha.length < 6) {
    erroUsuario.value = 'A senha deve ter no mínimo 6 caracteres.'
    return
  }
  salvandoUsuario.value = true
  try {
    await api.post('/api/usuarios', {
      nome:  novoUsuario.value.nome.trim(),
      email: novoUsuario.value.email.trim().toLowerCase(),
      senha: novoUsuario.value.senha,
      role:  novoUsuario.value.role,
    })
    dialogNovo.value = false
    toast.add({ severity: 'success', summary: 'Usuário cadastrado', life: 3000 })
    carregarUsuarios()
  } catch (e: any) {
    erroUsuario.value = e?.response?.data?.errors?.[0] ?? 'Erro ao cadastrar usuário.'
  } finally {
    salvandoUsuario.value = false
  }
}

// ── Ações de linha ────────────────────────────────────────────────────────────
function onEditarUsuario(_u: UsuarioListDto) {
  toast.add({ severity: 'info', summary: 'Edição em breve', detail: 'Funcionalidade será adicionada em breve.', life: 3000 })
}

function onExcluirUsuario(_u: UsuarioListDto) {
  toast.add({ severity: 'warn', summary: 'Exclusão em breve', detail: 'Funcionalidade será adicionada em breve.', life: 3000 })
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(() => {
  carregarUsuarios()
  if (vagaStore.vagas.length === 0) vagaStore.fetchAll()
})
</script>

<style scoped>
.config-view {
  /* remove padding from parent — the shell fills the full content area */
  margin: calc(-1 * var(--space-xl));
  height: calc(100vh - var(--topnav-height));
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* ── Shell ─────────────────────────────────────────────────────────────────── */
.settings-shell {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* ── Sidebar de categorias ──────────────────────────────────────────────────── */
.settings-nav {
  width: 220px;
  flex-shrink: 0;
  background: #fff;
  border-right: 1px solid var(--p-surface-200);
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  overflow-y: auto;
}

.nav-titulo {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 700;
  color: var(--p-surface-800);
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--p-surface-100);
}

.nav-lista {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px var(--space-md);
  border: none;
  background: transparent;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  color: var(--p-surface-600);
  cursor: pointer;
  text-align: left;
  transition: background 0.14s, color 0.14s;
}

.nav-item:hover { background: var(--p-surface-50); color: var(--p-surface-800); }

.nav-item.active {
  background: rgba(0,60,144,0.07);
  color: var(--color-primary);
  font-weight: 700;
  border-left: 3px solid var(--color-primary);
  padding-left: calc(var(--space-md) - 3px);
}

.nav-icon { font-size: 1rem; flex-shrink: 0; }

/* ── Workspace ───────────────────────────────────────────────────────────────── */
.settings-workspace {
  flex: 1;
  overflow-y: auto;
  background: var(--color-surface);
  padding: var(--space-xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Page header ─────────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
}

.page-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--p-surface-900);
  letter-spacing: -0.02em;
}

.page-subtitle {
  margin: 3px 0 0;
  font-size: var(--font-xs);
  color: var(--p-surface-500);
}

/* ── Botões ───────────────────────────────────────────────────────────────────── */
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 9px 18px;
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: var(--radius-md);
  font-size: 0.8125rem;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
  transition: background 0.15s;
}
.btn-primary:hover { background: var(--color-primary-light); }

.btn-cancel {
  padding: 9px 16px;
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-surface-700);
  cursor: pointer;
  transition: background 0.15s;
}
.btn-cancel:hover { background: var(--p-surface-50); }

.btn-icon {
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  cursor: pointer;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--p-surface-500);
  font-size: 0.9rem;
  transition: background 0.14s, color 0.14s;
}
.btn-icon:hover { background: var(--p-surface-100); color: var(--p-surface-800); }

/* ── Bento grid ──────────────────────────────────────────────────────────────── */
.bento-grid {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: var(--space-lg);
  align-items: start;
}

@media (max-width: 1100px) {
  .bento-grid { grid-template-columns: 1fr; }
}

/* ── Card base (shared) ──────────────────────────────────────────────────────── */
.tabela-card,
.system-card {
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px var(--space-md);
  border-bottom: 1px solid var(--p-surface-100);
  background: #fff;
}

.card-titulo {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--p-surface-800);
}

.card-header-icon {
  color: var(--color-primary);
  font-size: 1rem;
  margin-right: 6px;
}

/* ── Tabela de usuários ──────────────────────────────────────────────────────── */
.table-scroll { overflow-x: auto; flex: 1; }

.usuarios-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.usuarios-table th {
  padding: 8px var(--space-md);
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--p-surface-500);
  background: var(--p-surface-50);
  border-bottom: 2px solid var(--p-surface-200);
  text-align: left;
  white-space: nowrap;
}

.th-acoes { text-align: right; width: 80px; }

.usuarios-table td {
  padding: 10px var(--space-md);
  border-bottom: 1px solid var(--p-surface-100);
  color: var(--p-surface-800);
  vertical-align: middle;
}

.usuario-row:hover td { background: #f8fafc; }
.usuario-row:last-child td { border-bottom: none; }

.tr-empty td {
  text-align: center;
  padding: 32px;
  color: var(--p-surface-400);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

/* User cell */
.user-cell { display: flex; align-items: center; gap: 10px; }

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
}

.avatar-admin    { background: #dbeafe; color: #1d4ed8; }
.avatar-operador { background: #d1fae5; color: #065f46; }
.avatar-cliente  { background: #ede9fe; color: #5b21b6; }

.user-info { display: flex; flex-direction: column; gap: 1px; }
.user-name  { font-size: 13px; font-weight: 600; color: var(--p-surface-800); }
.user-email { font-size: 11px; color: var(--p-surface-400); }

.td-papel { font-size: 13px; color: var(--p-surface-700); }

/* Status pill */
.status-pill {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: var(--radius-full);
  font-size: 11px;
  font-weight: 600;
  border: 1px solid;
}
.status-ativo { background: #e9edff; color: var(--color-primary); border-color: rgba(0,60,144,0.2); }

/* Ações de linha */
.td-acoes { text-align: right; }

.btn-row-action {
  width: 28px;
  height: 28px;
  border: none;
  background: transparent;
  cursor: pointer;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
  color: var(--p-surface-400);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-left: 2px;
  transition: background 0.13s, color 0.13s;
}
.btn-row-action:hover { background: var(--p-surface-100); color: var(--p-surface-700); }
.btn-row-action.danger:hover { background: #fee2e2; color: #dc2626; }

/* Footer da tabela */
.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px var(--space-md);
  border-top: 1px solid var(--p-surface-100);
  background: #fff;
  font-size: 12px;
  color: var(--p-surface-500);
}

.footer-pager { display: flex; gap: 6px; }

.pager-btn {
  padding: 3px 10px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-sm);
  background: #fff;
  font-size: 12px;
  cursor: pointer;
  color: var(--p-surface-700);
  transition: background 0.14s;
}
.pager-btn:hover:not(:disabled) { background: var(--p-surface-50); }
.pager-btn:disabled { opacity: 0.45; cursor: not-allowed; }

/* ── System settings card ────────────────────────────────────────────────────── */
.card-body {
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  flex: 1;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.field label {
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: var(--p-surface-700);
}

.field-input {
  height: 36px;
  padding: 0 10px;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-md);
  background: var(--p-surface-50);
  font-size: 13px;
  color: var(--p-surface-800);
  width: 100%;
  box-sizing: border-box;
}
.field-input:focus { outline: 2px solid var(--color-primary); outline-offset: -1px; }
.field-input:disabled { background: var(--p-surface-100); color: var(--p-surface-400); cursor: not-allowed; }

.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-sm); }

.timeout-field { border-top: 1px solid var(--p-surface-100); padding-top: var(--space-md); }

.timeout-row { display: flex; align-items: center; gap: 8px; }
.timeout-input { width: 80px; flex-shrink: 0; }
.timeout-unit { font-size: 12px; color: var(--p-surface-500); }

.field-hint { font-size: 11px; color: var(--p-surface-400); line-height: 1.4; }

.card-footer--buttons {
  justify-content: flex-end;
  gap: var(--space-sm);
  background: var(--p-surface-50);
}

.btn-save { padding: 9px 20px; }

/* ── Perfil ──────────────────────────────────────────────────────────────────── */
.perfil-card {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  padding: var(--space-xl);
  background: #fff;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  max-width: 480px;
}

.perfil-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: var(--color-primary);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  font-weight: 700;
  flex-shrink: 0;
}

.perfil-info { display: flex; flex-direction: column; gap: 4px; }
.perfil-name  { font-size: 1rem; font-weight: 700; color: var(--p-surface-800); }
.perfil-email { font-size: var(--font-xs); color: var(--p-surface-500); }

/* ── Placeholder ─────────────────────────────────────────────────────────────── */
.placeholder-card {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  padding: 60px;
  background: #fff;
  border: 1px dashed var(--p-surface-200);
  border-radius: var(--radius-lg);
  color: var(--p-surface-400);
  font-size: var(--font-sm);
}

.placeholder-icon { font-size: 1.5rem; }

/* ── Dialog ──────────────────────────────────────────────────────────────────── */
.dialog-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding-top: var(--space-sm);
}

.dialog-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dialog-field label {
  font-size: var(--font-sm);
  font-weight: 600;
  color: var(--p-surface-700);
}

.w-full { width: 100%; }
</style>
