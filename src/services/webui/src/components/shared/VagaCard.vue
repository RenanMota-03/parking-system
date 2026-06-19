<template>
  <div
    class="vaga-card"
    :class="[`vaga-${statusKey}`, { 'vaga-pulse': vaga.status === StatusVaga.Reservada, 'vaga-clickable': clickable }]"
    :title="tooltipText"
  >
    <div class="vaga-status-bar" />
    <div class="vaga-body">
      <i :class="`pi ${typeIcon} vaga-icon`" />
      <span class="vaga-numero">{{ vaga.numero }}</span>
      <span class="vaga-status-label">{{ STATUS_VAGA_LABEL[vaga.status] }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { StatusVaga, TipoVaga, STATUS_VAGA_LABEL, TIPO_VAGA_LABEL } from '@/types'
import type { VagaDto } from '@/types'

const props = defineProps<{ vaga: VagaDto; clickable?: boolean }>()

const statusKey = computed(() => StatusVaga[props.vaga.status].toLowerCase())

const typeIcon = computed(() => {
  const map: Record<TipoVaga, string> = {
    [TipoVaga.Carro]:             'pi-car',
    [TipoVaga.Moto]:              'pi-bolt',
    [TipoVaga.Caminhonete]:       'pi-truck',
    [TipoVaga.DeficienteOuIdoso]: 'pi-heart',
  }
  return map[props.vaga.tipo_vaga] ?? 'pi-car'
})

const tooltipText = computed(() =>
  `${props.vaga.numero} | ${TIPO_VAGA_LABEL[props.vaga.tipo_vaga]} | ${STATUS_VAGA_LABEL[props.vaga.status]}`
)
</script>

<style scoped>
.vaga-card {
  background: var(--color-surface-card);
  border-radius: var(--radius-md);
  border: 1px solid #e2e8f0;
  overflow: hidden;
  cursor: default;
  transition: transform 0.15s, box-shadow 0.15s;
  position: relative;
}

.vaga-clickable { cursor: pointer; }

.vaga-clickable:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.vaga-status-bar {
  height: 4px;
}

.vaga-disponivel .vaga-status-bar { background: var(--color-available); }
.vaga-ocupada    .vaga-status-bar { background: var(--color-occupied); }
.vaga-reservada  .vaga-status-bar { background: var(--color-reserved); }
.vaga-manutencao .vaga-status-bar { background: var(--color-maintenance); }

.vaga-manutencao { opacity: 0.6; }

.vaga-body {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--space-md) var(--space-sm);
  gap: var(--space-xs);
}

.vaga-icon {
  font-size: 1.5rem;
}

.vaga-disponivel .vaga-icon { color: var(--color-available); }
.vaga-ocupada    .vaga-icon { color: var(--color-occupied); }
.vaga-reservada  .vaga-icon { color: var(--color-reserved); }
.vaga-manutencao .vaga-icon { color: var(--color-maintenance); }

.vaga-numero {
  font-size: var(--font-sm);
  font-weight: 700;
  color: var(--p-surface-800);
}

.vaga-status-label {
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--p-surface-500);
}

.vaga-pulse { animation: pulse-ring 2s infinite; }
</style>
