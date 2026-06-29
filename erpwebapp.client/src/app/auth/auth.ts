import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AddressApiResponse, ApiAuthResponse, AuthenticatedResponse, GoogleLoginRequest, LoginModel, RefreshTokenRequest, SignupModel } from './interfaces/login-model';
import { jwtDecode } from 'jwt-decode';
import { ApiService } from '../shared/services/api.service';

@Injectable({
  providedIn: 'root',
})
export class Auth {
constructor(private api: ApiService) {}

  login(credentials: LoginModel): Observable<any> {
    return this.api.post<ApiAuthResponse>('Auth/login', credentials);
  }

  googleLogin(request: GoogleLoginRequest): Observable<ApiAuthResponse> {
    return this.api.post<ApiAuthResponse>('Auth/GoogleLogin', request);
  }

  refreshToken(request?: RefreshTokenRequest): Observable<ApiAuthResponse> {
    const payload = request ?? {
      accessToken: localStorage.getItem('auth_token') ?? localStorage.getItem('jwt') ?? '',
      refreshToken: localStorage.getItem('refresh_token') ?? '',
    };
    return this.api.post<ApiAuthResponse>('Auth/RefreshToken', payload);
  }

  storeTokens(tokens: AuthenticatedResponse): void {
    localStorage.setItem('auth_token', tokens.accessToken);
    localStorage.setItem('jwt', tokens.accessToken);
    localStorage.setItem('refresh_token', tokens.refreshToken);
    localStorage.setItem('access_token_expires_at', tokens.accessTokenExpiresAtUtc);
    localStorage.setItem('refresh_token_expires_at', tokens.refreshTokenExpiresAtUtc);
  }

  clearSession(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('jwt');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('access_token_expires_at');
    localStorage.removeItem('refresh_token_expires_at');
    localStorage.removeItem('user');
  }

  hasValidAccessToken(): boolean {
    const token = localStorage.getItem('auth_token') ?? localStorage.getItem('jwt');
    if (!token) {
      return false;
    }

    try {
      const decoded: any = jwtDecode(token);
      return !!decoded.exp && decoded.exp > Date.now() / 1000;
    } catch {
      return false;
    }
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



