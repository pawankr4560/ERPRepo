import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { User } from './user/user';

const routes: Routes = [{ path: '', component: User }];

@NgModule({
  imports: [RouterModule.forChild(routes), User],
})
export class UsersModule {}
