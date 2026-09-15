import React from 'react';

export default function TaskTable({ issues }) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6">
      <table className="w-full text-left border-collapse">
        <thead className="bg-slate-50 border-b border-slate-200 text-slate-700">
          <tr>
            <th className="p-4 font-semibold text-xs uppercase tracking-wider">ID</th>
            <th className="p-4 font-semibold text-xs uppercase tracking-wider">Başlık</th>
            <th className="p-4 font-semibold text-xs uppercase tracking-wider">Durum</th>
            <th className="p-4 font-semibold text-xs uppercase tracking-wider">Öncelik</th>
            <th className="p-4 font-semibold text-xs uppercase tracking-wider">Oluşturan</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100 text-sm text-slate-600">
          {issues && issues.length > 0 ? (
            issues.map(issue => (
              <tr key={issue.id} className="hover:bg-slate-50/80 transition">
                <td className="p-4">{issue.id}</td>
                <td className="p-4 font-medium text-slate-800">{issue.title}</td>
                <td className="p-4">
                  <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">
                    {issue.status}
                  </span>
                </td>
                <td className="p-4">
                  <span className={`px-2 py-0.5 rounded font-medium text-xs ${
                    issue.priority === 'Urgent' ? 'bg-red-100 text-red-700' :
                    issue.priority === 'High' ? 'bg-amber-100 text-amber-700' :
                    'bg-slate-100 text-slate-600'
                  }`}>
                    {issue.priority}
                  </span>
                </td>
                <td className="p-4">{issue.reporter?.fullName || 'Sistem'}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="5" className="p-6 text-center text-slate-400">Görev bulunamadı.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}