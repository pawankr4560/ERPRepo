export interface LoginModel {
      email: string;
  password: string;
}


export interface AuthenticatedResponse{
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
  errorMessage?: string;
}

export interface ApiAuthResponse {
  success: boolean;
  message?: string;
  errorMessage?: string;
  data: AuthenticatedResponse;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface SignupModel {
    firstName : string;
    lastName : string;
    email : string;
    password : string;
    confirmPassword : string;
    phone:number,
    gender : string;
    address : string
}

export interface AddressApiResponse {
  success: boolean;
  message: string ;
  errorMessage: string ;
  data: {
    predictions: AddressPrediction[];
    status: string;
  };
}

export interface AddressPrediction {
  description: string;
  place_id: string;
  structured_formatting: {
    main_text: string;
    secondary_text: string;
  };
}
