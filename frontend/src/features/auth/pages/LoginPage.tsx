import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Button } from '../../../components/Button';
import { Input } from '../../../components/Input';
import { Lock, User as UserIcon, AlertCircle, Shield, CheckCircle } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!usernameOrEmail || !password) {
      setError('Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.');
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      await login({ usernameOrEmail, password });
      navigate('/dashboard');
    } catch (err: unknown) {
      const errorMsg =
        (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail ||
        'Tên đăng nhập hoặc mật khẩu không chính xác.';
      setError(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickLogin = async (roleUsername: string) => {
    setUsernameOrEmail(roleUsername);
    setPassword(roleUsername);
    try {
      setIsLoading(true);
      setError(null);
      await login({ usernameOrEmail: roleUsername, password: roleUsername });
      navigate('/dashboard');
    } catch (err: unknown) {
      const errorMsg =
        (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail ||
        'Đăng nhập tài khoản mẫu thất bại.';
      setError(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-xl border border-slate-200 overflow-hidden">
        {/* Header */}
        <div className="bg-gradient-to-br from-slate-900 to-blue-950 p-8 text-center text-white">
          <div className="w-14 h-14 bg-blue-600 rounded-2xl flex items-center justify-center mx-auto mb-4 text-2xl font-black shadow-lg shadow-blue-500/40">
            S
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-white mb-1">Đăng nhập StockHub</h1>
          <p className="text-xs text-blue-300">Hệ thống Quản lý kho hàng & Phê duyệt tài chính</p>
        </div>

        {/* Form Body */}
        <div className="p-8">
          {error && (
            <div className="mb-5 p-3.5 bg-red-50 border border-red-200 text-red-700 text-xs rounded-xl flex items-center gap-2.5">
              <AlertCircle className="w-4 h-4 shrink-0 text-red-500" />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <Input
                label="Tên đăng nhập hoặc Email"
                placeholder="Nhập tên đăng nhập..."
                value={usernameOrEmail}
                onChange={(e) => setUsernameOrEmail(e.target.value)}
                disabled={isLoading}
              />
            </div>

            <div>
              <Input
                label="Mật khẩu"
                type="password"
                placeholder="Nhập mật khẩu..."
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
              />
            </div>

            <Button type="submit" variant="primary" className="w-full mt-2" size="lg" isLoading={isLoading}>
              <Lock className="w-4 h-4" />
              <span>Đăng nhập hệ thống</span>
            </Button>
          </form>

          {/* Quick Demo Switcher (Phục vụ video demo 5-10 phút) */}
          <div className="mt-8 pt-6 border-t border-slate-200">
            <p className="text-[11px] font-bold uppercase tracking-wider text-slate-500 text-center mb-3">
              Chế độ Demo nhanh (1 Click)
            </p>
            <div className="grid grid-cols-3 gap-2">
              <button
                type="button"
                onClick={() => handleQuickLogin('admin')}
                className="px-2.5 py-2 text-xs font-semibold rounded-lg border border-purple-200 bg-purple-50 text-purple-700 hover:bg-purple-100 transition-colors flex flex-col items-center gap-1 cursor-pointer"
              >
                <Shield className="w-3.5 h-3.5 text-purple-600" />
                <span>Admin</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('staff')}
                className="px-2.5 py-2 text-xs font-semibold rounded-lg border border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100 transition-colors flex flex-col items-center gap-1 cursor-pointer"
              >
                <UserIcon className="w-3.5 h-3.5 text-blue-600" />
                <span>Thủ kho</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('accountant')}
                className="px-2.5 py-2 text-xs font-semibold rounded-lg border border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 transition-colors flex flex-col items-center gap-1 cursor-pointer"
              >
                <CheckCircle className="w-3.5 h-3.5 text-emerald-600" />
                <span>Kế toán</span>
              </button>
            </div>
            <p className="text-[10px] text-slate-400 text-center mt-2.5">
              Tài khoản mẫu: <code>admin/admin</code>, <code>staff/staff</code>, <code>accountant/accountant</code>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
