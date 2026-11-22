import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Search, Warehouse as WarehouseIcon, Edit2, PowerOff, RefreshCw } from 'lucide-react';
import { warehousesApi } from '../api/warehousesApi';
import type { Warehouse, CreateWarehouseRequest, UpdateWarehouseRequest, PagedWarehouses } from '../types';
import { WarehouseModal } from '../components/WarehouseModal';
import { Button } from '../../../components/Button';
import { Badge } from '../../../components/Badge';
import { Pagination } from '../../../components/Pagination';
import { useAuth } from '../../auth/context/AuthContext';

export const WarehousesPage: React.FC = () => {
  const { canEdit } = useAuth();

  const [data, setData] = useState<PagedWarehouses | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [currentPage, setCurrentPage] = useState(1);

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedWarehouse, setSelectedWarehouse] = useState<Warehouse | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchWarehouses = useCallback(async () => {
    try {
      setIsLoading(true);
      const isActive = statusFilter === 'all' ? undefined : statusFilter === 'active';
      const result = await warehousesApi.getWarehouses(search, isActive, currentPage, 10);
      setData(result);
    } catch (err) {
      console.error('Failed to load warehouses:', err);
    } finally {
      setIsLoading(false);
    }
  }, [search, statusFilter, currentPage]);

  useEffect(() => {
    fetchWarehouses();
  }, [fetchWarehouses]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchWarehouses();
  };

  const handleOpenCreateModal = () => {
    setSelectedWarehouse(null);
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (warehouse: Warehouse) => {
    setSelectedWarehouse(warehouse);
    setIsModalOpen(true);
  };

  const handleModalSubmit = async (requestData: CreateWarehouseRequest | UpdateWarehouseRequest) => {
    setIsSubmitting(true);
    try {
      if (selectedWarehouse) {
        await warehousesApi.updateWarehouse(selectedWarehouse.id, requestData as UpdateWarehouseRequest);
      } else {
        await warehousesApi.createWarehouse(requestData as CreateWarehouseRequest);
      }
      await fetchWarehouses();
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleToggleDeactivate = async (warehouse: Warehouse) => {
    if (!window.confirm(`Bạn có chắc chắn muốn ngưng hoạt động kho "${warehouse.name}"?`)) {
      return;
    }
    try {
      await warehousesApi.deleteWarehouse(warehouse.id);
      await fetchWarehouses();
    } catch (err) {
      alert('Không thể ngưng hoạt động kho.');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      {/* Top Banner / Actions */}
      <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900 flex items-center gap-2">
            <WarehouseIcon className="w-6 h-6 text-blue-600" />
            <span>Danh sách Kho hàng</span>
          </h2>
          <p className="text-xs text-slate-500 mt-1">
            Quản lý hệ thống các kho tổng và chi nhánh phân phối trên toàn quốc
          </p>
        </div>

        {canEdit && (
          <Button variant="primary" onClick={handleOpenCreateModal}>
            <Plus className="w-4 h-4" />
            <span>Thêm kho hàng mới</span>
          </Button>
        )}
      </div>

      {/* Filter and Search Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col md:flex-row items-center justify-between gap-3">
        <form onSubmit={handleSearchSubmit} className="w-full md:w-96 relative">
          <input
            type="text"
            placeholder="Tìm theo mã kho hoặc tên kho..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
        </form>

        <div className="flex items-center gap-2 w-full md:w-auto justify-end">
          <span className="text-xs font-semibold text-slate-500">Trạng thái:</span>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as 'all' | 'active' | 'inactive');
              setCurrentPage(1);
            }}
            className="px-3 py-1.5 border border-slate-300 rounded-lg text-xs font-medium bg-white text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
          >
            <option value="all">Tất cả trạng thái</option>
            <option value="active">Đang hoạt động</option>
            <option value="inactive">Đã ngưng</option>
          </select>

          <Button variant="outline" size="sm" onClick={() => fetchWarehouses()} title="Tải lại danh sách">
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </div>

      {/* Warehouses Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200 text-xs font-semibold uppercase text-slate-500">
              <tr>
                <th className="py-3 px-4">Mã kho</th>
                <th className="py-3 px-4">Tên kho hàng</th>
                <th className="py-3 px-4">Địa chỉ</th>
                <th className="py-3 px-4">Trạng thái</th>
                <th className="py-3 px-4">Ngày tạo</th>
                {canEdit && <th className="py-3 px-4 text-right">Thao tác</th>}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="py-12 text-center text-slate-400">
                    <div className="inline-flex items-center gap-2">
                      <RefreshCw className="w-5 h-5 animate-spin text-blue-600" />
                      <span>Đang tải dữ liệu kho hàng...</span>
                    </div>
                  </td>
                </tr>
              ) : !data || data.items.length === 0 ? (
                <tr>
                  <td colSpan={6} className="py-12 text-center text-slate-400">
                    Không tìm thấy kho hàng nào phù hợp với điều kiện tìm kiếm.
                  </td>
                </tr>
              ) : (
                data.items.map((warehouse) => (
                  <tr key={warehouse.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3.5 px-4 font-mono font-bold text-blue-600 text-xs">
                      {warehouse.code}
                    </td>
                    <td className="py-3.5 px-4 font-semibold text-slate-900">
                      {warehouse.name}
                    </td>
                    <td className="py-3.5 px-4 text-slate-600 text-xs max-w-xs truncate">
                      {warehouse.address || <span className="text-slate-400 italic">Chưa cập nhật</span>}
                    </td>
                    <td className="py-3.5 px-4">
                      {warehouse.isActive ? (
                        <Badge variant="success">Hoạt động</Badge>
                      ) : (
                        <Badge variant="neutral">Ngưng hoạt động</Badge>
                      )}
                    </td>
                    <td className="py-3.5 px-4 text-xs text-slate-500">
                      {new Date(warehouse.createdAt).toLocaleDateString('vi-VN')}
                    </td>
                    {canEdit && (
                      <td className="py-3.5 px-4 text-right space-x-1.5 whitespace-nowrap">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleOpenEditModal(warehouse)}
                          title="Chỉnh sửa thông tin kho"
                        >
                          <Edit2 className="w-3.5 h-3.5 text-slate-600" />
                          <span>Sửa</span>
                        </Button>

                        {warehouse.isActive && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleToggleDeactivate(warehouse)}
                            className="hover:border-red-300 hover:bg-red-50 text-red-600"
                            title="Ngưng hoạt động kho"
                          >
                            <PowerOff className="w-3.5 h-3.5" />
                            <span>Ngưng</span>
                          </Button>
                        )}
                      </td>
                    )}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && (
          <Pagination
            currentPage={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            pageSize={data.pageSize}
            onPageChange={(p) => setCurrentPage(p)}
          />
        )}
      </div>

      {/* Modal */}
      <WarehouseModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleModalSubmit}
        warehouse={selectedWarehouse}
        isLoading={isSubmitting}
      />
    </div>
  );
};
