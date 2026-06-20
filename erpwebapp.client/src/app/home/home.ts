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

const ROUTE_ROLE_MAP: Record<string, string[]> = {
  'users': ['admin', 'user'],
  'dashboard': ['admin'],
  'settings': ['admin'],
  'permission': ['admin'],
  'inventory/product': ['admin'],
  'inventory/item': ['admin'],
  'inventory/transactions': ['admin'],
  'inventory/payments': ['admin'],
  'inventory/emi': ['admin'],
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
  isMobile = false;
  private destroyed$ = new Subject<void>();

  constructor(
    private breakpointObserver: BreakpointObserver,
    private menuService: Menu,
    private authService: Auth,
    private router: Router
  ) {}

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

  isParentActive(menu: MenuItem): boolean {
    return menu.children.some(
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

      this.menuService.initmenus().subscribe(res => {
        this.menus = res
          .map((parent: any) => {
            const filteredChildren = (parent.children || [])
              .filter((child: any) => this.hasAccess(child.route, userRole))
              .sort((a: any, b: any) => (a.orderNumber || 0) - (b.orderNumber || 0));
            return {
              ...parent,
              children: filteredChildren
            };
          })
          .filter((parent: any) => {
            const isRouteAllowed = this.hasAccess(parent.route, userRole);
            const hasChildren = parent.children && parent.children.length > 0;
            const hadNoChildrenInitially = !parent.children || parent.children.length === 0;
            return isRouteAllowed && (hasChildren || hadNoChildrenInitially);
          })
          .sort((a: any, b: any) => (a.orderNumber || 0) - (b.orderNumber || 0));
      });
  }

  isChildActive(menu: any): boolean {
    return menu.children?.some((child: any) =>
      this.router.url.startsWith(this.getMenuPath(child))
    );
  }

  navigate(route?: string) {
    if (route) {
      this.router.navigateByUrl(this.getMenuPath({ route } as MenuItem));
    }
  }

  ngOnDestroy() {
    this.destroyed$.next();
    this.destroyed$.complete();
  }
}
