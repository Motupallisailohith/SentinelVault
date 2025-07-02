import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Shield, User, Plus, Calendar, TrendingUp } from 'lucide-react';
import { useProfiles } from '../hooks/useProfiles';
import { mockDashboardStats } from '../mocks/data';

interface HistoryItem {
  profileName: string;
  type: string;
  sizeBytes: number | null;
  timestamp: string;
  success: boolean;
}

interface BackupHistory {
  type: string;
  sizeBytes: number | null;
  timestamp: string;
  success: boolean;
}

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const { data: profiles = [] } = useProfiles();
  const [totalBackups, setTotalBackups] = useState(0);
  const [successRate, setSuccessRate] = useState(0);
  const [recentHistory, setRecentHistory] = useState<HistoryItem[]>([]);

  useEffect(() => {
    setTotalBackups(mockDashboardStats.totalBackups);
    setSuccessRate(mockDashboardStats.successRate);
    setRecentHistory(mockDashboardStats.recentHistory);
  }, []);

  function formatBytes(bytes: number) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  return (
    <div className="p-8">
      {profiles.length === 0 ? (
        <div className="max-w-2xl mx-auto text-center py-16">
          <div className="p-4 bg-blue-100 rounded-full w-20 h-20 mx-auto mb-6 flex items-center justify-center">
            <Shield className="w-10 h-10 text-blue-600" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 mb-4">Welcome to SentinelVault</h2>
          <p className="text-gray-600 mb-8 text-lg leading-relaxed">
            Get started by creating your first profile. Organize and manage your configurations.
          </p>
          <button
            onClick={() => navigate('/profiles/new')}
            className="inline-flex items-center px-8 py-4 bg-blue-600 text-white font-medium rounded-xl hover:bg-blue-700 transition-all duration-200 shadow-lg hover:shadow-xl"
          >
            <Plus className="w-5 h-5 mr-2" /> Create First Profile
          </button>
        </div>
      ) : (
        <>
          <div className="space-y-8">
            {/* Header */}
            <div>
              <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
              <p className="text-gray-600">Manage your profiles and configurations</p>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
            <div className="flex items-center">
              <Calendar className="w-6 h-6 text-green-600 bg-green-100 p-2 rounded-xl" />
              <div className="ml-4">
                <p className="text-sm text-gray-600">Total Backups</p>
                <p className="text-2xl font-bold text-gray-900">{totalBackups}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
            <div className="flex items-center">
              <TrendingUp className="w-6 h-6 text-purple-600 bg-purple-100 p-2 rounded-xl" />
              <div className="ml-4">
                <p className="text-sm text-gray-600">Backup Success Rate</p>
                <p className="text-2xl font-bold text-gray-900">{successRate}%</p>
              </div>
            </div>
          </div>
        </>
      )}

      {/* Recent Backup History */}
      <div className="bg-white rounded-xl shadow border border-gray-100">
        <div className="p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent Backup History</h2>
          <div className="overflow-x-auto">
            <table className="min-w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Profile
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Size
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Timestamp
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white">
                {recentHistory.map((item, index) => (
                  <tr key={index} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {item.profileName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {item.type}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {item.sizeBytes ? formatBytes(item.sizeBytes) : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {new Date(item.timestamp).toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        item.success ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      }`}>
                        {item.success ? 'Success' : 'Failed'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Recent Profiles */}
      <div className="bg-white rounded-xl shadow border border-gray-100">
        <div className="flex items-center justify-between p-6 border-b border-gray-100">
          <h2 className="text-xl font-semibold text-gray-900">Recent Profiles</h2>
          <button
            onClick={() => navigate('/profiles/new')}
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4 mr-2" /> New Profile
          </button>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {profiles.map((profile) => (
              <div
                key={profile.id}
                onClick={() => navigate(`/profiles/${profile.id}`)}
                className="p-4 border border-gray-200 rounded-lg hover:border-blue-300 hover:shadow-md transition-all duration-200 cursor-pointer"
              >
                <div className="flex items-center mb-3">
                  <User className="w-5 h-5 text-gray-600 bg-gray-100 p-2 rounded-lg mr-3" />
                  <h3 className="font-medium text-gray-900 truncate">{profile.name}</h3>
                </div>
                <p className="text-sm text-gray-600">{profile.engine}</p>
                <p className="text-sm text-gray-600">{profile.host}:{profile.port}</p>
                <p className="text-xs text-gray-500">
                  Created {new Date(profile.createdAt).toLocaleDateString()}
                </p>
              </div>
            ))}
          </div>

          {profiles.length > 3 && (
            <div className="mt-6 text-center">
              <button
                onClick={() => navigate('/profiles')}
                className="text-blue-600 hover:text-blue-700 font-medium text-sm"
              >
                View all profiles →
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
