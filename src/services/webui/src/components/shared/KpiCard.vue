<template>
  <div class="kpi-card" :style="{ '--kpi-accent': accentColor }">
    <div class="kpi-header">
      <i :class="`pi ${icon} kpi-icon`" />
      <span class="kpi-label">{{ label }}</span>
    </div>
    <div class="kpi-value">{{ value }}</div>
    <div v-if="trend !== undefined" class="kpi-trend" :class="trendClass">
      <i :class="`pi ${trendIcon}`" />
      <span>{{ trendLabel }}</span>
    </div>
    <div v-if="subtitle" class="kpi-subtitle">{{ subtitle }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  label:    string
  value:    string | number
  icon:     string
  accent?:  string
  trend?:   number
  subtitle?: string
}>()

const accentColor = computed(() => props.accent ?? 'var(--color-primary)')

const trendClass = computed(() => {
  if (props.trend === undefined) return ''
  if (props.trend > 0) return 'trend-up'
  if (props.trend < 0) return 'trend-down'
  return 'trend-neutral'
})

const trendIcon = computed(() => {
  if (props.trend === undefined) return ''
  if (props.trend > 0) return 'pi-arrow-up'
  if (props.trend < 0) return 'pi-arrow-down'
  return 'pi-minus'
})

const trendLabel = computed(() => {
  if (props.trend === undefined) return ''
  const abs = Math.abs(props.trend)
  return `${props.trend > 0 ? '+' : ''}${abs.toFixed(1)}%`
})
</script>

<style scoped>
.kpi-card {
  background: var(--color-surface-card);
  border-radius: var(--radius-md);
  border-top: 4px solid var(--kpi-accent);
  padding: var(--space-lg);
  box-shadow: var(--shadow-card);
}

.kpi-header {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-bottom: var(--space-sm);
}

.kpi-icon {
  font-size: 1.25rem;
  color: var(--kpi-accent);
}

.kpi-label {
  font-size: var(--font-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--p-surface-500);
}

.kpi-value {
  font-size: var(--font-numeric);
  font-weight: 700;
  color: var(--p-surface-800);
  line-height: 1;
  margin-bottom: var(--space-xs);
}

.kpi-trend {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: var(--font-xs);
  font-weight: 600;
}

.trend-up      { color: var(--color-available); }
.trend-down    { color: var(--color-occupied); }
.trend-neutral { color: var(--p-surface-400); }

.kpi-subtitle {
  font-size: var(--font-xs);
  color: var(--p-surface-400);
  margin-top: var(--space-xs);
}
</style>
