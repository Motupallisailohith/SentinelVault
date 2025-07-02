import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateProfile } from '../hooks/useProfiles';
import { Profile } from '../api/profiles';
import ProfileForm from '../components/ProfileForm';

export default function NewProfile() {
  const navigate = useNavigate();
  const createProfile = useCreateProfile();

  const handleSubmit = (vals: Omit<Profile, 'id' | 'createdAt' | 'updatedAt'>) => {
    createProfile.mutate(vals, {
      onSuccess: (res) => {
        // react-query returns the newly created profile in res.data
        navigate(`/profiles/${res.data.id}`);
      },
    });
  };

  const handleCancel = () => navigate(-1);

  return (
    <div className="p-8 max-w-lg mx-auto">
      <h1 className="text-2xl font-bold mb-4">New Profile</h1>
      <ProfileForm
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        isLoading={createProfile.isLoading}
      />
    </div>
  );
}
