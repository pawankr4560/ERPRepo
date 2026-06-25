import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PermissionComponent } from './permissioncomponent/permissioncomponent';

const routes: Routes = [{ path: '', component: PermissionComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), PermissionComponent],
})
export class PermissionModule {}
