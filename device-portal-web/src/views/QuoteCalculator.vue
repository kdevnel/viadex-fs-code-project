<template>
  <div class="page-view">
    <div class="page-header">
      <h1>Quote Calculator</h1>
      <p>Calculate leasing quotes for devices with different support tiers</p>
    </div>

    <QuoteCalculatorComponent />
  </div>
</template>

<script setup lang="ts">
import QuoteCalculatorComponent from '@/components/QuoteCalculator.vue';
</script>

<style scoped>
.page-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;
  text-align: center;
}

.page-header h1 {
  font-size: 2.5rem;
  color: #1f2937;
  margin-bottom: 0.5rem;
}

.page-header p {
  color: #6b7280;
  font-size: 1.1rem;
}
</style>
