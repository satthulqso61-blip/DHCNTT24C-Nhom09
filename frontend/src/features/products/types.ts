export interface Product {
  id: string;
  sku: string;
  name: string;
  unit: string;
  basePrice: number;
  isDeleted: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  unit: string;
  basePrice: number;
}

export interface UpdateProductRequest {
  name: string;
  unit: string;
  basePrice: number;
}

export interface PagedProducts {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
