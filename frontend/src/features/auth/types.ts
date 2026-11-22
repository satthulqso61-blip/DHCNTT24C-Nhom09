export const UserRole = {
  Admin: 1,
  WarehouseStaff: 2,
  Accountant: 3,
} as const;

export type UserRole = (typeof UserRole)[keyof typeof UserRole];

export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: UserRole;
  isActive: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
  user: User;
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export const getRoleLabel = (role: UserRole): string => {
  switch (role) {
    case UserRole.Admin:
      return 'Quản trị viên (Admin)';
    case UserRole.WarehouseStaff:
      return 'Thủ kho (Warehouse Staff)';
    case UserRole.Accountant:
      return 'Kế toán (Accountant)';
    default:
      return 'Người dùng';
  }
};
