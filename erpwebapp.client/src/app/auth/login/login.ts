import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { Router } from "@angular/router";
import { Auth } from "../auth";
import { LoginModel } from "../interfaces/login-model";
import { MatSnackBar } from "@angular/material/snack-bar";
import { ToastService } from "../../shared/services/toast.service";

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
export class Login {
  loginForm: FormGroup;

  user = {
  email: 'test1@gmail.com',
  password: 'test@123',
  name: 'Test 01',
  phone: '6541236578',
  isLogedIn:true
};
  constructor(private router: Router, private fb: FormBuilder,private authService:Auth,private snackBar: MatSnackBar, private toastService: ToastService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
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
      const token = response.data;
      localStorage.setItem("auth_token", token);
      localStorage.setItem("jwt", token);
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

}
