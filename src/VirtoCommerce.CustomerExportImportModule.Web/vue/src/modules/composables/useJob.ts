import { computed, getCurrentScope, onScopeDispose, ref } from "vue";
import { JobsClient, PushNotificationClient, useApiClient, useNotificationStore, type PushNotification } from "@vc-shell/framework";

export type JobNotification = PushNotification & {
  jobId?: string;
  finished?: Date | string | null;
  totalCount?: number;
  processedCount?: number;
  errorCount?: number;
  errors?: string[];
};

// A notification's Finished may be DateTime.MinValue (price import declares it non-nullable): not finished.
const done = (x?: JobNotification) => !!x?.finished && new Date(x.finished).getUTCFullYear() > 1;

/**
 * Copied from the Export module's Vue plugin (separate repo, no shared package).
 * Follows a background job through its push notification, the way the AngularJS blades did with
 * "new-notification-event": live updates come from the host's SignalR notification store; a 2s poll of
 * the notification API covers messages sent before we knew the id and types the store doesn't ingest.
 * The platform job API is polled too: a job that dies before it reports (state Failed/Deleted) would
 * otherwise leave the notification "running" forever.
 */
export function useJob<T extends JobNotification = JobNotification>() {
  const store = useNotificationStore();
  const { getApiClient } = useApiClient(PushNotificationClient);
  const { getApiClient: getJobsClient } = useApiClient(JobsClient);
  const started = ref<T>();
  const polled = ref<T>();
  const deadState = ref<string>();
  let timer: ReturnType<typeof setTimeout> | undefined;
  let runs = 0;

  const live = computed(() => store.history.value.find((x) => x.id && x.id === started.value?.id) as T | undefined);

  // the most advanced snapshot wins: finished first, then the most processed
  const job = computed<T | undefined>(() => {
    const candidates = [live.value, polled.value, started.value].filter(Boolean) as T[];
    const best = candidates.find(done) ?? [...candidates].sort((a, b) => (b.processedCount ?? 0) - (a.processedCount ?? 0))[0];
    if (!best || done(best) || !deadState.value) return best;
    const error = `Background job #${best.jobId}: ${deadState.value}`;
    return { ...best, finished: new Date(), description: error, errors: [...(best.errors ?? []), error], errorCount: (best.errorCount ?? 0) + 1 };
  });
  const finished = computed(() => done(job.value));
  const percent = computed(() =>
    job.value?.totalCount ? Math.min(100, Math.round(((job.value.processedCount ?? 0) / job.value.totalCount) * 100)) : 0,
  );

  function stop() {
    clearTimeout(timer);
    timer = undefined;
  }

  async function poll() {
    if (!started.value?.id || finished.value) return stop();
    try {
      const client = await getApiClient();
      const result = await client.searchPushNotification({ ids: [started.value.id], take: 1 });
      if (result.notifyEvents?.[0]) polled.value = result.notifyEvents[0] as T;
      const jobId = job.value?.jobId;
      if (jobId && !done(job.value)) {
        const state = (await (await getJobsClient()).getStatus(jobId)).state;
        if (state === "Failed" || state === "Deleted") deadState.value = state;
      }
    } catch {
      // transient: the next tick retries
    }
    if (finished.value) stop();
  }

  function follow(notification: T) {
    stop();
    polled.value = undefined;
    deadState.value = undefined;
    started.value = notification;
    // the next poll is armed when this one is done, so a slow server never gets overlapping polls
    const run = ++runs;
    const next = () => {
      timer = setTimeout(async () => {
        await poll();
        if (timer && run === runs) next();
      }, 2000);
    };
    next();
  }

  if (getCurrentScope()) onScopeDispose(stop);

  return { job, finished, percent, follow };
}
