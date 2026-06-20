export interface MenuPermission {
  id: number;
  name: string;
  level: number; // 0 = Parent, 1 = Child
  parentId?: number;
  permissions: {
    view: boolean;
    create: boolean;
    edit: boolean;
    submit: boolean;
    approve: boolean;
    amend: boolean;
    post: boolean;
    print: boolean;
    delete: boolean;
  };
}
