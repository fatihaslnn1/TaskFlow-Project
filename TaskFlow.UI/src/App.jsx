import React, { useState, useEffect } from 'react';
import Login from './components/Login';
import KanbanBoard from './components/KanbanBoard';
import TaskAnalytics from './components/TaskAnalytics';
import TaskTable from './components/TaskTable';
import api from './services/api';

export default function App() {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [projects, setProjects] = useState([]);
  const [selectedProject, setSelectedProject] = useState(null);
  const [issues, setIssues] = useState([]);
  const [viewMode, setViewMode] = useState('kanban'); // 'kanban' veya 'table'

  useEffect(() => {
    if (token) {
      fetchProjects();
    }
  }, [token]);

  useEffect(() => {
    if (selectedProject) {
      fetchIssues(selectedProject.id);
    }
  }, [selectedProject]);

  const fetchProjects = async () => {
    try {
      const res = await api.get('/Projects');
      setProjects(res.data);
    } catch (err) {
      console.error('Projeler yüklenemedi:', err);
    }
  };

  const fetchIssues = async (projectId) => {
    try {
      const res = await api.get(`/Issues/project/${projectId}`);
      setIssues(res.data);
    } catch (err) {
      console.error('Görevler yüklenemedi:', err);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setSelectedProject(null);
  };

  // Eğer kullanıcı giriş yapmadıysa Login ekranını göster
  if (!token) {
    return <Login onLoginSuccess={() => setToken(localStorage.getItem('token'))} />;
  }

  return (
    <div className="min-h-screen bg-slate-50 text-slate-800 font-sans">
      {/* Üst Bilgi Çubuğu */}
      <header className="bg-white border-b border-slate-200 px-6 py-4 flex justify-between items-center shadow-sm">
        <h1 className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-violet-600 bg-clip-text text-transparent">
          TaskFlow Yönetim Paneli
        </h1>
        <button 
          onClick={handleLogout}
          className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition text-sm shadow-sm"
        >
          Çıkış Yap
        </button>
      </header>

      {/* Ana İçerik Alanı */}
      <main className="p-6 max-w-7xl mx-auto">
        {!selectedProject ? (
          // Proje Seçim Ekranı
          <div className="bg-white rounded-2xl border border-slate-200 p-8 shadow-sm">
            <h2 className="text-xl font-bold mb-6 text-slate-700">Projelerim</h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {projects.map(project => (
                <div 
                  key={project.id}
                  onClick={() => setSelectedProject(project)}
                  className="p-6 border border-slate-200 rounded-xl hover:border-indigo-500 hover:shadow-md transition cursor-pointer bg-slate-50/50 flex flex-col justify-between group"
                >
                  <div>
                    <span className="text-xs font-semibold px-2.5 py-1 bg-indigo-100 text-indigo-700 rounded-md">
                      {project.projectKey}
                    </span>
                    <h3 className="font-bold text-lg text-slate-800 mt-3 group-hover:text-indigo-600 transition">
                      {project.projectName}
                    </h3>
                  </div>
                  <span className="text-sm font-medium text-indigo-600 mt-6 inline-flex items-center gap-1">
                    Panoya Git →
                  </span>
                </div>
              ))}
            </div>
          </div>
        ) : (
          // Proje İçi Görünüm (Kanban, Tablo, Grafik)
          <div>
            {/* Görünüm Değiştirici Sekmeler */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
              <div className="flex gap-2 bg-slate-200 p-1.5 rounded-xl shadow-inner">
                <button 
                  onClick={() => setViewMode('kanban')}
                  className={`px-4 py-2 rounded-lg font-medium text-sm transition shadow-sm ${viewMode === 'kanban' ? 'bg-white text-indigo-600' : 'text-slate-600 hover:text-slate-900 bg-transparent'}`}
                >
                  Kanban Panosu (Sürükle-Bırak)
                </button>
                <button 
                  onClick={() => setViewMode('table')}
                  className={`px-4 py-2 rounded-lg font-medium text-sm transition shadow-sm ${viewMode === 'table' ? 'bg-white text-indigo-600' : 'text-slate-600 hover:text-slate-900 bg-transparent'}`}
                >
                  Tablo Görünümü (TanStack Table)
                </button>
              </div>

              <button 
                onClick={() => setSelectedProject(null)}
                className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition text-sm shadow-sm"
              >
                ← Proje Listesine Dön
              </button>
            </div>

            {/* İstatistik Grafiği */}
            {issues.length > 0 && <TaskAnalytics issues={issues} />}

            {/* Seçilen Görünüm (Kanban veya Tablo) */}
            {viewMode === 'kanban' ? (
              <KanbanBoard project={selectedProject} onBack={() => setSelectedProject(null)} />
            ) : (
              <TaskTable issues={issues} />
            )}
          </div>
        )}
      </main>
    </div>
  );
}