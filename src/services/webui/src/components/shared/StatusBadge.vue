<template>
  <Tag :value="label" :class="`status-badge status-${variant}`" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Tag from 'primevue/tag'
import { StatusVaga, StatusReserva, STATUS_VAGA_LABEL, STATUS_RESERVA_LABEL } from '@/types'

const props = defineProps<{
  type:  'vaga' | 'reserva'
  value: number
}>()

const label = computed(() => {
  if (props.type === 'vaga')    return STATUS_VAGA_LABEL[props.value as StatusVaga]
  if (props.type === 'reserva') return STATUS_RESERVA_LABEL[props.value as StatusReserva]
  return String(props.value)
})

const variant = computed(() => {
  if (props.type === 'vaga') {
    const map: Record<StatusVaga, string> = {
      [StatusVaga.Disponivel]: 'available',
      [StatusVaga.Ocupada]:    'occupied',
      [StatusVaga.Reservada]:  'reserved',
      [StatusVaga.Manutencao]: 'maintenance',
    }
    return map[props.value as StatusVaga] ?? 'default'
  }
  if (props.type === 'reserva') {
    const map: Record<StatusReserva, string> = {
      [StatusReserva.Pendente]:   'reserved',
      [StatusReserva.Confirmada]: 'available',
      [StatusReserva.Cancelada]:  'occupied',
      [StatusReserva.Expirada]:   'maintenance',
      [StatusReserva.Utilizada]:  'default',
    }
    return map[props.value as StatusReserva] ?? 'default'
  }
  return 'default'
})
</script>

<style scoped>
.status-badge { font-size: var(--font-xs); font-weight: 600; border-radius: var(--radius-full); }
.status-available   { background: rgba(16,185,129,0.12) !important; color: #065f46 !important; }
.status-occupied    { background: rgba(239,68,68,0.12)  !important; color: #991b1b !important; }
.status-reserved    { background: rgba(245,158,11,0.12) !important; color: #92400e !important; }
.status-maintenance { background: rgba(100,116,139,0.12)!important; color: #334155 !important; }
.status-default     { background: rgba(100,116,139,0.12)!important; color: #475569 !important; }
</style>
