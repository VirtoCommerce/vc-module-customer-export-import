<template>
  <VcBlade
    :title="title"
    :subtitle="$t('CUSTOMER_EXPORT_IMPORT.UPLOAD.SUBTITLE')"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="cei-page">
        <VcSelect
          v-model="dataType"
          :label="$t('CUSTOMER_EXPORT_IMPORT.UPLOAD.DATA_TYPE')"
          :placeholder="$t('CUSTOMER_EXPORT_IMPORT.UPLOAD.DATA_TYPE_PLACEHOLDER')"
          :hint="$t('CUSTOMER_EXPORT_IMPORT.UPLOAD.DATA_TYPE_HINT')"
          :options="dataTypes"
          option-value="id"
          option-label="title"
          :disabled="!!options?.organization || !!file"
          :clearable="false"
          required
        />
        <VcFileUpload
          variant="file-upload"
          accept=".csv"
          :disabled="!dataType || loading"
          :custom-text="{ dragHere: $t('CUSTOMER_EXPORT_IMPORT.UPLOAD.DROP'), browse: $t('CUSTOMER_EXPORT_IMPORT.UPLOAD.BROWSE') }"
          @upload="upload"
        />
        <VcCard
          v-if="result"
          :header="$t(result.failed ? 'CUSTOMER_EXPORT_IMPORT.UPLOAD.FAILED' : 'CUSTOMER_EXPORT_IMPORT.UPLOAD.SUCCESS')"
        >
          <div class="cei-card">
            <div class="cei-file">
              <VcIcon
                :icon="result.failed ? 'lucide-file-x' : errors.length ? 'lucide-file-warning' : 'lucide-file-check'"
                size="xl"
                :variant="result.failed ? 'danger' : errors.length ? 'warning' : 'success'"
                class="cei-file__icon"
              />
              <div class="cei-file__info">
                <div class="cei-file__name">{{ result.name }}</div>
                <div
                  v-if="result.failed || !errors.length"
                  class="cei-file__hint"
                >
                  {{ result.failed ?? result.size }}
                </div>
                <ul
                  v-else
                  class="cei-file__errors"
                >
                  <li
                    v-for="(e, i) in errors"
                    :key="i"
                  >
                    {{ errorText(e) }}
                  </li>
                </ul>
              </div>
            </div>
            <div
              v-if="file"
              class="cei-file__actions"
            >
              <VcButton
                variant="outline"
                icon="lucide-trash-2"
                @click="remove"
              >
                {{ $t("CUSTOMER_EXPORT_IMPORT.UPLOAD.DELETE") }}
              </VcButton>
              <VcButton
                v-if="!errors.length"
                icon="lucide-eye"
                @click="preview"
              >
                {{ $t("CUSTOMER_EXPORT_IMPORT.UPLOAD.PREVIEW") }}
              </VcButton>
            </div>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { readableSize, useBlade, usePopup } from "@vc-shell/framework";
import { VcBlade, VcButton, VcCard, VcContainer, VcFileUpload, VcForm, VcIcon, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { customerImport, deleteAsset, numberSetting, uploadTmp, useErrorPopup, type DataType, type ValidationError } from "../api";
import type { Member } from "../toolbar";

// Upload + validation of the CSV to import into the root or into an organization
// (legacy: "Import" on memberListController + fileUploadController).
// Not routable: the organization arrives only through options, and a reload without them would import into the root
defineBlade({ url: "/customer-import", name: "CustomerImportUpload", permissions: ["customer:simpleImport"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeChildren, onBeforeClose } = useBlade<{ organization?: Member; reload?: () => Promise<void> }>();
const { showConfirmation } = usePopup();

const title = computed(() =>
  options.value?.organization
    ? t("CUSTOMER_EXPORT_IMPORT.UPLOAD.TITLE_MEMBER", { member: options.value.organization.name })
    : t("CUSTOMER_EXPORT_IMPORT.UPLOAD.TITLE_ROOT"),
);
const dataTypes = (["Contact", "Organization"] as DataType[]).map((id) => ({ id, title: t(`CUSTOMER_EXPORT_IMPORT.TYPES.${id}`) }));
// an organization takes contacts only
const dataType = ref<DataType | undefined>(options.value?.organization ? "Contact" : undefined);
const file = ref<string>(); // the uploaded CSV (asset relative url)
const result = ref<{ name: string; size?: string; failed?: string }>();
const errors = ref<ValidationError[]>([]);
const previewOpen = ref(false);
const loading = ref(false);
const { busy } = useErrorPopup(loading);
const maxSize = numberSetting("CustomerExportImport.Import.FileMaxSize", 1).then((mb) => mb * 1024 * 1024);

const errorText = (e: ValidationError) => {
  const key = `CUSTOMER_EXPORT_IMPORT.VALIDATION.${e.errorCode}`;
  const text = t(key, e.properties ?? {});
  return text === key ? e.errorCode : text;
};

async function dropFile() {
  const url = file.value;
  file.value = undefined;
  errors.value = [];
  if (url) await deleteAsset(url);
}

async function upload(files: FileList) {
  const picked = files[0];
  if (!picked) return;
  // replacing a file that is being previewed closes the preview first (legacy deleteAndCloseBlades)
  if (file.value && previewOpen.value) {
    if (!(await showConfirmation(t("CUSTOMER_EXPORT_IMPORT.UPLOAD.REPLACE_CONFIRMATION", { name: result.value?.name ?? "" })))) return;
    await closeChildren();
  }
  await busy(async () => {
    await dropFile();
    const limit = await maxSize;
    if (!/\.csv$/i.test(picked.name)) {
      result.value = { name: picked.name, failed: t("CUSTOMER_EXPORT_IMPORT.UPLOAD.FILE_TYPE_ERROR") };
    } else if (picked.size > limit) {
      result.value = { name: picked.name, failed: t("CUSTOMER_EXPORT_IMPORT.UPLOAD.MAX_SIZE_ERROR", { fileMaxSize: readableSize(limit) }) };
    } else {
      result.value = undefined;
      file.value = await uploadTmp(picked);
      errors.value = await customerImport.validate(dataType.value!, file.value);
      result.value = { name: picked.name, size: readableSize(picked.size) };
    }
  });
}

async function remove() {
  if (!(await showConfirmation(t("CUSTOMER_EXPORT_IMPORT.UPLOAD.DELETE_CONFIRMATION")))) return;
  await closeChildren();
  await busy(dropFile);
  result.value = undefined;
}

function preview() {
  previewOpen.value = true;
  openBlade({
    name: "CustomerImportPreview",
    options: { organization: options.value?.organization, filePath: file.value, dataType: dataType.value, reload: options.value?.reload },
    onClose: () => (previewOpen.value = false),
  });
}

// closing keeps no orphan file: confirm, then delete the uploaded CSV (legacy bladeClose)
onBeforeClose(async () => {
  if (!file.value) return false;
  if (!(await showConfirmation(t("CUSTOMER_EXPORT_IMPORT.UPLOAD.DELETE_CONFIRMATION")))) return true;
  await dropFile().catch(() => undefined);
  return false;
});
</script>

<style lang="scss">
.cei-file {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  &__icon {
    flex-shrink: 0;
  }
  &__info {
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-width: 0;
  }
  &__name {
    font-weight: 500;
    word-break: break-all;
  }
  &__hint {
    color: var(--color-neutral-500);
    font-size: 13px;
  }
  &__errors {
    margin: 0;
    padding-inline-start: 18px;
    color: var(--color-danger-600);
    font-size: 13px;
  }
  &__actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
}
</style>
