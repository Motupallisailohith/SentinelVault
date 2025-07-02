import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { User, ArrowLeft, Edit2, Trash2 } from 'lucide-react';
import { useProfiles } from '../hooks/useProfiles';
import { mockProfiles, mockBackupHistory } from '../mocks/data';
import BackupsTable from '../components/BackupsTable';
import ProfileForm from '../components/ProfileForm';
import { useDeleteProfile, useUpdateProfile } from '../hooks/useProfiles';

export default function ProfileDetail() {
  const { profileId } = useParams<{ profileId: string }>();
  const navigate = useNavigate();
  const { data: profiles = [], isLoading: profilesLoading } = useProfiles();
  const deleteMutation = useDeleteProfile();
  const updateMutation = useUpdateProfile();
  const [editing, setEditing] = useState(false);

  // Try to find profile in both real and mock data
  const profile = profiles.find(p => p.id === profileId);
  if (!profile) {
    // If not found in real data, check mock data
    const mockProfile = mockProfiles.find(p => p.id === profileId);
    if (mockProfile) {
      return (
        <div className="p-8 max-w-4xl mx-auto space-y-8">
          {/* Header */}
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <button onClick={() => navigate('/')} className="text-gray-400 hover:text-gray-600">
                <ArrowLeft className="w-6 h-6" />
              </button>
              <User className="w-8 h-8 text-blue-600 bg-blue-100 p-2 rounded-xl" />
              <h1 className="text-3xl font-bold">{mockProfile.name}</h1>
            </div>
            <div className="space-x-2">
              <button
                onClick={() => setEditing(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                <Edit2 className="w-4 h-4 inline-block mr-1" />
                Edit
              </button>
              <button
                onClick={() => {
                  if (confirm(`Delete profile “${mockProfile.name}”? This is permanent.`)) {
                    deleteMutation.mutate(mockProfile.id, {
                      onSuccess: () => {
                        navigate('/');
                      }
                    });
                  }
                }}
                className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
              >
                <Trash2 className="w-4 h-4 inline-block mr-1" />
                Delete
              </button>
            </div>
          </div>

          {/* Profile Details */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
              <h2 className="text-xl font-semibold mb-4">Basic Information</h2>
              <p className="mb-2"><strong>Description:</strong> {mockProfile.description || '—'}</p>
              <p className="text-sm text-gray-500">
                ID: <code>{mockProfile.id}</code>
              </p>
            </div>
            <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
              <h2 className="text-xl font-semibold mb-4">Metadata</h2>
              <p className="mb-2">
                <strong>Created:</strong> {new Date(mockProfile.createdAt).toLocaleString()}
              </p>
              <p>
                <strong>Updated:</strong> {new Date(mockProfile.updatedAt).toLocaleString()}
              </p>
            </div>
          </div>

          {/* Backups Table */}
          <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
            <h2 className="text-xl font-semibold mb-4">Backups</h2>
            <BackupsTable profileId={mockProfile.id} />
          </div>

          {/* Edit Form */}
          {editing && (
            <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
              <ProfileForm
                profile={mockProfile}
                onSubmit={(vals) => {
                  updateMutation.mutate({ ...mockProfile, ...vals }, {
                    onSuccess: () => {
                      setEditing(false);
                    }
                  });
                }}
                onCancel={() => setEditing(false)}
              />
            </div>
          )}
        </div>
      );
    }

    // If not found in mock data, show error
    return (
      <div className="p-8 text-center">
        <User className="mx-auto w-12 h-12 text-gray-400 mb-4" />
        <h2 className="text-2xl font-semibold mb-2">Profile Not Found</h2>
        <p className="text-gray-600 mb-6">Couldn't load profile "{profileId}".</p>
        <button
          onClick={() => navigate('/')}
          className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          <ArrowLeft className="w-4 h-4 inline-block mr-2" />
          Back to Dashboard
        </button>
      </div>
    );
  }

  // Show loading state
  if (profilesLoading || deleteMutation.isPending || updateMutation.isPending) {
    return (
      <div className="p-8 text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
        <h2 className="text-2xl font-semibold mb-2">Loading Profile...</h2>
      </div>
    );
  }

  const onDelete = () => {
    if (confirm(`Delete profile “${profile.name}”? This is permanent.`)) {
      deleteProfile.mutate(profile.id, { onSuccess: () => navigate('/') });
    }
  };

  const onUpdate = (vals: Omit<typeof profile, 'id' | 'createdAt' | 'updatedAt'>) => {
    updateProfile.mutate(
      { id: profile.id, ...vals },
      { onSuccess: () => setEditing(false) }
    );
  };

  return (
    <div className="p-8 max-w-4xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <button onClick={() => navigate('/')} className="text-gray-400 hover:text-gray-600">
            <ArrowLeft className="w-6 h-6" />
          </button>
          <User className="w-8 h-8 text-blue-600 bg-blue-100 p-2 rounded-xl" />
          <h1 className="text-3xl font-bold">{profile.name}</h1>
        </div>
        <div className="space-x-2">
          <button
            onClick={() => setEditing(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            <Edit2 className="w-4 h-4 inline-block mr-1" />
            Edit
          </button>
          <button
            onClick={onDelete}
            disabled={deleteProfile.isLoading}
            className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
          >
            <Trash2 className="w-4 h-4 inline-block mr-1" />
            Delete
          </button>
        </div>
      </div>

      {/* Profile Details */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
          <h2 className="text-xl font-semibold mb-4">Basic Information</h2>
          <p className="mb-2"><strong>Description:</strong> {profile.description || '—'}</p>
          <p className="text-sm text-gray-500">
            ID: <code>{profile.id}</code>
          </p>
        </div>
        <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
          <h2 className="text-xl font-semibold mb-4">Metadata</h2>
          <p className="mb-2">
            <strong>Created:</strong> {new Date(profile.createdAt).toLocaleString()}
          </p>
          <p>
            <strong>Updated:</strong> {new Date(profile.updatedAt).toLocaleString()}
          </p>
        </div>
      </div>

      {/* Backups Table */}
      <div className="bg-white rounded-xl shadow p-6 border border-gray-100">
        <h2 className="text-xl font-semibold mb-4">Backups</h2>
        <BackupsTable profileId={profile.id} />
      </div>

      {/* Edit Form Modal */}
      {editing && (
        <ProfileForm
          profile={profile}
          onSubmit={onUpdate}
          onCancel={() => setEditing(false)}
          isLoading={updateProfile.isLoading}
        />
      )}
    </div>
  );
}
