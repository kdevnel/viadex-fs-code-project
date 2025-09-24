// Simplified quote store for quote calculation only
import { defineStore } from 'pinia';
import { ref } from 'vue';
import { quoteApi } from '@/services/quoteApi';
import type {
  QuoteCalculation,
  QuoteCalculateRequest
} from '@/types/quote';

export const useQuoteCalculatorStore = defineStore('quoteCalculator', () => {
  // Quote calculation state
  const calculatedQuote = ref<QuoteCalculation | null>(null);
  const calculationLoading = ref(false);
  const calculationError = ref<string | null>(null);

  // Actions
  const calculateQuote = async (request: QuoteCalculateRequest) => {
    calculationLoading.value = true;
    calculationError.value = null;
    calculatedQuote.value = null;

    try {
      const quote = await quoteApi.calculateQuote(request);
      if (quote) {
        calculatedQuote.value = quote;
        return { success: true, quote };
      } else {
        throw new Error('Failed to calculate quote');
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to calculate quote';
      calculationError.value = errorMessage;
      console.error('Error calculating quote:', errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      calculationLoading.value = false;
    }
  };

  const clearCalculatedQuote = () => {
    calculatedQuote.value = null;
    calculationError.value = null;
  };

  return {
    // State
    calculatedQuote,
    calculationLoading,
    calculationError,

    // Actions
    calculateQuote,
    clearCalculatedQuote
  };
});
