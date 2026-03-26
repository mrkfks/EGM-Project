import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
// Reactive forms & HTTP
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login implements OnInit, OnDestroy {
  form!: FormGroup;
  loading   = false;
  showPwd   = false;
  errorMsg  = '';
  currentYear = new Date().getFullYear();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) document.body.classList.add('login-page');
    this.form = this.fb.group({
      sicil:    ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  ngOnDestroy(): void {
    if (isPlatformBrowser(this.platformId)) document.body.classList.remove('login-page');
  }

  get sicilCtrl()    { return this.form.get('sicil')!; }
  get passwordCtrl() { return this.form.get('password')!; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading  = true;
    this.errorMsg = '';

    this.http.post<{ token: string }>(`${environment.apiUrl}/api/auth/login`, {
      sicil:    this.sicilCtrl.value,
      password: this.passwordCtrl.value
    }).subscribe({
      next: res => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: err => {
        this.loading  = false;
        this.errorMsg = err.status === 401
          ? 'Geçersiz sicil numarası veya parola.'
          : 'Sunucuya bağlanılamadı. Lütfen tekrar deneyin.';
      }
    });
  }
}
