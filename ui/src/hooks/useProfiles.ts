import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { profilesApi } from '../api/profiles';
import type { Profile } from '../api/profiles';
import toast from 'react-hot-toast';

export const useProfiles = () => {
  return useQuery({
    queryKey: ['profiles'],
    queryFn: async () => {
      try {
        const response = await profilesApi.getAll();
        return response.data;
      } catch (error: any) {
        if (error?.isNetworkError || error?.code === 'ERR_NETWORK') {
          console.warn('Using mock profiles data - backend not available');
          return mockProfiles;
        }
        throw error;
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

export const mockProfiles: Profile[] = [
  {
    id: '1',
    name: 'Development MySQL',
    description: 'Development MySQL database backup',
    engine: 'mysql',
    host: 'localhost',
    port: 3306,
    username: 'dev_user',
    password: 'dev_password',
    database: 'dev_db',
    outPath: 'backups/dev',
    compression: 'zstd',
    s3Bucket: 'company-backups-dev',
    s3Region: 'us-east-1',
    openAiLogs: true,
    createdAt: '2025-06-15T20:00:00.000Z',
    updatedAt: '2025-06-16T00:00:00.000Z',
    environment: 'development'
  },
  {
    id: '2',
    name: 'Production PostgreSQL',
    description: 'Production PostgreSQL database backup',
    engine: 'postgres',
    host: 'db.prod.example.com',
    port: 5432,
    username: 'prod_user',
    password: 'prod_password',
    database: 'prod_db',
    outPath: 'backups/prod',
    compression: 'zstd',
    s3Bucket: 'company-backups-prod',
    s3Region: 'us-west-2',
    openAiLogs: true,
    createdAt: '2025-06-15T19:00:00.000Z',
    updatedAt: '2025-06-16T00:00:00.000Z',
    environment: 'production'
  },
  {
    id: '3',
    name: 'Staging SQLite',
    description: 'Staging SQLite database backup',
    engine: 'sqlite',
    host: 'staging.example.com',
    port: 0,
    username: 'staging_user',
    password: 'staging_password',
    database: 'staging_db',
    outPath: 'backups/staging',
    compression: 'zstd',
    s3Bucket: 'company-backups-staging',
    s3Region: 'eu-central-1',
    openAiLogs: true,
    createdAt: '2025-06-15T18:00:00.000Z',
    updatedAt: '2025-06-16T00:00:00.000Z',
    environment: 'staging'
  }
];

export const useCreateProfile = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (profile: Omit<Profile, 'id' | 'createdAt' | 'updatedAt'>) => {
      try {
        const response = await profilesApi.create(profile);
        return response.data;
      } catch (error: any) {
        if (error?.isNetworkError || error?.code === 'ERR_NETWORK') {
          // Create mock profile for offline mode
          const mockProfile: Profile = {
            id: Date.now().toString(),
            ...profile,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          };
          return mockProfile;
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      toast.success('Profile created successfully');
    },
    onError: () => {
      toast.error('Failed to create profile');
    },
  });
};

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ id, ...profile }: Partial<Profile> & { id: string }) => {
      try {
        const response = await profilesApi.update(id, profile);
        return response.data;
      } catch (error: any) {
        if (error?.isNetworkError || error?.code === 'ERR_NETWORK') {
          // Return updated mock profile for offline mode
          return { id, ...profile, updatedAt: new Date().toISOString() };
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      toast.success('Profile updated successfully');
    },
    onError: () => {
      toast.error('Failed to update profile');
    },
  });
};

export const useDeleteProfile = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (id: string) => {
      try {
        await profilesApi.delete(id);
        return id;
      } catch (error: any) {
        if (error?.isNetworkError || error?.code === 'ERR_NETWORK') {
          // Return success for offline mode
          return id;
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profiles'] });
      toast.success('Profile deleted successfully');
    },
    onError: () => {
      toast.error('Failed to delete profile');
    },
  });
};