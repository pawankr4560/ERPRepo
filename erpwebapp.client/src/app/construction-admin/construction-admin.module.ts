import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConstructionAdminComponent } from './construction-admin';

const routes: Routes = [{ path: '', component: ConstructionAdminComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), ConstructionAdminComponent],
})
export class ConstructionAdminModule {}
