// src/hooks/useRunFull.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { backupsApi } from '../api/backups';

export function useRunFull(profileId: string) {
  const qc = useQueryClient();
  return useMutation(() => backupsApi.runFull(profileId), {
    onSuccess: () => qc.invalidateQueries(['backups', profileId]),
  });
}
