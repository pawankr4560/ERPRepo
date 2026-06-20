import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { AddressApiResponse, AuthenticatedResponse, LoginModel, SignupModel } from './interfaces/login-model';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  get apiUrl(): string {
  return environment.apiUrl;
}
get apiKey(): string {
  return environment.apiKey;
}

private headers!: HttpHeaders;

constructor(private http: HttpClient) {
  this.headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    'api_key': this.apiKey, 
  });
}

  login(credentials: LoginModel): Observable<any> {
    return this.http.post<AuthenticatedResponse>(`${this.apiUrl}/api/Auth/login`, credentials,{headers:this.headers});
  }

  signup(request: SignupModel): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/api/Auth/signup`, request,{headers:this.headers});
  }

  searchAddress(address: string) {
    return this.http.get<AddressApiResponse>(
      `${this.apiUrl}/api/Auth/GetAddress`,
      { params: { address } }
    );
  }

   getRole() {
   var validToken = localStorage.getItem("jwt");
   if (validToken == null)
      {
     return false;
   }
   const decodeToken: any = jwtDecode(validToken);
   return decodeToken["Role"].toLowerCase();
 }
}



