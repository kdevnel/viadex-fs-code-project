// Native fetch-based API client for Viadex Device Portal
// Provides Axios-like functionality without external dependencies

interface ApiConfig {
  baseURL: string;
  timeout?: number;
  headers?: Record<string, string>;
}

interface ApiResponse<T = unknown> {
  data: T;
  status: number;
  statusText: string;
  headers: Headers;
}

class ApiError extends Error {
  status?: number;
  statusText?: string;

  constructor(options: { message: string; status?: number; statusText?: string }) {
    super(options.message);
    this.name = 'ApiError';
    this.status = options.status;
    this.statusText = options.statusText;
  }
}

class ApiClient {
  private config: ApiConfig;

  constructor(config: ApiConfig) {
    this.config = {
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
      ...config,
    };
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = `${this.config.baseURL}${endpoint}`;

    // Merge headers
    const headers = new Headers({
      ...this.config.headers,
      ...options.headers,
    });

    // Setup timeout
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), this.config.timeout);

    try {
      const response = await fetch(url, {
        ...options,
        headers,
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      // Handle non-2xx responses
      if (!response.ok) {
        throw new ApiError({
          message: `HTTP ${response.status}: ${response.statusText}`,
          status: response.status,
          statusText: response.statusText,
        });
      }

      // Parse JSON response only if there's content
      // HTTP 204 (No Content) and similar responses have no body
      let data;
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        data = null;
      } else {
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
          data = await response.json();
        } else {
          data = null;
        }
      }

      return {
        data,
        status: response.status,
        statusText: response.statusText,
        headers: response.headers,
      };
    } catch (error) {
      clearTimeout(timeoutId);

      if (error instanceof Error && error.name === 'AbortError') {
        throw new ApiError({ message: 'Request timeout' });
      }

      if (error instanceof ApiError) {
        throw error;
      }

      throw new ApiError({
        message: error instanceof Error ? error.message : 'Network error occurred'
      });
    }
  }

  // HTTP methods
  async get<T>(endpoint: string, params?: Record<string, string | number>): Promise<ApiResponse<T>> {
    const url = params ? `${endpoint}?${new URLSearchParams(params as Record<string, string>)}` : endpoint;
    return this.request<T>(url);
  }

  async post<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async put<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'DELETE',
    });
  }
}

// Create API client instance
const apiClient = new ApiClient({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7027',
});

export default apiClient;
export type { ApiResponse, ApiError };
