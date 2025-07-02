import { useQuery } from '@tanstack/react-query';
import { backupsApi } from '../api/backups';

export const useBackups = (profileId: string) =>
  useQuery({
    queryKey: ['backups', profileId],
    queryFn: () => backupsApi.list(profileId).then(r => r.data),
    enabled: !!profileId,
    refetchInterval: 30_000, // refresh every 30s
  });
