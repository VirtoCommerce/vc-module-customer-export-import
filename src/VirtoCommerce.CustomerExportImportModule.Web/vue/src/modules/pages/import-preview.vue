<template>
  <VcBlade
    :title="$t('CUSTOMER_EXPORT_IMPORT.PREVIEW.TITLE')"
    :subtitle="$t('CUSTOMER_EXPORT_IMPORT.PREVIEW.SUBTITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <div class="cei-preview">
      <div class="cei-preview__head">
        <span v-if="!isMobile">{{ $t("CUSTOMER_EXPORT_IMPORT.PREVIEW.LEGEND") }}</span>
        <span class="cei-preview__count">
          {{ $t("CUSTOMER_EXPORT_IMPORT.PREVIEW.COUNT", { count: n(rows.length), total: n(totalCount) }) }}
        </span>
      </div>
      <VcBanner
        v-if="unparsed"
        variant="warning"
      >
        {{ $t("CUSTOMER_EXPORT_IMPORT.PREVIEW.UNPARSED", { count: n(unparsed), preview: n(previewCount) }) }}
      </VcBanner>
      <!-- many columns: the table gets a minimum width and scrolls sideways (legacy ui-grid did the same) -->
      <div class="cei-preview__scroll">
        <VcDataTable
          class="cei-preview__table"
          :style="isMobile ? undefined : { minWidth: `${columns.length * 170 + 50}px` }"
          :items="rows"
          data-key="_row"
          :total-count="rows.length"
          :empty-state="{ icon: 'lucide-file-spreadsheet', title: $t('CUSTOMER_EXPORT_IMPORT.PREVIEW.EMPTY') }"
        >
          <VcColumn
            v-for="(column, i) in columns"
            :id="column"
            :key="column"
            :title="readable(column)"
            :type="column === 'birthday' ? 'date' : undefined"
            :width="i === 0 ? '220px' : '170px'"
            :always-visible="i === 0"
            :mobile-role="i === 0 ? 'title' : i < 5 ? 'field' : undefined"
          />
        </VcDataTable>
      </div>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, useResponsive, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { customerImport, useErrorPopup, type DataType } from "../api";
import type { Member } from "../toolbar";

// The first lines of the uploaded CSV as the importer reads them (legacy importPreviewController); Import starts the job.
// Not routable: the file arrives only through options, which a page reload doesn't restore
defineBlade({
  url: "/customer-import-preview",
  name: "CustomerImportPreview",
  permissions: ["customer:simpleImport"],
  routable: false,
});

const PREVIEW_SIZE = 10; // the server reads 10 lines
const { t, n } = useI18n({ useScope: "global" });
const { options, openBlade, closeSelf } = useBlade<{
  organization?: Member;
  filePath: string;
  dataType: DataType;
  reload?: () => Promise<void>;
}>();
const { showConfirmation } = usePopup();
const { isMobile } = useResponsive();

const rows = ref<Record<string, unknown>[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const { busy, report } = useErrorPopup(loading);
const previewCount = computed(() => Math.min(totalCount.value, PREVIEW_SIZE));
const unparsed = computed(() => Math.max(0, previewCount.value - rows.value.length));

// the record's own fields, then its dynamic properties; an organization's parent columns are hidden as in legacy,
// account passwords too (legacy showed them in clear)
const HIDDEN = ["_row", "password", "dynamicProperties", "parentOrganizationName", "parentOrganizationId", "parentOrganizationOuterId"];
const NAME_FIRST: Record<DataType, string> = { Contact: "contactFullName", Organization: "organizationName" };
// columns no previewed line fills are left out (a CSV rarely uses all ~40 of them)
const filled = (k: string) => rows.value.some((r) => r[k] !== null && r[k] !== undefined && r[k] !== "");
const columns = computed(() => {
  const keys = Object.keys(rows.value[0] ?? {}).filter((k) => !HIDDEN.includes(k) && filled(k));
  const first = options.value && NAME_FIRST[options.value.dataType];
  return first && keys.includes(first) ? [first, ...keys.filter((k) => k !== first)] : keys;
});
// "contactFirstName" -> "Contact First Name" (ui-grid's column naming)
const readable = (key: string) => key.replace(/([a-z0-9])([A-Z])/g, "$1 $2").replace(/^./, (c) => c.toUpperCase());

type DynamicProperty = { name?: string; values?: { value?: unknown }[] };
const flatten = (record: Record<string, unknown>, row: number) => ({
  ...record,
  ...Object.fromEntries(
    ((record.dynamicProperties as DynamicProperty[] | undefined) ?? []).map((p) => [
      p.name,
      (p.values ?? []).map((v) => v.value).join(", "),
    ]),
  ),
  _row: row, // the table's row key: CSV ids may repeat or be empty
});

onMounted(() => {
  const o = options.value;
  if (!o) return;
  busy(async () => {
    const r = await customerImport.preview(o.dataType, o.filePath);
    rows.value = (r.results ?? []).map(flatten);
    totalCount.value = r.totalCount ?? 0;
  });
});

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "import",
    title: t("CUSTOMER_EXPORT_IMPORT.TOOLBAR.IMPORT"),
    icon: "lucide-upload",
    permissions: "customer:simpleImport",
    disabled: !totalCount.value,
    async clickHandler() {
      const o = options.value;
      if (!o) return;
      const target = o.organization
        ? t("CUSTOMER_EXPORT_IMPORT.PREVIEW.CURRENT_ORGANIZATION", { name: o.organization.name })
        : t("CUSTOMER_EXPORT_IMPORT.PREVIEW.ROOT");
      const key = o.dataType === "Contact" ? "CONFIRM_CONTACTS" : "CONFIRM_ORGANIZATIONS";
      if (!(await showConfirmation(t(`CUSTOMER_EXPORT_IMPORT.PREVIEW.${key}`, { count: totalCount.value, target })))) return;
      try {
        const notification = await customerImport.run({ filePath: o.filePath, memberType: o.dataType, organizationId: o.organization?.id });
        await openBlade({ name: "CustomerImportProcessing", options: { notification, reload: o.reload } });
      } catch (e) {
        report(e);
      }
    },
  },
  {
    id: "close",
    title: t("CUSTOMER_EXPORT_IMPORT.TOOLBAR.CLOSE"),
    icon: "lucide-x",
    clickHandler: () => closeSelf(),
  },
]);
</script>

<style lang="scss">
.cei-preview {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  &__head {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
    gap: 8px 16px;
    padding: 12px 16px;
    font-size: 13px;
    color: var(--color-neutral-600);
    border-bottom: 1px solid var(--color-neutral-200);
  }
  &__count {
    font-weight: 500;
    color: var(--color-neutral-900);
  }
  &__scroll {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    overflow-x: auto;
  }
  &__table {
    flex: 1;
    min-height: 0;
  }
  .vc-banner {
    margin: 12px 16px 0;
  }
}
</style>
