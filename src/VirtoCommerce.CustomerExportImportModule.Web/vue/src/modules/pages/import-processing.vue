<template>
  <VcBlade
    :title="$t('CUSTOMER_EXPORT_IMPORT.PROCESSING.TITLE')"
    :toolbar-items="toolbar"
    :loading="!job"
    width="50%"
  >
    <VcContainer>
      <JobProgress
        v-if="job"
        :job="job"
        :finished="finished"
        :percent="percent"
        prefix="CUSTOMER_EXPORT_IMPORT"
      >
        <template #facts>
          <dt>{{ $t("CUSTOMER_EXPORT_IMPORT.PROCESSING.CREATED") }}</dt>
          <dd>{{ job.createdCount ?? 0 }}</dd>
          <dt>{{ $t("CUSTOMER_EXPORT_IMPORT.PROCESSING.UPDATED") }}</dt>
          <dd>{{ job.updatedCount ?? 0 }}</dd>
        </template>
        <div v-if="job.reportUrl">
          <VcButton
            icon="lucide-download"
            :title="fileName(job.reportUrl)"
            @click="download(job.reportUrl)"
          >
            {{ $t("CUSTOMER_EXPORT_IMPORT.PROCESSING.REPORT") }}
          </VcButton>
        </div>
      </JobProgress>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcContainer } from "@vc-shell/framework/ui";
import { computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import { customerImport, deleteAsset, download, fileName, useErrorPopup, type ImportNotification } from "../api";
import JobProgress from "../components/JobProgress.vue";
import { useJob } from "../composables/useJob";

// Progress of the import job (legacy importProcessingController); the error report is deleted on close, as in legacy.
// Not routable: the job arrives only through options, which a page reload doesn't restore
defineBlade({
  url: "/customer-import-progress",
  name: "CustomerImportProcessing",
  permissions: ["customer:simpleImport"],
  routable: false,
});

const { t } = useI18n({ useScope: "global" });
const { options, onBeforeClose } = useBlade<{ notification: ImportNotification; reload?: () => Promise<void> }>();
const { report } = useErrorPopup();
const { job, finished, percent, follow } = useJob<ImportNotification>();
if (options.value?.notification) follow(options.value.notification);

// the members list shows the imported members once the job is done
watch(finished, (done) => done && options.value?.reload?.());

onBeforeClose(async () => {
  if (job.value?.reportUrl) await deleteAsset(job.value.reportUrl).catch(() => undefined);
  return false;
});

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "cancel",
    title: t("CUSTOMER_EXPORT_IMPORT.TOOLBAR.CANCEL"),
    icon: "lucide-circle-x",
    disabled: finished.value || !job.value?.jobId,
    async clickHandler() {
      await customerImport.cancel(job.value!.jobId!).catch(report);
    },
  },
]);
</script>
