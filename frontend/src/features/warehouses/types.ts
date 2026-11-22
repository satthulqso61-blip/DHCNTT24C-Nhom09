export interface Warehouse {
  id: string;
  code: string;
  name: string;
  address: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateWarehouseRequest {
  code: string;
  name: string;
  address: string;
  isActive: boolean;
}

export interface UpdateWarehouseRequest {
  name: string;
  address: string;
  isActive: boolean;
}

export interface PagedWarehouses {
  items: Warehouse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
