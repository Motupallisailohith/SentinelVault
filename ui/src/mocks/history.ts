export interface BackupHistory {
  timestamp: string;
  type: 'full' | 'incremental';
  sizeBytes: number | null;
  success: boolean;
}

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
    },
    {
      timestamp: '2025-06-16T02:00:00.000Z',
      type: 'incremental',
      sizeBytes: 1310720, // 1.25MB
      success: true
    },
    {
      timestamp: '2025-06-16T03:00:00.000Z',
      type: 'incremental',
      sizeBytes: 655360, // 640KB
      success: false
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
    },
    {
      timestamp: '2025-06-16T01:00:00.000Z',
      type: 'incremental',
      sizeBytes: 2621440, // 2.5MB
      success: true
    },
    {
      timestamp: '2025-06-16T02:00:00.000Z',
      type: 'incremental',
      sizeBytes: 1310720, // 1.25MB
      success: false
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

export const mockRestores: Record<string, { timestamp: string; success: boolean; sourceFileName: string }[]> = {
  '1': [
    {
      timestamp: '2025-06-15T23:30:00.000Z',
      success: true,
      sourceFileName: 'dev_db_20250615_230000.sql.zstd'
    },
    {
      timestamp: '2025-06-16T00:30:00.000Z',
      success: true,
      sourceFileName: 'dev_db_20250616_000000.sql.zstd'
    }
  ],
  '2': [
    {
      timestamp: '2025-06-15T22:30:00.000Z',
      success: true,
      sourceFileName: 'prod_db_20250615_220000.sql.zstd'
    },
    {
      timestamp: '2025-06-15T23:30:00.000Z',
      success: true,
      sourceFileName: 'prod_db_20250615_230000.sql.zstd'
    }
  ],
  '3': [
    {
      timestamp: '2025-06-15T21:30:00.000Z',
      success: true,
      sourceFileName: 'staging_db_20250615_210000.sql.zstd'
    },
    {
      timestamp: '2025-06-15T22:30:00.000Z',
      success: true,
      sourceFileName: 'staging_db_20250615_220000.sql.zstd'
    }
  ]
};
