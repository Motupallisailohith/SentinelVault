// src/components/Sidebar.tsx
import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Shield, Home, Settings, Plus, User } from 'lucide-react';
import { useProfiles } from '../hooks/useProfiles';

export const Sidebar: React.FC = () => {
  const location = useLocation();
  const { data: profiles = [] } = useProfiles();

  const isActive = (path: string) => location.pathname === path;

  return (
    <aside className="fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-xl border-r border-gray-200">
      <div className="flex flex-col h-full">
        {/* Brand */}
        <div className="flex items-center px-6 py-6 border-b border-gray-200">
          <div className="p-2 bg-blue-100 rounded-lg mr-3">
            <Shield className="w-8 h-8 text-blue-600" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-gray-900">SentinelVault</h1>
            <p className="text-sm text-gray-500">DB Backup CLI</p>
          </div>
        </div>

        {/* Main Nav */}
        <nav className="flex-1 px-4 py-6 space-y-2">
          <Link
            to="/"
            className={`flex items-center px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 ${
              isActive('/') ? 'bg-blue-50 text-blue-700 shadow-sm' : 'text-gray-700 hover:bg-gray-50'
            }`}
          >
            <Home className="w-5 h-5 mr-3" /> Dashboard
          </Link>

          <Link
            to="/settings"
            className={`flex items-center px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 ${
              isActive('/settings') ? 'bg-blue-50 text-blue-700 shadow-sm' : 'text-gray-700 hover:bg-gray-50'
            }`}
          >
            <Settings className="w-5 h-5 mr-3" /> Settings
          </Link>
        </nav>

        {/* Profiles List */}
        <div className="px-4 py-4 border-t border-gray-200">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold text-gray-900">Profiles</h3>
            <Link
              to="/profiles/new"
              className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg"
              title="Add New Profile"
            >
              <Plus className="w-4 h-4" />
            </Link>
          </div>

          <div className="space-y-2 max-h-48 overflow-y-auto">
            {profiles.map((p) => (
              <Link
                key={p.id}
                to={`/profiles/${p.id}`}
                className={`block px-3 py-3 rounded-lg text-sm transition-all duration-200 ${
                  location.pathname === `/profiles/${p.id}`
                    ? 'bg-blue-50 text-blue-700 shadow-sm'
                    : 'text-gray-600 hover:bg-gray-50'
                }`}
              >
                <div className="flex items-center">
                  <User className="w-4 h-4 mr-2 opacity-60" />
                  <div className="truncate font-medium">{p.name}</div>
                </div>
              </Link>
            ))}

            {!profiles.length && (
              <div className="text-center py-6">
                <User className="w-8 h-8 text-gray-300 mx-auto mb-2" />
                <p className="text-xs text-gray-500">No profiles yet</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </aside>
  );
};
