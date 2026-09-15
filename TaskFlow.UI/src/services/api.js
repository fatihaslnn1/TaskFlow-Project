import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5045/api',
});

// Her istek öncesinde localStorage'dan token'ı alıp Header'a ekler
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;