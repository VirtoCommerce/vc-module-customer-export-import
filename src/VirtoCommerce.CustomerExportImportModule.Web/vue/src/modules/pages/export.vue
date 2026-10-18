<template>
  <VcBlade
    :title="$t(job ? 'CUSTOMER_EXPORT_IMPORT.PROCESSING.EXPORT_TITLE' : 'CUSTOMER_EXPORT_IMPORT.EXPORT.TITLE')"
    :subtitle="options?.organization?.name"
    :toolbar-items="toolbar"
    :loading="loading"
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
        <div
          v-if="job.contactsFileUrl || job.organizationsFileUrl"
          class="cei-links"
        >
          <VcButton
            v-if="job.contactsFileUrl"
            icon="lucide-download"
            :title="fileName(job.contactsFileUrl)"
            @click="download(job.contactsFileUrl)"
          >
            {{ $t("CUSTOMER_EXPORT_IMPORT.EXPORT.CONTACTS_FILE") }}
          </VcButton>
          <VcButton
            v-if="job.organizationsFileUrl"
            icon="lucide-download"
            :title="fileName(job.organizationsFileUrl)"
            @click="download(job.organizationsFileUrl)"
          >
            {{ $t("CUSTOMER_EXPORT_IMPORT.EXPORT.ORGANIZATIONS_FILE") }}
          </VcButton>
        </div>
      </JobProgress>
      <div
        v-else-if="counts"
        class="cei-page"
      >
        <VcBanner
          v-if="tooMany"
          variant="warning"
        >
          {{ $t("CUSTOMER_EXPORT_IMPORT.EXPORT.TOO_MANY", { count: total, limit: counts.limit }) }}
        </VcBanner>
        <VcBanner
          v-else-if="!total"
          variant="info"
        >
          {{ $t("CUSTOMER_EXPORT_IMPORT.EXPORT.EMPTY") }}
        </VcBanner>
        <VcCard
          v-else
          :header="$t('CUSTOMER_EXPORT_IMPORT.EXPORT.SUMMARY')"
        >
          <div class="cei-card">{{ summary }}</div>
        </VcCard>
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcButton, VcCard, VcContainer } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { customerExport, download, fileName, membersCount, numberSetting, useErrorPopup, type ExportNotification } from "../api";
import JobProgress from "../components/JobProgress.vue";
import { useJob } from "../composables/useJob";
import type { Member } from "../toolbar";

// Export of the Contacts list (legacy: "Export" on memberListController + customerExport dialog + export-processing blade):
// the selected members, or everything in the current organization matching the list's search.
// Not routable: the scope arrives only through options, and a reload without them would export every member
defineBlade({ url: "/customer-export", name: "CustomerExport", permissions: ["customer:simpleExport"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ organization?: Member; objectIds?: string[]; keyword?: string }>();
const { job, finished, percent, follow } = useJob<ExportNotification>();

const loading = ref(false);
const { busy } = useErrorPopup(loading);
const counts = ref<{ contacts: number; organizations: number; limit: number }>();
const total = computed(() => (counts.value ? counts.value.contacts + counts.value.organizations : 0));
const tooMany = computed(() => !!counts.value && total.value > counts.value.limit);
const objectIds = computed(() => options.value?.objectIds ?? []);
const exportAll = computed(() => !objectIds.value.length);

// the export searches by the selection, or by keyword when nothing is selected (ExportDataRequest.ToSearchCriteria)
const request = computed(() => ({
  keyword: exportAll.value ? options.value?.keyword : undefined,
  objectIds: objectIds.value,
  organizationId: options.value?.organization?.id,
}));

const summary = computed(() => {
  const values = { contacts: counts.value?.contacts ?? 0, organizations: counts.value?.organizations ?? 0 };
  if (options.value?.organization)
    return t("CUSTOMER_EXPORT_IMPORT.EXPORT.FROM_ORGANIZATION", { ...values, organization: options.value.organization.name });
  return t(exportAll.value ? "CUSTOMER_EXPORT_IMPORT.EXPORT.ALL" : "CUSTOMER_EXPORT_IMPORT.EXPORT.SELECTED", values);
});

onMounted(() =>
  busy(async () => {
    const criteria = (memberType: string) => ({
      memberType,
      memberId: request.value.organizationId,
      objectIds: request.value.objectIds,
      keyword: request.value.keyword,
      deepSearch: true,
      objectType: "Member",
    });
    const [limit, contacts, organizations] = await Promise.all([
      numberSetting("CustomerExportImport.Export.LimitOfLines", 10000),
      membersCount(criteria("Contact")),
      membersCount(criteria("Organization")),
    ]);
    counts.value = { contacts, organizations, limit };
  }),
);

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "start",
    title: t("CUSTOMER_EXPORT_IMPORT.TOOLBAR.START_EXPORT"),
    icon: "lucide-play",
    isVisible: !job.value,
    disabled: !total.value || tooMany.value,
    clickHandler: () => busy(async () => follow(await customerExport.run(request.value))),
  },
  {
    id: "cancel",
    title: t("CUSTOMER_EXPORT_IMPORT.TOOLBAR.CANCEL"),
    icon: "lucide-circle-x",
    isVisible: !!job.value,
    disabled: finished.value || !job.value?.jobId,
    clickHandler: () => busy(() => customerExport.cancel(job.value!.jobId!)),
  },
]);
</script>

<style lang="scss">
.cei-page {
  padding: 20px 22px 36px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.cei-card {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  font-size: 14px;
  color: var(--color-neutral-900);
}

.cei-links {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
</style>
