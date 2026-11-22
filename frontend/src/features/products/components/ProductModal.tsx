import React, { useState, useEffect } from 'react';
import { Modal } from '../../../components/Modal';
import { Input } from '../../../components/Input';
import { Button } from '../../../components/Button';
import type { Product, CreateProductRequest, UpdateProductRequest } from '../types';

interface ProductModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateProductRequest | UpdateProductRequest) => Promise<void>;
  product?: Product | null;
  isLoading?: boolean;
}

export const ProductModal: React.FC<ProductModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  product,
  isLoading = false,
}) => {
  const [sku, setSku] = useState('');
  const [name, setName] = useState('');
  const [unit, setUnit] = useState('Chiếc');
  const [basePrice, setBasePrice] = useState<number | string>(0);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!product;

  useEffect(() => {
    if (product) {
      setSku(product.sku);
      setName(product.name);
      setUnit(product.unit);
      setBasePrice(product.basePrice);
    } else {
      setSku('');
      setName('');
      setUnit('Chiếc');
      setBasePrice(0);
    }
    setError(null);
  }, [product, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('Tên sản phẩm không được để trống.');
      return;
    }

    if (!isEditing && !sku.trim()) {
      setError('Mã SKU không được để trống.');
      return;
    }

    if (!unit.trim()) {
      setError('Đơn vị tính không được để trống.');
      return;
    }

    const priceNum = Number(basePrice);
    if (isNaN(priceNum) || priceNum < 0) {
      setError('Giá cơ bản không hợp lệ.');
      return;
    }

    try {
      setError(null);
      if (isEditing) {
        await onSubmit({ name: name.trim(), unit: unit.trim(), basePrice: priceNum });
      } else {
        await onSubmit({
          sku: sku.trim().toUpperCase(),
          name: name.trim(),
          unit: unit.trim(),
          basePrice: priceNum,
        });
      }
      onClose();
    } catch (err: unknown) {
      const errorMsg =
        (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail ||
        'Có lỗi xảy ra khi lưu thông tin sản phẩm.';
      setError(errorMsg);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditing ? `Chỉnh sửa sản phẩm: ${product.sku}` : 'Thêm mới Sản phẩm (SKU)'}
      footer={
        <>
          <Button variant="outline" onClick={onClose} disabled={isLoading} type="button">
            Hủy bỏ
          </Button>
          <Button variant="primary" onClick={handleSubmit} isLoading={isLoading} type="button">
            {isEditing ? 'Lưu thay đổi' : 'Tạo mới sản phẩm'}
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 border border-red-200 text-red-700 text-xs rounded-lg">
            {error}
          </div>
        )}

        <Input
          label="Mã SKU"
          placeholder="VD: LAP-DELL-5520"
          value={sku}
          onChange={(e) => setSku(e.target.value.toUpperCase())}
          disabled={isEditing || isLoading}
          helperText={
            isEditing
              ? 'Mã SKU là mã định danh không thể sửa đổi.'
              : 'Mã định danh duy nhất (chữ cái in hoa, số, dấu gạch ngang).'
          }
        />

        <Input
          label="Tên sản phẩm / Hàng hóa"
          placeholder="VD: Laptop Dell Latitude 5520 i7 16GB"
          value={name}
          onChange={(e) => setName(e.target.value)}
          disabled={isLoading}
        />

        <div className="grid grid-cols-2 gap-4">
          <Input
            label="Đơn vị tính (ĐVT)"
            placeholder="VD: Chiếc, Hộp, Bộ, Thùng..."
            value={unit}
            onChange={(e) => setUnit(e.target.value)}
            disabled={isLoading}
          />

          <Input
            label="Giá cơ bản (VND)"
            type="number"
            min="0"
            step="1000"
            placeholder="VD: 24500000"
            value={basePrice}
            onChange={(e) => setBasePrice(e.target.value)}
            disabled={isLoading}
          />
        </div>
      </form>
    </Modal>
  );
};
