import { useNavigate } from 'react-router-dom';
import { ShieldX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';

const VENDOR_ROLES = ['vendor_admin', 'vendor_staff'];

export default function UnauthorizedPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const isVendor = user?.roles.some(r => VENDOR_ROLES.includes(r)) ?? false;

  const handleHome = () => {
    if (isVendor && user?.vendorId) {
      navigate(`/app/vendor-portal/${user.vendorId}/profile`, { replace: true });
    } else if (isVendor) {
      // No vendorId yet — go back to root so LoginRedirectPage resolves it
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
      <div className="w-24 h-24 rounded-full bg-red-100 flex items-center justify-center mb-6">
        <ShieldX className="h-12 w-12 text-red-500" />
      </div>

      <p className="text-8xl font-black text-red-100 leading-none select-none mb-2">403</p>

      <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3">Access Denied</h1>
      <p className="text-slate-500 max-w-sm mb-8">
        You don't have permission to view this page. Contact your administrator if you believe this is a mistake.
      </p>

      <Button onClick={handleHome}>{label}</Button>
    </div>
  );
}
