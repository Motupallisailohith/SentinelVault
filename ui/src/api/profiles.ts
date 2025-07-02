import { httpClient } from './http';
export const backupsApi = {
  list: (profileId: string) =>
    httpClient.get(`/api/backups/${profileId}`),
  runFull: (profileId: string) =>
    httpClient.post(`/api/backups/${profileId}/full`),
  runInc: (profileId: string) =>
    httpClient.post(`/api/backups/${profileId}/incremental`),
  restore: (body: { profileId: string; sourceFileName: string }) =>
    httpClient.post('/api/restores', body),
  explain: (runId: string) =>
    httpClient.get(`/api/ai/explain/${runId}`),
};

export interface Profile {
  id: string;
  name: string;
  description: string;
  engine: 'mysql' | 'postgres' | 'sqlite';
  host: string;
  port: number;
  username: string;
  password: string;
  database: string;
  outPath: string;
  compression: 'zstd';
  s3Bucket: string;
  s3Region: string;
  openAiLogs: boolean;
  createdAt: string;
  updatedAt: string;
  environment: 'development' | 'staging' | 'production';
}

export const profilesApi = {
  getAll: () => httpClient.get<Profile[]>('/api/profiles'),
  
  //create: (profile: Omit<Profile, 'id' | 'createdAt' | 'updatedAt'>) => 
    //httpClient.post<Profile>('/api/profiles', profile),
create: (p: Partial<Profile>) => httpClient.post<Profile>('/api/profiles', p),
  
 // update: (id: string, profile: Partial<Omit<Profile, 'id' | 'createdAt'>>) =>
  //  httpClient.put<Profile>(`/api/profiles/${id}`, profile),
update: (id: string, p: Partial<Profile>) => httpClient.put(`/api/profiles/${id}`, p),
  
  delete: (id: string) => httpClient.delete(`/api/profiles/${id}`),
};