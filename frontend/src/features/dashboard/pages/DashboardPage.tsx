import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Warehouse,
  Package,
  FileText,
  Boxes,
  CheckCircle2,
  ArrowRight,
  ShieldAlert,
  Layers,
} from 'lucide-react';
import { useAuth } from '../../auth/context/AuthContext';
import { getRoleLabel } from '../../auth/types';
import { warehousesApi } from '../../warehouses/api/warehousesApi';
import { productsApi } from '../../products/api/productsApi';

export const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  const [warehouseCount, setWarehouseCount] = useState<number | null>(null);
  const [productCount, setProductCount] = useState<number | null>(null);

  useEffect(() => {
    const fetchSummary = async () => {
      try {
        const [wRes, pRes] = await Promise.all([
          warehousesApi.getWarehouses(undefined, undefined, 1, 1),
          productsApi.getProducts(undefined, 1, 1, false),
        ]);
        setWarehouseCount(wRes.totalCount);
        setProductCount(pRes.totalCount);
      } catch (err) {
        console.error('Failed to load dashboard summary:', err);
      }
    };

    fetchSummary();
  }, []);

  return (
    <div className="space-y-8">
      {/* Welcome Banner */}
      <div className="bg-gradient-to-r from-slate-900 via-blue-950 to-slate-900 rounded-3xl p-8 text-white shadow-xl relative overflow-hidden">
        <div className="absolute right-0 top-0 bottom-0 w-96 bg-blue-500/10 blur-3xl pointer-events-none" />
        <div className="relative z-10 max-w-2xl">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-blue-500/20 border border-blue-400/30 text-blue-300 text-xs font-semibold mb-4">
            <span>Đề tài ĐT07 · StockHub Enterprise</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mb-2 text-white">
            Xin chào, {user?.fullName || user?.username}!
          </h1>
          <p className="text-sm text-slate-300 leading-relaxed">
            Hệ thống quản lý chuỗi cung ứng, kho hàng đa chi nhánh và kiểm soát bất biến chứng từ.
            Vai trò hiện tại của bạn:{' '}
            <strong className="text-blue-400">{user ? getRoleLabel(user.role) : ''}</strong>.
          </p>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {/* Card 1: Warehouses */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs hover:border-blue-300 transition-all">
          <div className="flex items-center justify-between mb-4">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
              Tổng số Kho hàng
            </span>
            <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600">
              <Warehouse className="w-5 h-5" />
            </div>
          </div>
          <div className="flex items-baseline justify-between">
            <span className="text-3xl font-black text-slate-900">
              {warehouseCount !== null ? warehouseCount : '...'}
            </span>
            <Link
              to="/warehouses"
              className="text-xs font-semibold text-blue-600 hover:text-blue-700 inline-flex items-center gap-1"
            >
              Xem chi tiết <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
          <p className="text-[11px] text-slate-400 mt-2">Gồm Kho Tổng, Miền Bắc & Miền Nam</p>
        </div>

        {/* Card 2: Products */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs hover:border-emerald-300 transition-all">
          <div className="flex items-center justify-between mb-4">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
              Mặt hàng (SKU)
            </span>
            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center text-emerald-600">
              <Package className="w-5 h-5" />
            </div>
          </div>
          <div className="flex items-baseline justify-between">
            <span className="text-3xl font-black text-slate-900">
              {productCount !== null ? productCount : '...'}
            </span>
            <Link
              to="/products"
              className="text-xs font-semibold text-emerald-600 hover:text-emerald-700 inline-flex items-center gap-1"
            >
              Xem chi tiết <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
          <p className="text-[11px] text-slate-400 mt-2">Hỗ trợ xóa mềm bảo toàn lịch sử</p>
        </div>

        {/* Card 3: Backend Status */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between mb-4">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
              Trạng thái API
            </span>
            <div className="w-10 h-10 rounded-xl bg-purple-50 flex items-center justify-center text-purple-600">
              <Layers className="w-5 h-5" />
            </div>
          </div>
          <div className="flex items-baseline justify-between">
            <span className="text-lg font-bold text-slate-900 flex items-center gap-2">
              <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 animate-pulse" />
              .NET 10 Web API
            </span>
            <span className="text-xs font-mono text-slate-500">/swagger</span>
          </div>
          <p className="text-[11px] text-slate-400 mt-2">JWT Bearer + Refresh Token Rotation</p>
        </div>

        {/* Card 4: Rules Status */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between mb-4">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
              3 Quy tắc bất biến
            </span>
            <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center text-amber-600">
              <ShieldAlert className="w-5 h-5" />
            </div>
          </div>
          <div className="text-xs font-semibold text-amber-700 space-y-1">
            <p>1. Không âm kho (OCC Token)</p>
            <p>2. Phiếu duyệt cấm sửa</p>
            <p>3. Link duyệt 1 lần (SHA-256)</p>
          </div>
        </div>
      </div>

      {/* Reserved Section for Member 2 */}
      <div className="bg-slate-900/5 rounded-2xl border-2 border-dashed border-slate-300 p-6 sm:p-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <span>Khu vực chờ nối tiếp — Công đoạn Thành viên 2</span>
              <span className="text-xs font-bold bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full">
                Phase 3 sắp diễn ra
              </span>
            </h3>
            <p className="text-xs text-slate-500 mt-1">
              Thành viên 1 đã chuẩn bị sẵn cơ sở dữ liệu, API danh mục và các đường dẫn điều hướng trên Sidebar.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-white p-5 rounded-xl border border-slate-200 opacity-90">
            <div className="w-9 h-9 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center mb-3">
              <FileText className="w-5 h-5" />
            </div>
            <h4 className="text-sm font-bold text-slate-900 mb-1">Phiếu Nhập / Xuất / Chuyển kho</h4>
            <p className="text-xs text-slate-500 leading-relaxed">
              Tạo phiếu nháp, kiểm tra số lượng tồn khả dụng, bảo vệ <strong>Quy tắc 1 (Không âm kho)</strong> và{' '}
              <strong>Quy tắc 2 (Cấm sửa phiếu đã duyệt)</strong>.
            </p>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 opacity-90">
            <div className="w-9 h-9 rounded-lg bg-emerald-100 text-emerald-700 flex items-center justify-center mb-3">
              <Boxes className="w-5 h-5" />
            </div>
            <h4 className="text-sm font-bold text-slate-900 mb-1">Tra cứu Tồn kho & Sổ biến động</h4>
            <p className="text-xs text-slate-500 leading-relaxed">
              Tra cứu tồn kho thực tế (On-hand), tồn giữ chỗ (Reserved) và sổ cái biến động bất biến{' '}
              <code>StockTransactionHistory</code>.
            </p>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 opacity-90">
            <div className="w-9 h-9 rounded-lg bg-purple-100 text-purple-700 flex items-center justify-center mb-3">
              <CheckCircle2 className="w-5 h-5" />
            </div>
            <h4 className="text-sm font-bold text-slate-900 mb-1">Cổng Duyệt Phiếu Kế toán 1 lần</h4>
            <p className="text-xs text-slate-500 leading-relaxed">
              Nhận liên kết bảo mật chứa token ngẫu nhiên, xem bảng kê tài chính và thực thi Duyệt/Từ chối đúng 1 lần (<strong>Quy tắc 3</strong>).
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
