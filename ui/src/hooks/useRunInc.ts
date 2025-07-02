// src/hooks/useRunInc.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { backupsApi } from '../api/backups';

export function useRunInc(profileId: string) {
  const qc = useQueryClient();
  return useMutation(() => backupsApi.runInc(profileId), {
    onSuccess: () => qc.invalidateQueries(['backups', profileId]),
  });
}
