import { Profile } from '../api/profiles';
import { BackupHistory } from '../api/backups';

interface HistoryItem {
  profileName: string;
  type: string;
  sizeBytes: number | null;
  timestamp: string;
  success: boolean;
}

export interface DashboardStats {
  totalBackups: number;
  successRate: number;
  recentHistory: HistoryItem[];
}

export const mockDashboardStats: DashboardStats = {
  totalBackups: 11,
  successRate: 78,
  recentHistory: [
    {
      profileName: 'Development MySQL',
      type: 'full',
      sizeBytes: 104857600,
      timestamp: '2025-06-16T03:00:00.000Z',
      success: true
    },
    {
      profileName: 'Production PostgreSQL',
      type: 'incremental',
      sizeBytes: 5242880,
      timestamp: '2025-06-16T02:00:00.000Z',
      success: true
    },
    {
      profileName: 'Staging SQLite',
      type: 'incremental',
      sizeBytes: 2621440,
      timestamp: '2025-06-16T01:00:00.000Z',
      success: true
    }
  ]
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

export const mockBackupHistory: Record<string, BackupHistory[]> = {
  '1': [
    {
      timestamp: '2025-06-15T23:00:00.000Z',
      type: 'full',
      sizeBytes: 104857600, // 100MB
      success: true
    },
    {
      timestamp: '2025-06-16T00:00:00.000Z',
      type: 'incremental',
      sizeBytes: 5242880, // 5MB
      success: true
    },
    {
      timestamp: '2025-06-16T01:00:00.000Z',
      type: 'incremental',
      sizeBytes: 2621440, // 2.5MB
      success: true
    }
  ],
  '2': [
    {
      timestamp: '2025-06-15T22:00:00.000Z',
      type: 'full',
      sizeBytes: 209715200, // 200MB
      success: true
    },
    {
      timestamp: '2025-06-15T23:00:00.000Z',
      type: 'incremental',
      sizeBytes: 10485760, // 10MB
      success: true
    },
    {
      timestamp: '2025-06-16T00:00:00.000Z',
      type: 'incremental',
      sizeBytes: 5242880, // 5MB
      success: true
    }
  ],
  '3': [
    {
      timestamp: '2025-06-15T21:00:00.000Z',
      type: 'full',
      sizeBytes: 52428800, // 50MB
      success: true
    },
    {
      timestamp: '2025-06-15T22:00:00.000Z',
      type: 'incremental',
      sizeBytes: 2621440, // 2.5MB
      success: true
    },
    {
      timestamp: '2025-06-15T23:00:00.000Z',
      type: 'incremental',
      sizeBytes: 1310720, // 1.25MB
      success: true
    }
  ]
};
