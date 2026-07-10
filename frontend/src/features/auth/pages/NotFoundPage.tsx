import { useNavigate } from 'react-router-dom';
import { Ghost } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';

const VENDOR_ROLES = ['vendor_admin', 'vendor_staff'];

export default function NotFoundPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const isVendor = user?.roles.some(r => VENDOR_ROLES.includes(r)) ?? false;

  const handleHome = () => {
    if (isVendor && user?.vendorId) {
      navigate(`/app/vendor-portal/${user.vendorId}/profile`, { replace: true });
    } else if (isVendor) {
      navigate('/', { replace: true });
    } else if (user) {
      navigate('/app/dashboard', { replace: true });
    } else {
      navigate('/', { replace: true });
    }
  };

  const label = isVendor && user?.vendorId
    ? 'Go to Vendor Portal'
    : user
      ? 'Go to Dashboard'
      : 'Login';

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-slate-50 p-6 text-center">
      <div className="w-24 h-24 rounded-full bg-slate-200 flex items-center justify-center mb-6">
        <Ghost className="h-12 w-12 text-slate-400" />
      </div>

      <p className="text-8xl font-black text-slate-200 leading-none select-none mb-2">404</p>

      <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3">Page Not Found</h1>
      <p className="text-slate-500 max-w-sm mb-8">
        The page you're looking for doesn't exist or may have been moved.
      </p>

      <Button onClick={handleHome}>{label}</Button>
    </div>
  );
}
