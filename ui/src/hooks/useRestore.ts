// src/hooks/useRestore.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { backupsApi } from '../api/backups';

export function useRestore(profileId: string) {
  const qc = useQueryClient();
  return useMutation((sourceFile: string) =>
    backupsApi.restore(profileId, sourceFile), {
      onSuccess: () => qc.invalidateQueries(['backups', profileId]),
    }
  );
}
