import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import api from '../services/api';
import { DragDropContext, Droppable, Draggable } from '@hello-pangea/dnd';

export default function KanbanBoard({ project, onBack }) {
  const [issues, setIssues] = useState([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [showForm, setShowForm] = useState(false);

  useEffect(() => {
    fetchIssues();

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5045/taskflowHub")
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => {
        connection.on("ReceiveBoardUpdate", () => {
          fetchIssues();
        });
      })
      .catch(err => console.error("SignalR Bağlantı Hatası: ", err));

    return () => {
      connection.stop();
    };
  }, [project.id]);

  const fetchIssues = async () => {
    try {
      const response = await api.get(`/Issues/project/${project.id}`);
      setIssues(response.data);
    } catch (err) {
      console.error('Görevler yüklenemedi:', err);
    }
  };

  const handleDragEnd = async (result) => {
    if (!result.destination) return;

    const { draggableId, destination } = result;
    const newStatus = destination.droppableId;
    const issueId = parseInt(draggableId);

    setIssues(prev => prev.map(i => i.id === issueId ? { ...i, status: newStatus } : i));

    try {
      await api.patch(`/Issues/${issueId}/status`, { status: newStatus });
    } catch (err) {
      console.error('Durum güncellenemedi:', err);
      fetchIssues();
    }
  };

  const handleCreateIssue = async (e) => {
    e.preventDefault();
    try {
      await api.post('/Issues', {
        title,
        description,
        priority,
        projectId: project.id,
        issueType: 'Task'
      });
      setTitle('');
      setDescription('');
      setShowForm(false);
      fetchIssues();
    } catch (err) {
      console.error('Görev oluşturulamadı:', err);
    }
  };

  const statuses = [
    { key: 'To Do', label: 'To Do', bg: 'bg-slate-100' },
    { key: 'In Progress', label: 'In Progress', bg: 'bg-blue-50' },
    { key: 'Done', label: 'Done', bg: 'bg-emerald-50' }
  ];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center bg-white p-4 rounded-xl shadow-sm border border-slate-200">
        <button onClick={onBack} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition text-sm">
          ← Projelere Dön
        </button>
        <h2 className="text-lg font-bold text-slate-800">
          [{project.projectKey}] {project.projectName}
        </h2>
        <button onClick={() => setShowForm(!showForm)} className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium rounded-lg transition text-sm shadow-sm">
          {showForm ? 'Kapat' : '+ Yeni Görev'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreateIssue} className="bg-white p-6 rounded-xl shadow-md border border-slate-200 flex flex-col gap-4">
          <h3 className="text-md font-semibold text-slate-700">Yeni Görev Oluştur</h3>
          <input 
            type="text" 
            placeholder="Görev Başlığı" 
            value={title} 
            onChange={(e) => setTitle(e.target.value)} 
            required 
            className="p-3 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none text-sm"
          />
          <textarea 
            placeholder="Açıklama" 
            value={description} 
            onChange={(e) => setDescription(e.target.value)} 
            className="p-3 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none resize-none h-20 text-sm"
          />
          <div className="flex gap-4">
            <select 
              value={priority} 
              onChange={(e) => setPriority(e.target.value)} 
              className="p-3 border border-slate-300 rounded-lg flex-1 bg-white outline-none text-sm"
            >
              <option value="Low">Düşük (Low)</option>
              <option value="Medium">Orta (Medium)</option>
              <option value="High">Yüksek (High)</option>
              <option value="Urgent">Acil (Urgent)</option>
            </select>
            <button type="submit" className="px-6 py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-lg transition text-sm shadow-sm">
              Kaydet
            </button>
          </div>
        </form>
      )}

      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {statuses.map((status) => {
            const columnIssues = issues.filter(i => i.status === status.key);
            return (
              <Droppable key={status.key} droppableId={status.key}>
                {(provided, snapshot) => (
                  <div 
                    ref={provided.innerRef} 
                    {...provided.droppableProps}
                    className={`${status.bg} rounded-xl p-4 min-h-[500px] border border-slate-200 shadow-inner flex flex-col transition-colors ${snapshot.isDraggingOver ? 'bg-indigo-50/80 border-indigo-300' : ''}`}
                  >
                    <div className="flex justify-between items-center mb-4 pb-2 border-b border-slate-200">
                      <h3 className="font-bold text-slate-700 text-sm">{status.label}</h3>
                      <span className="bg-white px-2.5 py-0.5 rounded-full text-xs font-semibold text-slate-600 shadow-sm border border-slate-200">
                        {columnIssues.length}
                      </span>
                    </div>

                    <div className="flex flex-col gap-3 flex-1">
                      {columnIssues.map((issue, index) => (
                        <Draggable key={issue.id} draggableId={issue.id.toString()} index={index}>
                          {(provided, snapshot) => (
                            <div
                              ref={provided.innerRef}
                              {...provided.draggableProps}
                              {...provided.dragHandleProps}
                              className={`bg-white p-4 rounded-xl border border-slate-200 shadow-sm transition-shadow hover:shadow-md ${snapshot.isDragging ? 'shadow-lg ring-2 ring-indigo-500 bg-indigo-50/50' : ''}`}
                            >
                              <h4 className="font-semibold text-slate-800 text-sm mb-1">{issue.title}</h4>
                              <p className="text-xs text-slate-600 mb-3 line-clamp-2">{issue.description}</p>
                              
                              <div className="flex justify-between items-center pt-2 border-t border-slate-100 text-xs text-slate-500">
                                <span className={`px-2 py-0.5 rounded font-medium ${
                                  issue.priority === 'Urgent' ? 'bg-red-100 text-red-700' :
                                  issue.priority === 'High' ? 'bg-amber-100 text-amber-700' :
                                  'bg-slate-100 text-slate-600'
                                }`}>
                                  {issue.priority}
                                </span>
                                {issue.reporter && <span className="font-medium text-slate-700">{issue.reporter.fullName}</span>}
                              </div>
                            </div>
                          )}
                        </Draggable>
                      ))}
                      {provided.placeholder}
                    </div>
                  </div>
                )}
              </Droppable>
            );
          })}
        </div>
      </DragDropContext>
    </div>
  );
}