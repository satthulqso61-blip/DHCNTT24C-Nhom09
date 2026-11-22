import { http } from '../../../lib/http';
import type { Product, CreateProductRequest, UpdateProductRequest, PagedProducts } from '../types';

export const productsApi = {
  getProducts: async (
    search?: string,
    page: number = 1,
    pageSize: number = 10,
    includeDeleted: boolean = false
  ): Promise<PagedProducts> => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    params.append('page', String(page));
    params.append('pageSize', String(pageSize));
    params.append('includeDeleted', String(includeDeleted));

    const response = await http.get<PagedProducts>(`/products?${params.toString()}`);
    return response.data;
  },

  getProductById: async (id: string): Promise<Product> => {
    const response = await http.get<Product>(`/products/${id}`);
    return response.data;
  },

  createProduct: async (data: CreateProductRequest): Promise<Product> => {
    const response = await http.post<Product>('/products', data);
    return response.data;
  },

  updateProduct: async (id: string, data: UpdateProductRequest): Promise<Product> => {
    const response = await http.put<Product>(`/products/${id}`, data);
    return response.data;
  },

  deleteProduct: async (id: string): Promise<void> => {
    await http.delete(`/products/${id}`);
  },

  restoreProduct: async (id: string): Promise<Product> => {
    const response = await http.post<Product>(`/products/${id}/restore`);
    return response.data;
  },
};
