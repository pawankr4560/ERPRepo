export interface MenuItem {
  id: number;
  title: string;
  iconClass: string;
  route?: string | null;
  orderNumber: number;
  isActive: boolean;
  children: MenuItem[];
}
