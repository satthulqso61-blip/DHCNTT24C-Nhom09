import React, { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';

export const MainLayout: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();

  const getPageTitle = (path: string): string => {
    if (path.startsWith('/warehouses')) return 'Quản lý Kho hàng';
    if (path.startsWith('/products')) return 'Danh mục Hàng hóa (Sản phẩm)';
    if (path.startsWith('/dashboard')) return 'Tổng quan hệ thống';
    return 'Hệ thống Quản lý kho StockHub';
  };

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Sidebar */}
      <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 lg:pl-72">
        <Header onToggleSidebar={() => setSidebarOpen(true)} title={getPageTitle(location.pathname)} />

        <main className="flex-1 p-4 sm:p-8 max-w-7xl w-full mx-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
