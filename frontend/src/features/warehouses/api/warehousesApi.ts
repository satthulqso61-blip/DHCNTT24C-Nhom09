import { http } from '../../../lib/http';
import type { Warehouse, CreateWarehouseRequest, UpdateWarehouseRequest, PagedWarehouses } from '../types';

export const warehousesApi = {
  getWarehouses: async (
    search?: string,
    isActive?: boolean,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedWarehouses> => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (isActive !== undefined) params.append('isActive', String(isActive));
    params.append('page', String(page));
    params.append('pageSize', String(pageSize));

    const response = await http.get<PagedWarehouses>(`/warehouses?${params.toString()}`);
    return response.data;
  },

  getWarehouseById: async (id: string): Promise<Warehouse> => {
    const response = await http.get<Warehouse>(`/warehouses/${id}`);
    return response.data;
  },

  createWarehouse: async (data: CreateWarehouseRequest): Promise<Warehouse> => {
    const response = await http.post<Warehouse>('/warehouses', data);
    return response.data;
  },

  updateWarehouse: async (id: string, data: UpdateWarehouseRequest): Promise<Warehouse> => {
    const response = await http.put<Warehouse>(`/warehouses/${id}`, data);
    return response.data;
  },

  deleteWarehouse: async (id: string): Promise<void> => {
    await http.delete(`/warehouses/${id}`);
  },
};
