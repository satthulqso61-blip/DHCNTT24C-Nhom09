import React, { createContext, useContext, useState } from 'react';
import { UserRole, type User, type LoginRequest } from '../types';
import { authApi } from '../api/authApi';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  canEdit: boolean;
  isAdmin: boolean;
  isStaff: boolean;
  isAccountant: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const savedUser = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');
    if (savedUser && token) {
      try {
        return JSON.parse(savedUser);
      } catch {
        localStorage.clear();
      }
    }
    return null;
  });
  const [isLoading] = useState<boolean>(false);

  const login = async (credentials: LoginRequest) => {
    const res = await authApi.login(credentials);
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    localStorage.setItem('user', JSON.stringify(res.user));
    setUser(res.user);
  };

  const logout = () => {
    authApi.logout();
    setUser(null);
    window.location.href = '/login';
  };

  const canEdit = user?.role === UserRole.Admin || user?.role === UserRole.WarehouseStaff;
  const isAdmin = user?.role === UserRole.Admin;
  const isStaff = user?.role === UserRole.WarehouseStaff;
  const isAccountant = user?.role === UserRole.Accountant;

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        canEdit,
        isAdmin,
        isStaff,
        isAccountant,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
