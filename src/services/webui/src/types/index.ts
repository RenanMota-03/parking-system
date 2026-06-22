// ─── Enums ───────────────────────────────────────────────────────────────────

export enum Role {
  Admin      = 0,
  Operador   = 1,
  Cliente    = 2,
  SuperAdmin = 3,
}

export enum TipoVaga {
  Carro            = 0,
  Moto             = 1,
  Caminhonete      = 2,
  DeficienteOuIdoso = 3,
}

export enum StatusVaga {
  Disponivel  = 0,
  Ocupada     = 1,
  Reservada   = 2,
  Manutencao  = 3,
}

export enum StatusReserva {
  Pendente   = 0,
  Confirmada = 1,
  Cancelada  = 2,
  Expirada   = 3,
  Utilizada  = 4,
}

export enum FormaPagamento {
  Dinheiro = 0,
  Cartao   = 1,
  Pix      = 2,
}

// ─── Auth ────────────────────────────────────────────────────────────────────

export interface LoginRequest {
  email: string
  senha: string
}

export interface RegistroRequest {
  nome:          string
  email:         string
  senha:         string
  codigoConvite: string
}

export interface AuthResponse {
  access_token: string
  token_type:   string
  expires_at:   string
  role:         string
  name:         string
}

export interface AuthUser {
  id:    string
  name:  string
  email: string
  role:  Role
}

// ─── Paginação ───────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items:             T[]
  page:              number
  page_size:         number
  total_items:       number
  total_pages:       number
  has_next_page:     boolean
  has_previous_page: boolean
}

// ─── Vagas ───────────────────────────────────────────────────────────────────

export interface VagaDto {
  id:         number
  numero:     string
  tipo_vaga:  TipoVaga
  status:     StatusVaga
  created_at: string
  updated_at: string
}

export interface CadastrarVagaRequest {
  numero:   string
  tipoVaga: TipoVaga
}

export interface AlterarStatusVagaRequest {
  novoStatus: StatusVaga
}

// ─── Movimentações ───────────────────────────────────────────────────────────

export interface MovimentacaoDto {
  id:              number
  vaga_id:         number
  numero_vaga:     string
  placa_veiculo:   string
  data_entrada:    string
  data_saida:      string | null
  valor_total:     number | null
  pago:            boolean
  forma_pagamento: FormaPagamento | null
}

export interface RegistrarEntradaRequest {
  vagaId:       number
  placaVeiculo: string
}

export interface RegistrarSaidaRequest {
  placaVeiculo: string
}

export interface PagarRequest {
  movimentacaoId:  number
  formaPagamento:  FormaPagamento
}

// ─── Tarifas ─────────────────────────────────────────────────────────────────

export interface TarifaDto {
  id:           number
  tipo_vaga:    TipoVaga
  valor_hora:   number
  valor_diaria: number
  valor_mensal: number
  vigente_ate:  string | null
  created_at:   string
}

export interface CadastrarTarifaRequest {
  tipoVaga:    TipoVaga
  valorHora:   number
  valorDiaria: number
  valorMensal: number
  vigenteAte?: string | null
}

export interface AtualizarTarifaRequest {
  valorHora:   number
  valorDiaria: number
  valorMensal: number
  vigenteAte?: string | null
}

// ─── Reservas ────────────────────────────────────────────────────────────────

export interface ReservaDto {
  id:           number
  vaga_id:      number
  numero_vaga:  string
  usuario_id:   string
  data_agendada: string
  data_limite:  string
  status:       StatusReserva
  created_at:   string
}

export interface CriarReservaRequest {
  vagaId:       number
  usuarioId:    string
  dataAgendada: string
  dataLimite:   string
}

// ─── Helpers de label ────────────────────────────────────────────────────────

export const TIPO_VAGA_LABEL: Record<TipoVaga, string> = {
  [TipoVaga.Carro]:             'Carro',
  [TipoVaga.Moto]:              'Moto',
  [TipoVaga.Caminhonete]:       'Caminhonete',
  [TipoVaga.DeficienteOuIdoso]: 'Deficiente/Idoso',
}

export const STATUS_VAGA_LABEL: Record<StatusVaga, string> = {
  [StatusVaga.Disponivel]: 'Disponível',
  [StatusVaga.Ocupada]:    'Ocupada',
  [StatusVaga.Reservada]:  'Reservada',
  [StatusVaga.Manutencao]: 'Manutenção',
}

export const STATUS_RESERVA_LABEL: Record<StatusReserva, string> = {
  [StatusReserva.Pendente]:   'Pendente',
  [StatusReserva.Confirmada]: 'Confirmada',
  [StatusReserva.Cancelada]:  'Cancelada',
  [StatusReserva.Expirada]:   'Expirada',
  [StatusReserva.Utilizada]:  'Utilizada',
}

export const FORMA_PAGAMENTO_LABEL: Record<FormaPagamento, string> = {
  [FormaPagamento.Dinheiro]: 'Dinheiro',
  [FormaPagamento.Cartao]:   'Cartão',
  [FormaPagamento.Pix]:      'Pix',
}

export const ROLE_LABEL: Record<Role, string> = {
  [Role.Admin]:      'Admin',
  [Role.Operador]:   'Operador',
  [Role.Cliente]:    'Cliente',
  [Role.SuperAdmin]: 'Super Admin',
}

// ─── Relatórios ──────────────────────────────────────────────────────────────

export interface ResumoFinanceiroDto {
  receita_hoje:     number
  receita_total:    number
  ticket_medio:     number
  transacoes_hoje:  number
  reservas_ativas:  number
}

// ─── Usuários (listagem admin) ────────────────────────────────────────────────

export interface UsuarioListDto {
  id:         number
  nome:       string
  email:      string
  role:       Role
  created_at: string
}
