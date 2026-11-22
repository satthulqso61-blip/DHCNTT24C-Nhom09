import React, { useState, useEffect } from 'react';
import { Modal } from '../../../components/Modal';
import { Input } from '../../../components/Input';
import { Button } from '../../../components/Button';
import type { Warehouse, CreateWarehouseRequest, UpdateWarehouseRequest } from '../types';

interface WarehouseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateWarehouseRequest | UpdateWarehouseRequest) => Promise<void>;
  warehouse?: Warehouse | null;
  isLoading?: boolean;
}

export const WarehouseModal: React.FC<WarehouseModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  warehouse,
  isLoading = false,
}) => {
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!warehouse;

  useEffect(() => {
    if (warehouse) {
      setCode(warehouse.code);
      setName(warehouse.name);
      setAddress(warehouse.address || '');
      setIsActive(warehouse.isActive);
    } else {
      setCode('');
      setName('');
      setAddress('');
      setIsActive(true);
    }
    setError(null);
  }, [warehouse, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('Tên kho không được để trống.');
      return;
    }

    if (!isEditing && !code.trim()) {
      setError('Mã kho không được để trống.');
      return;
    }

    try {
      setError(null);
      if (isEditing) {
        await onSubmit({ name: name.trim(), address: address.trim(), isActive });
      } else {
        await onSubmit({ code: code.trim().toUpperCase(), name: name.trim(), address: address.trim(), isActive });
      }
      onClose();
    } catch (err: unknown) {
      const errorMsg =
        (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail ||
        'Có lỗi xảy ra khi lưu thông tin kho.';
      setError(errorMsg);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditing ? `Chỉnh sửa kho: ${warehouse.code}` : 'Thêm mới Kho hàng'}
      footer={
        <>
          <Button variant="outline" onClick={onClose} disabled={isLoading} type="button">
            Hủy bỏ
          </Button>
          <Button variant="primary" onClick={handleSubmit} isLoading={isLoading} type="button">
            {isEditing ? 'Lưu thay đổi' : 'Tạo mới kho'}
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
          label="Mã kho (Code)"
          placeholder="VD: WH-DANANG"
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          disabled={isEditing || isLoading}
          helperText={isEditing ? 'Mã kho là định danh bất biến, không thể sửa đổi.' : 'Mã viết hoa, không dấu, không khoảng trắng.'}
        />

        <Input
          label="Tên kho hàng"
          placeholder="VD: Kho Đà Nẵng (Hải Châu)"
          value={name}
          onChange={(e) => setName(e.target.value)}
          disabled={isLoading}
        />

        <Input
          label="Địa chỉ chi tiết"
          placeholder="Số nhà, đường, quận/huyện, tỉnh/thành phố..."
          value={address}
          onChange={(e) => setAddress(e.target.value)}
          disabled={isLoading}
        />

        <div className="flex items-center gap-2 pt-2">
          <input
            type="checkbox"
            id="warehouse-active"
            checked={isActive}
            onChange={(e) => setIsActive(e.target.checked)}
            className="w-4 h-4 text-blue-600 rounded border-slate-300 focus:ring-blue-500 cursor-pointer"
            disabled={isLoading}
          />
          <label htmlFor="warehouse-active" className="text-sm font-medium text-slate-700 cursor-pointer">
            Kho đang hoạt động (Active)
          </label>
        </div>
      </form>
    </Modal>
  );
};
