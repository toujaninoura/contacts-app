import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { AuthService } from './auth.service';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-token',
    email: 'test@test.com',
    firstName: 'Test',
    lastName: 'User',
    expiresAt: new Date(Date.now() + 3600000).toISOString()
  };

  const mockApiResponse: ApiResponse<AuthResponse> = {
    success: true,
    data: mockAuthResponse,
    message: null,
    errors: null,
    timestamp: new Date().toISOString()
  };

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should call login endpoint and store token', () => {
      const request: LoginRequest = { email: 'test@test.com', password: 'password' };

      service.login(request).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(localStorage.getItem('token')).toBe('mock-token');
      });

      const req = httpMock.expectOne('https://localhost:7000/api/v1/auth/login');
      expect(req.request.method).toBe('POST');
      req.flush(mockApiResponse);
    });
  });

  describe('register', () => {
    it('should call register endpoint and store token', () => {
      const request: RegisterRequest = {
        firstName: 'Test',
        lastName: 'User',
        email: 'test@test.com',
        password: 'password'
      };

      service.register(request).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(localStorage.getItem('token')).toBe('mock-token');
      });

      const req = httpMock.expectOne('https://localhost:7000/api/v1/auth/register');
      expect(req.request.method).toBe('POST');
      req.flush(mockApiResponse);
    });
  });

  describe('logout', () => {
    it('should clear localStorage and redirect to login', () => {
      localStorage.setItem('token', 'mock-token');
      localStorage.setItem('user', JSON.stringify(mockAuthResponse));

      service.logout();

      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
  });

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem('token', 'mock-token');
      expect(service.getToken()).toBe('mock-token');
    });

    it('should return null when no token', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists and not expired', () => {
      localStorage.setItem('token', 'mock-token');
      localStorage.setItem('user', JSON.stringify(mockAuthResponse));
      expect(service.isAuthenticated()).toBeTrue();
    });

    it('should return false when no token', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('should return false when token is expired', () => {
      const expiredUser: AuthResponse = {
        ...mockAuthResponse,
        expiresAt: new Date(Date.now() - 3600000).toISOString()
      };
      localStorage.setItem('token', 'mock-token');
      localStorage.setItem('user', JSON.stringify(expiredUser));
      expect(service.isAuthenticated()).toBeFalse();
    });
  });

  describe('getCurrentUser', () => {
    it('should return user from localStorage', () => {
      localStorage.setItem('user', JSON.stringify(mockAuthResponse));
      const user = service.getCurrentUser();
      expect(user).toEqual(mockAuthResponse);
    });

    it('should return null when no user in localStorage', () => {
      expect(service.getCurrentUser()).toBeNull();
    });
  });
});
