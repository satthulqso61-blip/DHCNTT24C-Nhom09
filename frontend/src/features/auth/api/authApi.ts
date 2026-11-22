import { http } from '../../../lib/http';
import type { AuthResponse, LoginRequest, User } from '../types';

export const authApi = {
  login: async (request: LoginRequest): Promise<AuthResponse> => {
    const response = await http.post<AuthResponse>('/auth/login', request);
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const response = await http.get<User>('/auth/me');
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },
};
