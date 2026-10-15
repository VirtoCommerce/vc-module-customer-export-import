// The Customer members list's toolbar extension point "customer-members-toolbar" (contract documented in the
// Customer plugin, vc-module-customer .../vue/src/modules/pages/members-list.vue). Types repeated here: the contract is by shape.

export interface Member {
  id?: string;
  name?: string;
  memberType?: string;
}

export interface CustomerMembersToolbarContext {
  organization?: Member;
  selection: Member[];
  keyword?: string;
  totalCount: number;
  openBlade: (event: { name: string; param?: string; options?: Record<string, unknown> }) => Promise<void>;
  reload: () => Promise<void>;
}

export interface CustomerMembersToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: CustomerMembersToolbarContext) => boolean;
  disabled?: (ctx: CustomerMembersToolbarContext) => boolean;
  onClick: (ctx: CustomerMembersToolbarContext) => unknown;
}
