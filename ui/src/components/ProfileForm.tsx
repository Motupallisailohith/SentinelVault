// src/components/ProfileForm.tsx
import React, { useState, useEffect } from 'react';
import { X, Save } from 'lucide-react';
import { Profile } from '../api/profiles';

export interface ProfileFormProps {
  profile?: Profile;                                    // ← editing
  onSubmit: (vals: Omit<Profile, 'id' | 'createdAt' | 'updatedAt'>) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

const engines: Profile['engine'][] = ['mysql', 'postgres', 'sqlite'];

const blank: Omit<Profile, 'id' | 'createdAt' | 'updatedAt'> = {
  name: '',
  description: '',
  engine: 'mysql',
  host: 'localhost',
  port: 3306,
  username: 'root',
  password: '',
  database: '',
  outPath: './backups',
  compression: 'zstd',
  s3Bucket: '',
  s3Region: '',
  openAiLogs: false,
  environment: 'development'
};

// Validate profile data before submission
const validateProfile = (profile: typeof blank): boolean => {
  if (!profile.name.trim()) {
    alert('Please enter a profile name');
    return false;
  }
  if (!['mysql', 'postgres', 'sqlite'].includes(profile.engine)) {
    alert('Invalid database engine');
    return false;
  }
  if (typeof profile.port !== 'number' || profile.port < 0 || profile.port > 65535) {
    alert('Port must be a valid number between 0 and 65535');
    return false;
  }
  if (!profile.database.trim()) {
    alert('Please enter a database name');
    return false;
  }
  if (!profile.s3Bucket.trim()) {
    alert('Please enter an S3 bucket name');
    return false;
  }
  if (!profile.s3Region.trim()) {
    alert('Please enter an S3 region');
    return false;
  }
  if (!['development', 'staging', 'production'].includes(profile.environment)) {
    alert('Invalid environment type');
    return false;
  }
  return true;
};

export default function ProfileForm({
  profile,
  onSubmit,
  onCancel,
  isLoading = false,
}: ProfileFormProps) {
  const [vals, setVals] = useState(blank);

  // populate defaults when editing ------------------------------------------
  useEffect(() => {
    if (profile) {
      const { id, createdAt, updatedAt, ...rest } = profile;
      setVals(rest);
    }
  }, [profile]);

  // helpers ------------------------------------------------------------------
  const set = (k: keyof typeof vals, v: any) =>
    setVals((p) => ({ ...p, [k]: v }));

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateProfile(vals)) {
      onSubmit(vals);
    }
  };

  // view ---------------------------------------------------------------------
  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white w-full max-w-3xl rounded-xl shadow-xl">
        {/* header */}
        <div className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-xl font-semibold">
            {profile ? 'Edit profile' : 'New profile'}
          </h2>
          <button
            onClick={onCancel}
            className="text-gray-500 hover:text-gray-700"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* row 1 –––––––––––––––––––––––––––––––––––––––––––––––––––––––––– */}
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm mb-1">Display name *</label>
              <input
                required
                value={vals.name}
                onChange={(e) => set('name', e.target.value)}
                className="form-input"
              />
            </div>
            <div>
              <label className="block text-sm mb-1">Engine *</label>
              <select
                value={vals.engine}
                onChange={(e) => {
                  const eng = e.target.value as Profile['engine'];
                  // pick sane default port when engine changes
                  setVals((p) => ({
                    ...p,
                    engine: eng,
                    port: eng === 'postgres' ? 5432 : eng === 'mysql' ? 3306 : 0,
                  }));
                }}
                className="form-select"
              >
                {engines.map((e) => (
                  <option key={e}>{e}</option>
                ))}
              </select>
            </div>
          </div>

          {/* row 2 –––––––––––––––––––––––––––––––––––––––––––––––––––––––––– */}
          <div>
            <label className="block text-sm mb-1">Description</label>
            <textarea
              rows={2}
              value={vals.description}
              onChange={(e) => set('description', e.target.value)}
              className="form-textarea"
            />
          </div>

          {/* row 3 –––––––––––––––––––––––––––––––––––––––––––––––––––––––––– */}
          <div className="grid md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm mb-1">Host *</label>
              <input
                required
                value={vals.host}
                onChange={(e) => set('host', e.target.value)}
                className="form-input"
              />
            </div>
            <div>
              <label className="block text-sm mb-1">Port *</label>
              <input
                required
                type="number"
                value={vals.port}
                onChange={(e) => set('port', parseInt(e.target.value, 10))}
                className="form-input"
              />
            </div>
            <div>
              <label className="block text-sm mb-1">Database *</label>
              <input
                required
                value={vals.database}
                onChange={(e) => set('database', e.target.value)}
                className="form-input"
              />
            </div>
          </div>

          {/* row 4 –––––––––––––––––––––––––––––––––––––––––––––––––––––––––– */}
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm mb-1">User *</label>
              <input
                required
                value={vals.username}
                onChange={(e) => set('username', e.target.value)}
                className="form-input"
              />
            </div>
            <div>
              <label className="block text-sm mb-1">Password *</label>
              <input
                required
                type="password"
                value={vals.password}
                onChange={(e) => set('password', e.target.value)}
                className="form-input"
              />
            </div>
          </div>

          {/* row 5 –––––––––––––––––––––––––––––––––––––––––––––––––––––––––– */}
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm mb-1">Backup folder</label>
              <input
                value={vals.outPath}
                onChange={(e) => set('outPath', e.target.value)}
                className="form-input"
              />
            </div>
            <div>
              <label className="block text-sm mb-1">Compression</label>
              <select
                value={vals.compression}
                onChange={(e) => set('compression', e.target.value)}
                className="form-select"
              >
                <option value="zstd">zstd</option>
                {/* easy to extend later */}
              </select>
            </div>
          </div>

          {/* actions -------------------------------------------------------- */}
          <div className="flex justify-end space-x-3 pt-6">
            <button
              type="button"
              onClick={onCancel}
              className="px-6 py-3 text-sm bg-gray-100 rounded-lg hover:bg-gray-200"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="px-6 py-3 text-sm text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center"
            >
              <Save className="w-4 h-4 mr-2" />
              {isLoading ? 'Saving…' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
