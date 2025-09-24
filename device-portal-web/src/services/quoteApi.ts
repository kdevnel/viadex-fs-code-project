// Quote API service for quote calculation only
import api from './api';
import type {
  QuoteCalculation,
  QuoteCalculateRequest
} from '@/types/quote';

export const quoteApi = {
  // Calculate quote (without saving)
  async calculateQuote(request: QuoteCalculateRequest): Promise<QuoteCalculation | null> {
    try {
      const response = await api.post<QuoteCalculation>('/api/quotes/calculate', request);
      return response.data;
    } catch (error) {
      console.error('Error calculating quote:', error);
      return null;
    }
  }
};
