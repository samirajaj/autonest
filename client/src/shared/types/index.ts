export type Role = "Customer" | "Company" | "Admin";

export interface Session {
  token: string;
  expiresAt: string;
  role: Role;
  displayName: string;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Car {
  id: number;
  companyId: number;
  company: string;
  make: string;
  model: string;
  year: number;
  type: string;
  gearType: string;
  fuelType: string;
  seatsCount: number;
  mileage: number;
  price: number;
  isAvailable: boolean;
  isForSale: boolean;
  inRent: boolean;
  rating: number;
  imageUrl?: string;
  imageUrls?: string[];
  listingDate?: string;
}

export interface RequestItem {
  id: number;
  carId: number;
  car: string;
  company: string;
  type: "Rent" | "Sale";
  state: string;
  requestDate: string;
  deadline?: string;
  startDate?: string;
  endDate?: string;
}

export interface Transaction {
  id: number;
  requestId: number;
  car: string;
  paidAmount: number;
  listingDate: string;
  state: string;
  rating?: number;
}

export interface Metric {
  label: string;
  value: number;
  format: string;
}

export interface City {
  id: number;
  name: string;
}

export interface Dashboard {
  metrics: Metric[];
  quarterlyProfits: number[];
}
