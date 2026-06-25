import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Login } from './login/login';
import { Signup } from './signup/signup';

const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Signup },
  { path: 'signup', component: Signup },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forChild(routes), Login, Signup],
})
export class AuthModule {}
