<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <div id="app">
    <header class="app-header">
      <div class="container">
        <h1 class="app-title">Viadex Device Portal</h1>
        <nav class="app-nav">
          <RouterLink to="/devices" class="nav-link">Devices</RouterLink>
        </nav>
      </div>
    </header>

    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  line-height: 1.6;
  color: #333;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-header {
  background: white;
  border-bottom: 1px solid #e5e7eb;
  padding: 1rem 0;
}

.container {
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.app-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1e293b;
}

.app-nav {
  display: flex;
  gap: 1rem;
}

.nav-link {
  padding: 0.5rem 1rem;
  text-decoration: none;
  color: #64748b;
  font-weight: 500;
  border-radius: 0.375rem;
  transition: all 0.15s ease;
}

.nav-link:hover,
.nav-link.router-link-active {
  color: #3b82f6;
  background-color: #eff6ff;
}

.app-main {
  flex: 1;
}

@media (max-width: 768px) {
  .container {
    padding: 0 1rem;
    flex-direction: column;
    gap: 1rem;
  }

  .app-title {
    font-size: 1.25rem;
  }
}
</style>
