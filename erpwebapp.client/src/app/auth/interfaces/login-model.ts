export interface LoginModel {
      email: string;
  password: string;
}


export interface AuthenticatedResponse{
  token: string;
  errorMessage:string;
}

export interface SignupModel {
    firstName : string;
    lastName : string;
    email : string;
    password : string;
    confirmPassword : string;
    phone:number,
    weight:number,
    height:number,
    calorie:number,
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