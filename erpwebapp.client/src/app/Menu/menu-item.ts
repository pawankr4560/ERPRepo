export interface MenuItem {
  id: number;
  parentId?: number | null;
  title: string;
  iconClass: string;
  route?: string | null;
  orderNumber: number;
  isActive: boolean;
  children: MenuItem[];
}
