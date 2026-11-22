import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Search, Package, Edit2, Trash2, RotateCcw, RefreshCw } from 'lucide-react';
import { productsApi } from '../api/productsApi';
import type { Product, CreateProductRequest, UpdateProductRequest, PagedProducts } from '../types';
import { ProductModal } from '../components/ProductModal';
import { Button } from '../../../components/Button';
import { Badge } from '../../../components/Badge';
import { Pagination } from '../../../components/Pagination';
import { useAuth } from '../../auth/context/AuthContext';

export const ProductsPage: React.FC = () => {
  const { canEdit } = useAuth();

  const [data, setData] = useState<PagedProducts | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchProducts = useCallback(async () => {
    try {
      setIsLoading(true);
      const result = await productsApi.getProducts(search, currentPage, 10, includeDeleted);
      setData(result);
    } catch (err) {
      console.error('Failed to load products:', err);
    } finally {
      setIsLoading(false);
    }
  }, [search, currentPage, includeDeleted]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchProducts();
  };

  const handleOpenCreateModal = () => {
    setSelectedProduct(null);
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (product: Product) => {
    setSelectedProduct(product);
    setIsModalOpen(true);
  };

  const handleModalSubmit = async (requestData: CreateProductRequest | UpdateProductRequest) => {
    setIsSubmitting(true);
    try {
      if (selectedProduct) {
        await productsApi.updateProduct(selectedProduct.id, requestData as UpdateProductRequest);
      } else {
        await productsApi.createProduct(requestData as CreateProductRequest);
      }
      await fetchProducts();
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSoftDelete = async (product: Product) => {
    if (
      !window.confirm(
        `Xác nhận XÓA MỀM sản phẩm "${product.name}" (${product.sku})?\n\nLưu ý: Sản phẩm sẽ được ẩn khỏi danh sách kinh doanh nhưng vẫn bảo toàn lịch sử giao dịch kho.`
      )
    ) {
      return;
    }
    try {
      await productsApi.deleteProduct(product.id);
      await fetchProducts();
    } catch (err) {
      alert('Không thể xóa mềm sản phẩm.');
      console.error(err);
    }
  };

  const handleRestore = async (product: Product) => {
    if (!window.confirm(`Khôi phục sản phẩm "${product.name}" (${product.sku}) trở lại kinh doanh?`)) {
      return;
    }
    try {
      await productsApi.restoreProduct(product.id);
      await fetchProducts();
    } catch (err) {
      alert('Không thể khôi phục sản phẩm.');
      console.error(err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="space-y-6">
      {/* Top Banner / Actions */}
      <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-xs flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900 flex items-center gap-2">
            <Package className="w-6 h-6 text-blue-600" />
            <span>Danh mục Hàng hóa (Sản phẩm)</span>
          </h2>
          <p className="text-xs text-slate-500 mt-1">
            Quản lý mã hàng hóa (SKU), đơn vị tính, giá nhập xuất và trạng thái xóa mềm bảo toàn dữ liệu
          </p>
        </div>

        {canEdit && (
          <Button variant="primary" onClick={handleOpenCreateModal}>
            <Plus className="w-4 h-4" />
            <span>Thêm sản phẩm mới</span>
          </Button>
        )}
      </div>

      {/* Filter and Search Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col md:flex-row items-center justify-between gap-3">
        <form onSubmit={handleSearchSubmit} className="w-full md:w-96 relative">
          <input
            type="text"
            placeholder="Tìm theo mã SKU hoặc tên sản phẩm..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
        </form>

        <div className="flex items-center gap-4 w-full md:w-auto justify-end">
          <label className="flex items-center gap-2 text-xs font-semibold text-slate-600 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={includeDeleted}
              onChange={(e) => {
                setIncludeDeleted(e.target.checked);
                setCurrentPage(1);
              }}
              className="w-4 h-4 text-blue-600 rounded border-slate-300 focus:ring-blue-500 cursor-pointer"
            />
            <span>Hiển thị sản phẩm đã xóa mềm</span>
          </label>

          <Button variant="outline" size="sm" onClick={() => fetchProducts()} title="Tải lại danh sách">
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </div>

      {/* Products Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200 text-xs font-semibold uppercase text-slate-500">
              <tr>
                <th className="py-3 px-4">Mã SKU</th>
                <th className="py-3 px-4">Tên hàng hóa / Sản phẩm</th>
                <th className="py-3 px-4">ĐVT</th>
                <th className="py-3 px-4 text-right">Giá cơ bản</th>
                <th className="py-3 px-4">Trạng thái</th>
                <th className="py-3 px-4">Ngày tạo</th>
                {canEdit && <th className="py-3 px-4 text-right">Thao tác</th>}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400">
                    <div className="inline-flex items-center gap-2">
                      <RefreshCw className="w-5 h-5 animate-spin text-blue-600" />
                      <span>Đang tải dữ liệu hàng hóa...</span>
                    </div>
                  </td>
                </tr>
              ) : !data || data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400">
                    Không tìm thấy sản phẩm nào phù hợp.
                  </td>
                </tr>
              ) : (
                data.items.map((product) => (
                  <tr
                    key={product.id}
                    className={`hover:bg-slate-50/80 transition-colors ${
                      product.isDeleted ? 'bg-slate-50/50 text-slate-400' : ''
                    }`}
                  >
                    <td className="py-3.5 px-4 font-mono font-bold text-xs">
                      <span className={product.isDeleted ? 'line-through text-slate-400' : 'text-blue-600'}>
                        {product.sku}
                      </span>
                    </td>
                    <td className="py-3.5 px-4 font-semibold text-slate-900">
                      <div className="flex items-center gap-2">
                        <span className={product.isDeleted ? 'line-through text-slate-400' : ''}>
                          {product.name}
                        </span>
                        {product.isDeleted && (
                          <span className="text-[10px] bg-red-100 text-red-600 font-bold px-1.5 py-0.5 rounded">
                            Đã xóa mềm
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="py-3.5 px-4 text-xs font-medium text-slate-600">
                      {product.unit}
                    </td>
                    <td className="py-3.5 px-4 text-right font-semibold text-slate-900">
                      {formatCurrency(product.basePrice)}
                    </td>
                    <td className="py-3.5 px-4">
                      {product.isDeleted ? (
                        <Badge variant="danger">Đã ngưng bán</Badge>
                      ) : (
                        <Badge variant="success">Đang kinh doanh</Badge>
                      )}
                    </td>
                    <td className="py-3.5 px-4 text-xs text-slate-500">
                      {new Date(product.createdAt).toLocaleDateString('vi-VN')}
                    </td>
                    {canEdit && (
                      <td className="py-3.5 px-4 text-right space-x-1.5 whitespace-nowrap">
                        {!product.isDeleted ? (
                          <>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleOpenEditModal(product)}
                              title="Sửa thông tin sản phẩm"
                            >
                              <Edit2 className="w-3.5 h-3.5 text-slate-600" />
                              <span>Sửa</span>
                            </Button>

                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleSoftDelete(product)}
                              className="hover:border-red-300 hover:bg-red-50 text-red-600"
                              title="Xóa mềm (bảo toàn lịch sử)"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                              <span>Xóa</span>
                            </Button>
                          </>
                        ) : (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleRestore(product)}
                            className="hover:border-emerald-300 hover:bg-emerald-50 text-emerald-600"
                            title="Khôi phục sản phẩm"
                          >
                            <RotateCcw className="w-3.5 h-3.5" />
                            <span>Khôi phục</span>
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
      <ProductModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleModalSubmit}
        product={selectedProduct}
        isLoading={isSubmitting}
      />
    </div>
  );
};
