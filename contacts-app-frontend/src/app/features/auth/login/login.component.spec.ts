import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/auth.model';
import { Router } from '@angular/router';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-jwt-token',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    expiresAt: new Date(Date.now() + 3600000).toISOString()
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should show email validation error when email is invalid', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('not-an-email');
    emailControl?.markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const emailError = compiled.querySelector('[data-testid="email-error"]');
    expect(emailError?.textContent).toContain('Email invalide');
  });

  it('should show password validation error when password is empty', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('');
    passwordControl?.markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const passwordError = compiled.querySelector('[data-testid="password-error"]');
    expect(passwordError?.textContent).toContain('Mot de passe obligatoire');
  });

  it('should call authService.login when form is valid', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));

    component.loginForm.setValue({ email: 'test@example.com', password: 'Password123!' });
    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'Password123!'
    });
  });

  it('should navigate to /contacts on success', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    const navigateSpy = spyOn(router, 'navigate');

    component.loginForm.setValue({ email: 'test@example.com', password: 'Password123!' });
    component.onSubmit();

    expect(navigateSpy).toHaveBeenCalledWith(['/contacts']);
  });

  it('should show error message on login failure', () => {
    authServiceSpy.login.and.returnValue(throwError(() => ({ status: 401 })));

    component.loginForm.setValue({ email: 'test@example.com', password: 'wrongpassword' });
    component.onSubmit();
    fixture.detectChanges();

    expect(component.errorMessage).toBe('Identifiants incorrects');
  });
});
