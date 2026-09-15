import React from 'react';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import { Doughnut } from 'react-chartjs-2';

ChartJS.register(ArcElement, Tooltip, Legend);

export default function TaskAnalytics({ issues }) {
  const priorityCounts = {
    Urgent: issues.filter(i => i.priority === 'Urgent').length,
    High: issues.filter(i => i.priority === 'High').length,
    Medium: issues.filter(i => i.priority === 'Medium').length,
    Low: issues.filter(i => i.priority === 'Low').length,
  };

  const data = {
    labels: ['Acil', 'Yüksek', 'Orta', 'Düşük'],
    datasets: [
      {
        data: [priorityCounts.Urgent, priorityCounts.High, priorityCounts.Medium, priorityCounts.Low],
        backgroundColor: ['#ef4444', '#f59e0b', '#3b82f6', '#64748b'],
        borderWidth: 2,
        borderColor: '#ffffff',
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          boxWidth: 12,
          padding: 15,
          font: {
            size: 12,
          },
          color: '#334155',
        },
      },
    },
  };

  return (
    <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm mb-6 w-full max-w-sm">
      <h3 className="font-bold text-slate-700 text-sm mb-4">Görev Öncelik Dağılımı</h3>
      <div className="w-52 mx-auto">
        <Doughnut data={data} options={options} />
      </div>
    </div>
  );
}