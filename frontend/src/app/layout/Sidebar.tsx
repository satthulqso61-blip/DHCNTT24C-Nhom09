import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Warehouse,
  Package,
  FileText,
  Boxes,
  CheckCircle2,
  LogOut,
  User as UserIcon,
} from 'lucide-react';
import { useAuth } from '../../features/auth/context/AuthContext';
import { getRoleLabel } from '../../features/auth/types';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const { user, logout } = useAuth();

  const navItemClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium transition-all ${
      isActive
        ? 'bg-blue-600 text-white shadow-sm shadow-blue-500/20'
        : 'text-slate-400 hover:text-white hover:bg-slate-800/60'
    }`;

  const reservedItemClass =
    'flex items-center justify-between px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-500 hover:text-slate-300 hover:bg-slate-800/40 transition-all cursor-not-allowed opacity-80';

  return (
    <>
      {/* Mobile Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-slate-950/70 backdrop-blur-xs lg:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      <aside
        className={`fixed top-0 bottom-0 left-0 z-50 w-72 bg-slate-900 border-r border-slate-800 flex flex-col transition-transform duration-300 ease-in-out lg:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {/* Brand */}
        <div className="h-16 flex items-center px-6 border-b border-slate-800 gap-3">
          <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center text-white font-black text-xl shadow-md shadow-blue-500/30">
            S
          </div>
          <div>
            <h1 className="text-base font-extrabold text-white tracking-tight flex items-center gap-1.5">
              StockHub
              <span className="text-[10px] font-bold uppercase tracking-wider bg-blue-500/20 text-blue-400 border border-blue-500/30 px-1.5 py-0.5 rounded">
                v1.0
              </span>
            </h1>
            <p className="text-xs text-slate-400">Quản lý kho đa chi nhánh</p>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
          {/* Member 1 Modules */}
          <div>
            <div className="px-3 mb-2 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              Chức năng chính (Thành viên 1)
            </div>
            <div className="space-y-1">
              <NavLink to="/dashboard" className={navItemClass} onClick={onClose}>
                <LayoutDashboard className="w-4 h-4" />
                <span>Tổng quan</span>
              </NavLink>

              <NavLink to="/warehouses" className={navItemClass} onClick={onClose}>
                <Warehouse className="w-4 h-4" />
                <span>Quản lý Kho hàng</span>
              </NavLink>

              <NavLink to="/products" className={navItemClass} onClick={onClose}>
                <Package className="w-4 h-4" />
                <span>Danh mục Hàng hóa</span>
              </NavLink>
            </div>
          </div>

          {/* Member 2 Reserved Modules */}
          <div>
            <div className="px-3 mb-2 flex items-center justify-between text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <span>Nghiệp vụ kho (Thành viên 2)</span>
              <span className="text-[9px] bg-slate-800 text-slate-400 px-1.5 py-0.5 rounded font-normal">Chờ nối tiếp</span>
            </div>
            <div className="space-y-1">
              <div className={reservedItemClass} title="Tính năng của Thành viên 2">
                <div className="flex items-center gap-3">
                  <FileText className="w-4 h-4" />
                  <span>Phiếu Nhập / Xuất kho</span>
                </div>
                <span className="text-[10px] bg-slate-800 text-amber-400 px-1.5 py-0.5 rounded">P3</span>
              </div>

              <div className={reservedItemClass} title="Tính năng của Thành viên 2">
                <div className="flex items-center gap-3">
                  <Boxes className="w-4 h-4" />
                  <span>Tra cứu Tồn kho</span>
                </div>
                <span className="text-[10px] bg-slate-800 text-amber-400 px-1.5 py-0.5 rounded">P3</span>
              </div>

              <div className={reservedItemClass} title="Tính năng của Thành viên 2">
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Duyệt phiếu Kế toán</span>
                </div>
                <span className="text-[10px] bg-slate-800 text-amber-400 px-1.5 py-0.5 rounded">1 lần</span>
              </div>
            </div>
          </div>
        </nav>

        {/* User Card & Logout */}
        <div className="p-4 border-t border-slate-800 bg-slate-950/40">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2.5 overflow-hidden">
              <div className="w-9 h-9 rounded-full bg-slate-800 flex items-center justify-center text-blue-400 font-bold border border-slate-700 shrink-0">
                <UserIcon className="w-4 h-4" />
              </div>
              <div className="overflow-hidden">
                <p className="text-xs font-bold text-white truncate">{user?.fullName || user?.username}</p>
                <p className="text-[11px] text-blue-400 truncate">{user ? getRoleLabel(user.role) : ''}</p>
              </div>
            </div>
          </div>

          <button
            onClick={logout}
            className="w-full flex items-center justify-center gap-2 px-3 py-2 text-xs font-semibold text-rose-400 hover:text-white hover:bg-rose-600/80 rounded-lg transition-colors border border-rose-500/20 cursor-pointer"
          >
            <LogOut className="w-3.5 h-3.5" />
            <span>Đăng xuất</span>
          </button>
        </div>
      </aside>
    </>
  );
};
