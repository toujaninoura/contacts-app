import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { ContactService } from './contact.service';
import { Contact, CreateContactRequest, UpdateContactRequest } from '../models/contact.model';
import { ApiResponse, PagedResponse } from '../models/api-response.model';

describe('ContactService', () => {
  let service: ContactService;
  let httpMock: HttpTestingController;

  const mockContact: Contact = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@test.com',
    phone: '0600000000',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  };

  const mockApiResponse: ApiResponse<Contact> = {
    success: true,
    data: mockContact,
    message: null,
    errors: null,
    timestamp: new Date().toISOString()
  };

  const mockPagedResponse: ApiResponse<PagedResponse<Contact>> = {
    success: true,
    data: {
      data: [mockContact],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
      hasNext: false,
      hasPrev: false
    },
    message: null,
    errors: null,
    timestamp: new Date().toISOString()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ContactService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ContactService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should call contacts endpoint with pagination params', () => {
      service.getAll(1, 10).subscribe(response => {
        expect(response.data.length).toBe(1);
        expect(response.totalCount).toBe(1);
      });

      const req = httpMock.expectOne(r =>
        r.url === 'https://localhost:7000/api/v1/contacts' &&
        r.params.get('page') === '1' &&
        r.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResponse);
    });

    it('should include search param when provided', () => {
      service.getAll(1, 10, 'John').subscribe(response => {
        expect(response.data.length).toBe(1);
      });

      const req = httpMock.expectOne(r =>
        r.url === 'https://localhost:7000/api/v1/contacts' &&
        r.params.get('search') === 'John'
      );
      expect(req.request.params.get('search')).toBe('John');
      req.flush(mockPagedResponse);
    });
  });

  describe('getById', () => {
    it('should call contact by id endpoint', () => {
      service.getById(1).subscribe(contact => {
        expect(contact).toEqual(mockContact);
      });

      const req = httpMock.expectOne('https://localhost:7000/api/v1/contacts/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockApiResponse);
    });
  });

  describe('create', () => {
    it('should call POST contacts endpoint', () => {
      const createRequest: CreateContactRequest = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        phone: '0600000000'
      };

      service.create(createRequest).subscribe(contact => {
        expect(contact).toEqual(mockContact);
      });

      const req = httpMock.expectOne('https://localhost:7000/api/v1/contacts');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createRequest);
      req.flush(mockApiResponse);
    });
  });

  describe('update', () => {
    it('should call PUT contacts endpoint', () => {
      const updateRequest: UpdateContactRequest = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com'
      };

      service.update(1, updateRequest).subscribe(contact => {
        expect(contact).toEqual(mockContact);
      });

      const req = httpMock.expectOne('https://localhost:7000/api/v1/contacts/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateRequest);
      req.flush(mockApiResponse);
    });
  });

  describe('delete', () => {
    it('should call DELETE contacts endpoint', () => {
      service.delete(1).subscribe();

      const req = httpMock.expectOne('https://localhost:7000/api/v1/contacts/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
