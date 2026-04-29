import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { Contact, CreateContactRequest, UpdateContactRequest } from '../models/contact.model';
import { ApiResponse, PagedResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/contacts`;

  getAll(page: number, pageSize: number, search?: string): Observable<PagedResponse<Contact>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim().length > 0) {
      params = params.set('search', search.trim());
    }

    return this.http.get<ApiResponse<PagedResponse<Contact>>>(this.apiUrl, { params }).pipe(
      map(response => response.data)
    );
  }

  getById(id: number): Observable<Contact> {
    return this.http.get<ApiResponse<Contact>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data)
    );
  }

  create(request: CreateContactRequest): Observable<Contact> {
    return this.http.post<ApiResponse<Contact>>(this.apiUrl, request).pipe(
      map(response => response.data)
    );
  }

  update(id: number, request: UpdateContactRequest): Observable<Contact> {
    return this.http.put<ApiResponse<Contact>>(`${this.apiUrl}/${id}`, request).pipe(
      map(response => response.data)
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
