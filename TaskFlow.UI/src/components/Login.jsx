import React, { useState } from 'react';
import api from '../services/api';

export default function Login({ onLoginSuccess }) {
  const [email, setEmail] = useState('admin@taskflow.com');
  const [password, setPassword] = useState('Admin123!');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const response = await api.post('/Auth/login', { email, password });
      const { token } = response.data;
      localStorage.setItem('token', token);
      onLoginSuccess();
    } catch (err) {
      setError('Giriş başarısız. E-posta veya şifre hatalı!');
    }
  };

  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', width: '300px', gap: '12px' }}>
        <h2>TaskFlow Giriş</h2>
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <input 
          type="email" 
          placeholder="E-posta" 
          value={email} 
          onChange={(e) => setEmail(e.target.value)} 
          required 
          style={{ padding: '8px' }}
        />
        <input 
          type="password" 
          placeholder="Şifre" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          required 
          style={{ padding: '8px' }}
        />
        <button type="submit" style={{ padding: '10px', backgroundColor: '#007bff', color: '#fff', border: 'none', cursor: 'pointer' }}>
          Giriş Yap
        </button>
      </form>
    </div>
  );
}