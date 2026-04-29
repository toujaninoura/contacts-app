export interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContactRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
}

export interface UpdateContactRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
}
