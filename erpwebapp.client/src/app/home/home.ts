import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { Menu } from '../Menu/menu';
import { MenuItem } from '../Menu/menu-item';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTab, MatTabGroup } from '@angular/material/tabs';
import { MatCard, MatCardContent } from '@angular/material/card';
import { jwtDecode } from 'jwt-decode';
import { Auth } from '../auth/auth';

interface JwtPayload {
  FirstName: string;
  LastName: string;
  Email: string;
  Phone: string;
  Exp?: number;
}

interface SidebarSection {
  title: string;
  items: MenuItem[];
}

const ROUTE_ROLE_MAP: Record<string, string[]> = {
  'users': ['admin', 'user'],
  'dashboard': ['admin'],
  'settings': ['admin'],
  'menu': ['admin'],
  'permission': ['admin'],
  'inventory/product': ['admin'],
  'inventory/item': ['admin'],
  'inventory/transactions': ['admin'],
  'inventory/payments': ['admin'],
  'inventory/emi': ['admin'],
  'pay-emi': ['admin', 'user'],
  'booking/cars': ['admin'],
  'booking/items': ['admin'],
  'booking/list': ['user'],
  'booking/payments': ['admin'],
  'dynamic': ['admin']
};

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    RouterModule,
    MatTab,
    MatTabGroup,
    MatCardContent,
    MatCard,MatExpansionModule,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatExpansionModule,
    MatListModule,
  MatIconModule,
  MatExpansionModule,
  RouterModule
  ],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnDestroy,OnInit {
  name :string ='';
  email:string='';
  phone:string='';
  menus: MenuItem[] = [];
  sidebarSections: SidebarSection[] = [];
  isLoadingMenus = false;
  isMobile = false;
  private destroyed$ = new Subject<void>();

  constructor(
    private breakpointObserver: BreakpointObserver,
    private menuService: Menu,
    private authService: Auth,
    private router: Router
  ) {}

  getInitials(): string {
    if (!this.name) return 'AK';
    const parts = this.name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + (parts[parts.length - 1][0] || '')).toUpperCase();
  }

  getMenuPath(menu: MenuItem): string {
    if (!menu.route) {
      return '/home';
    }

    if (menu.route.startsWith('/')) {
      return menu.route;
    }

    if (menu.route.startsWith('home/')) {
      return `/${menu.route}`;
    }

    return `/home/${menu.route}`;
  }

  getMenuIcon(menu: MenuItem): string {
    const icon = menu.iconClass?.trim();
    const normalizedIcon = icon?.toLowerCase();
    if (normalizedIcon === 'fa fa-money' || normalizedIcon === 'fa-money') {
      return 'payments';
    }

    if (icon && !icon.includes(' ')) {
      return icon;
    }

    const title = menu.title?.toLowerCase() ?? '';
    if (title.includes('home')) return 'home';
    if (title.includes('configuration')) return 'tune';
    if (title.includes('dashboard')) return 'space_dashboard';
    if (title.includes('loan')) return 'credit_card';
    if (title.includes('calculator') || title.includes('emi')) return 'calculate';
    if (title.includes('statement') || title.includes('payment')) return 'receipt_long';
    if (title.includes('profile') || title.includes('user')) return 'person';
    if (title.includes('setting')) return 'settings';
    if (title.includes('support')) return 'support_agent';
    if (title.includes('menu')) return 'menu_open';
    if (title.includes('permission')) return 'admin_panel_settings';
    if (title.includes('booking')) return 'event_available';
    if (title.includes('product')) return 'inventory_2';
    if (title.includes('item')) return 'category';
    return 'radio_button_unchecked';
  }

  isParentActive(menu: MenuItem): boolean {
    return (menu.children ?? []).some(
      child =>
        this.router.url.startsWith(this.getMenuPath(child)) ||
        this.isParentActive(child)
    );
  }

  redirectPage() {
    this.router.navigate(['/home/product']);
  }

  logOut() {
    const user = localStorage.getItem('user');
    if (user != null) {
      var updatedUser = JSON.parse(user);
      updatedUser.isLogedIn = false;
      localStorage.setItem('user',JSON.stringify(updatedUser));
      this.router.navigate(['auth']);
    }
    else this.router.navigate(['auth']);
  }

  hasAccess(route: string | null | undefined, role: string): boolean {
    if (!route) return true;

    let cleanRoute = route.trim().replace(/^\/+/g, '').replace(/\/+$/g, '');
    if (cleanRoute.startsWith('home/')) {
      cleanRoute = cleanRoute.substring(5);
    }

    for (const key of Object.keys(ROUTE_ROLE_MAP)) {
      if (cleanRoute === key || cleanRoute.startsWith(key + '/')) {
        const allowedRoles = ROUTE_ROLE_MAP[key];
        return allowedRoles.map(r => r.toLowerCase()).includes(role.toLowerCase());
      }
    }

    return true;
  }

  ngOnInit() {
    this.breakpointObserver
      .observe([Breakpoints.Handset])
      .pipe(takeUntil(this.destroyed$))
      .subscribe(r => (this.isMobile = r.matches));

      //get data from localstorage
      const token = localStorage.getItem('jwt');

      if (token) {
        const decodedToken = jwtDecode<JwtPayload>(token);

        this.name = `${decodedToken.FirstName} ${decodedToken.LastName}`;
        this.email = decodedToken.Email;
        this.phone = decodedToken.Phone;
      }

      const userRole = this.authService.getRole() || 'user';

      // Redirect if path is exactly '/home' or '/home/' to the first allowed landing page
      if (this.router.url === '/home' || this.router.url === '/home/') {
        if (userRole.toLowerCase() === 'admin') {
          this.router.navigate(['/home/dashboard']);
        } else {
          this.router.navigate(['/home/users']);
        }
      }

      this.isLoadingMenus = true;
      this.menuService.initmenus()
        .pipe(takeUntil(this.destroyed$))
        .subscribe({
          next: (res) => {
            this.menus = this.prepareMenus(res ?? [], userRole);
            this.sidebarSections = this.buildSidebarSections(this.menus);
            if (!this.sidebarSections.length) {
              this.sidebarSections = this.buildSidebarSections(this.getFallbackMenus(userRole));
            }
            this.isLoadingMenus = false;
          },
          error: () => {
            this.menus = this.getFallbackMenus(userRole);
            this.sidebarSections = this.buildSidebarSections(this.menus);
            this.isLoadingMenus = false;
          },
        });
  }

  isChildActive(menu: any): boolean {
    return menu.children?.some((child: any) =>
      this.router.url.startsWith(this.getMenuPath(child))
    );
  }

  hasChildren(menu: MenuItem): boolean {
    return (menu.children ?? []).length > 0;
  }

  navigate(route?: string) {
    if (route) {
      this.router.navigateByUrl(this.getMenuPath({ route } as MenuItem));
    }
  }

  closeMobileMenu(drawer: { close: () => void }, event: MouseEvent): void {
    event.stopPropagation();
    if (this.isMobile) {
      drawer.close();
    }
  }

  ngOnDestroy() {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  private prepareMenus(items: any[], role: string): MenuItem[] {
    const normalized = (items ?? [])
      .map((item) => this.normalizeMenu(item))
      .filter((item) => item.isActive !== false);

    const tree = normalized.some((item) => item.children?.length)
      ? normalized
      : this.buildMenuTree(normalized);

    return this.filterMenusByRole(
      this.groupStandaloneMenuItems(this.normalizeSidebarGroups(tree)),
      role
    );
  }

  private normalizeMenu(item: any): MenuItem {
    const activeValue = item.isActive ?? item.IsActive ?? item.active ?? item.Active ?? item.F_Active ?? true;

    const route = item.route ?? item.Route ?? item.F_route ?? item.F_Route ?? item.url ?? item.Url ?? null;
    const title = item.title ?? item.Title ?? item.T_Title ?? item.menuName ?? item.MenuName ?? '';

    return {
      id: item.id ?? item.Id ?? item.menuId ?? item.F_menu_Index ?? item.F_Menu_Index ?? 0,
      parentId: item.parentId ?? item.ParentId ?? item.F_Parent_menu_index ?? item.F_Parent_Menu_Index ?? null,
      title: this.getSidebarDisplayTitle(title, route),
      iconClass: item.iconClass ?? item.IconClass ?? item.F_icon_class ?? item.F_Icon_Class ?? item.icon ?? item.Icon ?? '',
      route,
      orderNumber: item.orderNumber ?? item.OrderNumber ?? item.F_order_number ?? item.F_Order_Number ?? item.displayOrder ?? item.DisplayOrder ?? 0,
      isActive: activeValue === true || activeValue === 1 || activeValue === '1' || String(activeValue).toLowerCase() === 'true',
      children: (item.children ?? item.Children ?? []).map((child: any) => this.normalizeMenu(child)),
    };
  }

  private buildMenuTree(items: MenuItem[]): MenuItem[] {
    const map = new Map<number, MenuItem>();
    items.forEach((item) => map.set(item.id, { ...item, children: [] }));

    const roots: MenuItem[] = [];
    map.forEach((item) => {
      const parent = item.parentId ? map.get(item.parentId) : undefined;
      if (parent) {
        parent.children.push(item);
      } else {
        roots.push(item);
      }
    });

    return this.sortMenuTree(roots);
  }

  private normalizeSidebarGroups(items: MenuItem[]): MenuItem[] {
    return this.sortMenuTree(
      items.map((item) => {
        const children = this.normalizeSidebarGroups(item.children ?? []);
        return {
          ...item,
          title: this.getSidebarDisplayTitle(item.title, item.route, children),
          children,
        };
      })
    );
  }

  private groupStandaloneMenuItems(items: MenuItem[]): MenuItem[] {
    const roots: MenuItem[] = [];
    const groupMap = new Map<string, MenuItem>();

    const groupTitles: Record<string, string> = {
      home: 'Home',
      loan: 'Loan',
      booking: 'Booking',
      configuration: 'Configuration',
    };

    const getGroup = (key: keyof typeof groupTitles, orderNumber: number, iconClass: string): MenuItem => {
      const existing = groupMap.get(key);
      if (existing) {
        return existing;
      }

      const group: MenuItem = {
        id: -1 * (groupMap.size + 1),
        title: groupTitles[key],
        iconClass,
        route: null,
        orderNumber,
        isActive: true,
        children: [],
      };
      groupMap.set(key, group);
      roots.push(group);
      return group;
    };

    const attachToGroup = (item: MenuItem): boolean => {
      const route = this.cleanRoute(item.route);
      if (route === 'dashboard') {
        getGroup('home', 1, 'home').children.push({ ...item, parentId: -1 });
        return true;
      }

      if (['inventory/transactions', 'inventory/payments', 'inventory/emi'].includes(route)) {
        getGroup('loan', 2, 'credit_card').children.push({ ...item, parentId: -2 });
        return true;
      }

      if (['booking/cars', 'booking/items', 'booking/list', 'booking/payments'].includes(route)) {
        getGroup('booking', 3, 'event_available').children.push({ ...item, parentId: -3 });
        return true;
      }

      if (route === 'settings') {
        getGroup('configuration', 4, 'tune').children.push({ ...item, parentId: -4 });
        return true;
      }

      return false;
    };

    items.forEach((item) => {
      const title = item.title.toLowerCase();
      if (!item.route && ['home', 'loan', 'booking', 'configuration'].includes(title)) {
        const key = title as keyof typeof groupTitles;
        const group = getGroup(
          key,
          item.orderNumber || (key === 'home' ? 1 : key === 'loan' ? 2 : key === 'booking' ? 3 : 4),
          item.iconClass
        );
        group.id = item.id;
        group.parentId = item.parentId;
        group.iconClass = item.iconClass || group.iconClass;
        group.children.push(...(item.children ?? []));
        return;
      }

      if (!attachToGroup(item)) {
        roots.push(item);
      }
    });

    return this.sortMenuTree(roots.filter((item) => !!item.route || item.children.length > 0));
  }

  private sortMenuTree(items: MenuItem[]): MenuItem[] {
    return [...items]
      .map((item) => ({ ...item, children: this.sortMenuTree(item.children ?? []) }))
      .sort((a, b) => (a.orderNumber || 0) - (b.orderNumber || 0));
  }

  private getSidebarDisplayTitle(title: string, route?: string | null, children: MenuItem[] = []): string {
    const cleanTitle = (title || '').trim();
    const normalizedTitle = cleanTitle.toLowerCase();
    const cleanRoute = this.cleanRoute(route);

    if (cleanRoute === 'dashboard') {
      return 'Dashboard';
    }

    if (cleanRoute === 'inventory/transactions') {
      return 'My Loans';
    }

    if (cleanRoute === 'inventory/payments') {
      return 'Statements';
    }

    if (cleanRoute === 'inventory/emi') {
      return 'Calculator';
    }

    if (cleanRoute === 'booking/cars') {
      return 'Cars';
    }

    if (cleanRoute === 'booking/items') {
      return 'Items';
    }

    if (cleanRoute === 'booking/list') {
      return 'Bookings';
    }

    if (cleanRoute === 'booking/payments') {
      return 'Booking Payments';
    }

    if (!cleanRoute && (normalizedTitle === 'customer' || normalizedTitle === 'loans')) {
      return 'Loan';
    }

    if (!cleanRoute && normalizedTitle === 'booking') {
      return 'Booking';
    }

    if (!cleanRoute && (normalizedTitle === 'system' || normalizedTitle === 'settings')) {
      return 'Configuration';
    }

    if (!cleanRoute && !cleanTitle && children.length) {
      return 'Menu';
    }

    return cleanTitle;
  }

  private cleanRoute(route?: string | null): string {
    let cleanRoute = route?.trim().replace(/^\/+/g, '').replace(/\/+$/g, '') ?? '';
    if (cleanRoute.startsWith('home/')) {
      cleanRoute = cleanRoute.substring(5);
    }
    return cleanRoute;
  }

  private filterMenusByRole(items: MenuItem[], role: string): MenuItem[] {
    return items
      .map((item) => {
        const children = this.filterMenusByRole(item.children ?? [], role)
          .sort((a, b) => (a.orderNumber || 0) - (b.orderNumber || 0));
        return { ...item, children };
      })
      .filter((item) => {
        const allowed = this.hasAccess(item.route, role);
        return allowed && (!!item.route || item.children.length > 0);
      })
      .sort((a, b) => (a.orderNumber || 0) - (b.orderNumber || 0));
  }

  private buildSidebarSections(menus: MenuItem[]): SidebarSection[] {
    return menus.length ? [{ title: 'Main', items: menus }] : [];
  }

  private getFallbackMenus(role: string): MenuItem[] {
    const fallback: MenuItem[] = [
      {
        id: 1,
        title: 'Home',
        iconClass: 'home',
        route: null,
        orderNumber: 1,
        isActive: true,
        children: [
          { id: 2, parentId: 1, title: 'Dashboard', iconClass: 'space_dashboard', route: 'dashboard', orderNumber: 1, isActive: true, children: [] },
        ],
      },
      {
        id: 3,
        title: 'Loan',
        iconClass: 'credit_card',
        route: null,
        orderNumber: 2,
        isActive: true,
        children: [
          { id: 4, parentId: 3, title: 'My Loans', iconClass: 'credit_card', route: 'inventory/transactions', orderNumber: 1, isActive: true, children: [] },
          { id: 5, parentId: 3, title: 'Statements', iconClass: 'receipt_long', route: 'inventory/payments', orderNumber: 2, isActive: true, children: [] },
          { id: 6, parentId: 3, title: 'Calculator', iconClass: 'calculate', route: 'inventory/emi', orderNumber: 3, isActive: true, children: [] },
        ],
      },
      {
        id: 7,
        title: 'Booking',
        iconClass: 'event_available',
        route: null,
        orderNumber: 3,
        isActive: true,
        children: [
          { id: 8, parentId: 7, title: 'Cars', iconClass: 'directions_car', route: 'booking/cars', orderNumber: 1, isActive: true, children: [] },
          { id: 9, parentId: 7, title: 'Items', iconClass: 'category', route: 'booking/items', orderNumber: 2, isActive: true, children: [] },
          { id: 10, parentId: 7, title: 'Bookings', iconClass: 'event_available', route: 'booking/list', orderNumber: 3, isActive: true, children: [] },
        ],
      },
      {
        id: 11,
        title: 'Configuration',
        iconClass: 'tune',
        route: null,
        orderNumber: 4,
        isActive: true,
        children: [
          { id: 12, parentId: 11, title: 'Settings', iconClass: 'settings', route: 'settings', orderNumber: 1, isActive: true, children: [] },
        ],
      },
    ];

    return this.filterMenusByRole(this.normalizeSidebarGroups(fallback), role);
  }
}
