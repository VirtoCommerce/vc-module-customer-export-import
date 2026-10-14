// The module's API (api/customer/export, api/customer/import) and the platform calls its blades need,
// as small same-origin fetches (cookie auth, like the generated clients).
import { usePopup } from "@vc-shell/framework";
import type { Ref } from "vue";
import type { JobNotification } from "./composables/useJob";

export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: { Accept: "application/json", ...(body !== undefined && { "Content-Type": "application/json" }) },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status} ${text}`.trim());
  return (text ? JSON.parse(text) : undefined) as T;
}

export type DataType = "Contact" | "Organization";

export interface ValidationError {
  errorCode: string;
  properties?: Record<string, string>;
}

export interface ImportNotification extends JobNotification {
  createdCount?: number;
  updatedCount?: number;
  reportUrl?: string;
}

export interface ExportNotification extends JobNotification {
  contactsFileUrl?: string;
  organizationsFileUrl?: string;
}

/** A positive integer platform setting (legacy settings.getValues). */
export const numberSetting = (name: string, fallback: number) =>
  http<unknown[]>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`).then(
    (v) => Number(v?.[0]) || fallback,
    () => fallback,
  );

/** Uploads a file to the asset store's tmp folder (as the AngularJS uploader did); returns its relative url. */
export async function uploadTmp(file: File): Promise<string> {
  const body = new FormData();
  body.append("file", file);
  const res = await fetch("/api/assets?folderUrl=tmp", { method: "POST", headers: { Accept: "application/json" }, body });
  if (!res.ok) throw new Error(`${file.name}: ${res.status} ${await res.text()}`.trim());
  const [asset] = (await res.json()) as { relativeUrl?: string; url?: string }[];
  return asset.relativeUrl ?? asset.url!;
}

export const deleteAsset = (url: string) => http("DELETE", `/api/assets?urls=${encodeURIComponent(url)}`);

export const membersCount = (criteria: Record<string, unknown>) =>
  http<{ totalCount?: number }>("POST", "/api/members/search", { ...criteria, take: 0 }).then((r) => r.totalCount ?? 0);

export const customerImport = {
  validate: (dataType: DataType, filePath: string) =>
    http<{ errors?: ValidationError[] }>("POST", "/api/customer/import/validate", { dataType, filePath }).then((r) => r.errors ?? []),
  preview: (dataType: DataType, filePath: string) =>
    http<{ totalCount?: number; results?: Record<string, unknown>[] }>("POST", "/api/customer/import/preview", { dataType, filePath }),
  run: (request: { filePath: string; memberType: DataType; organizationId?: string }) =>
    http<ImportNotification>("POST", "/api/customer/import/run", request),
  cancel: (jobId: string) => http("POST", "/api/customer/import/cancel", { jobId }),
};

export const customerExport = {
  run: (request: { keyword?: string; objectIds: string[]; organizationId?: string }) =>
    http<ExportNotification>("POST", "/api/customer/export/run", request),
  cancel: (jobId: string) => http("POST", "/api/customer/export/cancel", { jobId }),
};

/** A blade's error popup for failed requests; `busy` also holds `loading` on while the action runs. */
export function useErrorPopup(loading?: Ref<boolean>) {
  const { showError } = usePopup();
  const report = (e: unknown) => showError(String((e as Error)?.message ?? e));
  async function busy(action: () => Promise<unknown>) {
    if (loading) loading.value = true;
    try {
      await action();
    } catch (e) {
      report(e);
    } finally {
      if (loading) loading.value = false;
    }
  }
  return { report, busy };
}

/** Opens a file of the asset store (relative or absolute url) in a download. */
export function download(url: string) {
  const link = document.createElement("a");
  link.href = new URL(url, `${location.origin}/`).href;
  link.download = "";
  document.body.appendChild(link);
  link.click();
  link.remove();
}

export const fileName = (url?: string) => url?.split(/[\\/]/).pop() ?? "";
