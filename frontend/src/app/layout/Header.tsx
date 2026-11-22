import React from 'react';
import { Menu, ShieldCheck } from 'lucide-react';
import { useAuth } from '../../features/auth/context/AuthContext';
import { UserRole } from '../../features/auth/types';

interface HeaderProps {
  onToggleSidebar: () => void;
  title?: string;
}

export const Header: React.FC<HeaderProps> = ({ onToggleSidebar, title = 'Bảng điều khiển' }) => {
  const { user } = useAuth();

  const getRoleBadge = (role?: UserRole) => {
    switch (role) {
      case UserRole.Admin:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200">
            <ShieldCheck className="w-3.5 h-3.5 text-purple-600" />
            Admin
          </span>
        );
      case UserRole.WarehouseStaff:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
            Thủ kho
          </span>
        );
      case UserRole.Accountant:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
            Kế toán (Chỉ xem)
          </span>
        );
      default:
        return null;
    }
  };

  return (
    <header className="h-16 bg-white border-b border-slate-200 sticky top-0 z-30 flex items-center justify-between px-4 sm:px-8">
      <div className="flex items-center gap-3">
        <button
          onClick={onToggleSidebar}
          className="p-2 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 lg:hidden cursor-pointer"
        >
          <Menu className="w-5 h-5" />
        </button>
        <h2 className="text-base sm:text-lg font-bold text-slate-800">{title}</h2>
      </div>

      <div className="flex items-center gap-3">
        {getRoleBadge(user?.role)}
        <div className="hidden sm:flex flex-col text-right">
          <span className="text-xs font-bold text-slate-900">{user?.fullName || user?.username}</span>
          <span className="text-[11px] text-slate-500">{user?.email}</span>
        </div>
      </div>
    </header>
  );
};
