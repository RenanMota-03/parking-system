<template>
  <div class="app-shell">
    <AppSidebar :expanded="false" />
    <AppTopNav />
    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import AppSidebar from './AppSidebar.vue'
import AppTopNav from './AppTopNav.vue'
</script>

<style scoped>
.app-shell {
  min-height: 100vh;
  background: var(--color-surface);
}

.app-main {
  margin-left: var(--sidebar-collapsed);
  margin-top: var(--topnav-height);
  padding: var(--space-xl);
  min-height: calc(100vh - var(--topnav-height));
}
</style>
