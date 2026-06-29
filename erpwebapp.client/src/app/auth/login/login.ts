import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { ActivatedRoute, Router } from "@angular/router";
import { Auth } from "../auth";
import { LoginModel } from "../interfaces/login-model";
import { MatSnackBar } from "@angular/material/snack-bar";
import { ToastService } from "../../shared/services/toast.service";
import { environment } from "../../../environments/environment";

declare const google: any;

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit, AfterViewInit {
  @ViewChild('googleButton', { static: false }) googleButton?: ElementRef<HTMLDivElement>;
  loginForm: FormGroup;
  pointerX = 50;
  pointerY = 50;
  pointerShiftX = 0;
  pointerShiftY = 0;
  pointerShiftInverseX = 0;
  pointerShiftInverseY = 0;

  user = {
  email: 'test1@gmail.com',
  password: 'test@123',
  name: 'Test 01',
  phone: '6541236578',
  isLogedIn:true
};
  constructor(private router: Router, private route: ActivatedRoute, private fb: FormBuilder,private authService:Auth,private snackBar: MatSnackBar, private toastService: ToastService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

ngOnInit(): void {
  const emailConfirmed = this.route.snapshot.queryParamMap.get('emailConfirmed');

  if (emailConfirmed === 'true') {
    this.toastService.success('Email confirmed successfully. Please login.');
  }

  if (emailConfirmed === 'false') {
    this.toastService.error('Email confirmation link is invalid or expired.');
  }

  if (this.authService.hasValidAccessToken()) {
    this.router.navigate(['/home']);
  }
}

ngAfterViewInit(): void {
  this.loadGoogleSignIn();
}

updateBackgroundPointer(event: MouseEvent): void {
  const bounds = (event.currentTarget as HTMLElement).getBoundingClientRect();
  this.pointerX = Math.round(((event.clientX - bounds.left) / bounds.width) * 100);
  this.pointerY = Math.round(((event.clientY - bounds.top) / bounds.height) * 100);
  this.pointerShiftX = Math.round((this.pointerX - 50) * 0.35);
  this.pointerShiftY = Math.round((this.pointerY - 50) * 0.35);
  this.pointerShiftInverseX = this.pointerShiftX * -1;
  this.pointerShiftInverseY = this.pointerShiftY * -1;
}

login() {
  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }
   const request: LoginModel = {
    email: this.loginForm.get('email')?.value,
    password: this.loginForm.get('password')?.value
  };
   this.authService.login(request).subscribe({
  next: (response) => {
    if(response.success){
      this.authService.storeTokens(response.data);
    this.snackBar.open('Login successful ✅', 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
});
    this.toastService.success('Login successful');
    this.router.navigate(['/home']); 
    }
    else 
    {
      this.snackBar.open('Invalid email or password ❌', 'Close', {
      duration: 3000,
      panelClass: ['error-snackbar']
    });
    }
  },
  error: (err) => {
    this.snackBar.open(err.error.errorMessage+'❌'
, 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
    console.error('Login failed', err);
  }
});
  // debugger;
  // const { email, password } = this.loginForm.value;
  // debugger;
  // const userData = localStorage.getItem('user');
  // if(userData == null)
  //   localStorage.setItem('user', JSON.stringify(this.user));
  // else{
  //    var updatedUser = JSON.parse(userData);
  //     updatedUser.isLogedIn = true;
  //     localStorage.setItem('user',JSON.stringify(updatedUser));
  //   this.router.navigate(['/home']);
  // }
}

private loadGoogleSignIn(): void {
  if (!environment.googleClientId || !this.googleButton?.nativeElement) {
    return;
  }

  if (typeof google !== 'undefined') {
    this.renderGoogleButton();
    return;
  }

  const existingScript = document.querySelector<HTMLScriptElement>('script[src="https://accounts.google.com/gsi/client"]');
  if (existingScript) {
    existingScript.addEventListener('load', () => this.renderGoogleButton());
    return;
  }

  const script = document.createElement('script');
  script.src = 'https://accounts.google.com/gsi/client';
  script.async = true;
  script.defer = true;
  script.onload = () => this.renderGoogleButton();
  document.head.appendChild(script);
}

private renderGoogleButton(): void {
  if (!this.googleButton?.nativeElement || typeof google === 'undefined') {
    return;
  }

  google.accounts.id.initialize({
    client_id: environment.googleClientId,
    callback: (response: { credential: string }) => this.handleGoogleCredential(response.credential),
  });

  google.accounts.id.renderButton(this.googleButton.nativeElement, {
    theme: 'outline',
    size: 'large',
    width: 340,
    text: 'signin_with',
  });
}

private handleGoogleCredential(idToken: string): void {
  this.authService.googleLogin({ idToken }).subscribe({
    next: (response) => {
      if (response.success && response.data) {
        this.authService.storeTokens(response.data);
        this.toastService.success('Google login successful');
        this.router.navigate(['/home']);
      } else {
        this.toastService.error(response.errorMessage || 'Google login failed');
      }
    },
    error: (error) => {
      this.toastService.error(error?.error?.message || error?.error?.errorMessage || 'Google login failed');
    },
  });
}

}
