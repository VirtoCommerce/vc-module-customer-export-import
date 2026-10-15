import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, useExtensionPoint } from "@vc-shell/framework";
import { markRaw } from "vue";
import type { CustomerMembersToolbarCommand } from "./toolbar";

// Import / Export on the Contacts list toolbar (legacy: toolbarService on memberListController)
const none = markRaw({ render: () => null });
const toolbar = useExtensionPoint("customer-members-toolbar");
toolbar.add({
  id: "customer-csv-import",
  priority: 10,
  component: none,
  meta: {
    title: "CUSTOMER_EXPORT_IMPORT.TOOLBAR.IMPORT",
    icon: "lucide-upload",
    permissions: "customer:simpleImport",
    onClick: (ctx) =>
      ctx.openBlade({ name: "CustomerImportUpload", options: { organization: ctx.organization, reload: ctx.reload } }),
  } satisfies CustomerMembersToolbarCommand,
});
toolbar.add({
  id: "customer-csv-export",
  priority: 20,
  component: none,
  meta: {
    title: "CUSTOMER_EXPORT_IMPORT.TOOLBAR.EXPORT",
    icon: "lucide-download",
    permissions: "customer:simpleExport",
    onClick: (ctx) =>
      ctx.openBlade({
        name: "CustomerExport",
        options: {
          organization: ctx.organization,
          // the export takes contacts and organizations only
          objectIds: ctx.selection.filter((m) => m.memberType === "Contact" || m.memberType === "Organization").map((m) => m.id),
          keyword: ctx.keyword,
        },
      }),
  } satisfies CustomerMembersToolbarCommand,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
