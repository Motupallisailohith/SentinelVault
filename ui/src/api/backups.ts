import { httpClient } from './http';

export interface BackupInfo {
  fileName: string;
  utcTimestamp: string;
  type: string;
  sizeBytes: number;
}

export interface AiExplanation {
  runId: string;
  explanation: string;
}

export interface BackupHistory {
  timestamp: string;
  type: string;
  sizeBytes: number | null;
  success: boolean;
}

export const backupsApi = {
  // List all backups for a given profile
  list: (profileId: string) =>
    httpClient.get<BackupInfo[]>(`/api/backups/${profileId}`),

  // Trigger a full backup for that profile
  runFull: (profileId: string) =>
    httpClient.post<void>(`/api/backups/${profileId}/full`),

  // Trigger an incremental backup
  runInc: (profileId: string) =>
    httpClient.post<void>(`/api/backups/${profileId}/incremental`),

  // Restore from a backup
  restore: (payload: {
    engine: string;
    host: string;
    port: number;
    user: string;
    password: string;
    database: string;
    sourceFileName: string;
  }) => httpClient.post('/api/restores', payload),

  // Get backup history for a profile
  history: (profileId: string) =>
    httpClient.get<BackupHistory[]>(`/api/backups/${profileId}/history`),

  // Ask AI to explain a particular run (runId)
  explain: (runId: string) =>
    httpClient.get<AiExplanation>(`/api/ai/explain/${runId}`),
};
