// src/api/http.ts
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export const httpClient = axios.create({
  baseURL:      API_BASE_URL,
  timeout:      5000,
  headers:      { 'Content-Type': 'application/json' },
});

httpClient.interceptors.request.use(
  (config) => {
    // future: inject auth token here
    return config;
  },
  (error) => Promise.reject(error),
);

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.code === 'ECONNREFUSED' || error.isNetworkError) {
      console.warn('Backend API unavailable—switching to offline mode.');
      return Promise.reject({ ...error, isNetworkError: true });
    }
    return Promise.reject(error);
  },
);

export const checkBackendHealth = async (): Promise<boolean> => {
  try {
    await httpClient.get('/health');
    return true;
  } catch {
    return false;
  }
};
