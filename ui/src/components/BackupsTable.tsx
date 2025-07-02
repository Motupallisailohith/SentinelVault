import React, { useState } from 'react';
import { useBackups } from '../hooks/useBackups';
import { useRunFull } from '../hooks/useRunFull';
import { useRunInc }  from '../hooks/useRunInc';
import { useRestore } from '../hooks/useRestore';
import { backupsApi } from '../api/backups';
import ReactMarkdown from 'react-markdown';

interface BackupRow {
  fileName: string;
  utcTimestamp: string;
  type: 'Full' | 'Incremental';
  sizeBytes: number;
}

export default function BackupsTable({ profileId }: { profileId: string }) {
  const { data: backups, isLoading } = useBackups(profileId);
  const runFull = useRunFull(profileId);
  const runInc  = useRunInc(profileId);
  const restore = useRestore(profileId);
  const [aiExplanation, setAiExplanation] = useState<string | null>(null);

  if (isLoading) {
    return <div className="text-center py-8">Loading backups…</div>;
  }

  return (
    <div className="space-y-4">
      {/* Action Buttons */}
      <div className="flex gap-2">
        <button
          onClick={() => runFull.mutate()}
          disabled={runFull.isLoading}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {runFull.isLoading ? 'Running Full…' : 'Run Full'}
        </button>
        <button
          onClick={() => runInc.mutate()}
          disabled={runInc.isLoading || !runInc.mutate}
          className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
        >
          {runInc.isLoading ? 'Running Incremental…' : 'Run Incremental'}
        </button>
      </div>

      {/* Backups Table */}
      <table className="min-w-full bg-white border border-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="p-2 text-left text-sm font-medium text-gray-600">Date</th>
            <th className="p-2 text-left text-sm font-medium text-gray-600">Type</th>
            <th className="p-2 text-left text-sm font-medium text-gray-600">Size (KB)</th>
            <th className="p-2 text-left text-sm font-medium text-gray-600">Actions</th>
          </tr>
        </thead>
        <tbody>
          {backups?.map((b: BackupRow) => (
            <tr key={b.fileName} className="hover:bg-gray-50">
              <td className="p-2">{new Date(b.utcTimestamp).toLocaleString()}</td>
              <td className="p-2">{b.type}</td>
              <td className="p-2">{(b.sizeBytes / 1024).toFixed(1)}</td>
              <td className="p-2 space-x-2">
                <a
                  href={`${import.meta.env.VITE_API_BASE_URL}/backups/${b.fileName}`}
                  download
                  className="text-blue-600 hover:underline"
                >
                  Download
                </a>
                <button
                  onClick={async () => {
                    const { data } = await backupsApi.explain(b.fileName);
                    setAiExplanation(data.explanation);
                  }}
                  className="text-purple-600 hover:underline"
                >
                  Explain
                </button>
                <button
                  onClick={() => restore.mutate(b.fileName)}
                  disabled={restore.isLoading}
                  className="text-red-600 hover:underline disabled:opacity-50"
                >
                  {restore.isLoading ? 'Restoring…' : 'Restore'}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* AI Explanation Drawer */}
      {aiExplanation && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white max-w-lg w-full p-6 rounded-lg shadow-lg">
            <div className="flex justify-end">
              <button
                onClick={() => setAiExplanation(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                ✕
              </button>
            </div>
            <ReactMarkdown className="prose">{aiExplanation}</ReactMarkdown>
          </div>
        </div>
      )}
    </div>
  );
}
