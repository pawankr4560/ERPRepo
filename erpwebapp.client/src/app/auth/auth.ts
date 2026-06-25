import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AddressApiResponse, AuthenticatedResponse, LoginModel, SignupModel } from './interfaces/login-model';
import { jwtDecode } from 'jwt-decode';
import { ApiService } from '../shared/services/api.service';

@Injectable({
  providedIn: 'root',
})
export class Auth {
constructor(private api: ApiService) {}

  login(credentials: LoginModel): Observable<any> {
    return this.api.post<AuthenticatedResponse>('Auth/login', credentials);
  }

  signup(request: SignupModel): Observable<any> {
    return this.api.post<any>('Auth/signup', request);
  }

  searchAddress(address: string) {
    return this.api.get<AddressApiResponse>(
      `Auth/GetAddress?address=${encodeURIComponent(address)}`
    );
  }

   getRole() {
   var validToken = localStorage.getItem("auth_token") ?? localStorage.getItem("jwt");
   if (validToken == null)
      {
     return false;
   }
   const decodeToken: any = jwtDecode(validToken);
   const role =
     decodeToken["role"] ??
     decodeToken["Role"] ??
     decodeToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
   return typeof role === 'string' ? role.toLowerCase() : false;
 }
}



