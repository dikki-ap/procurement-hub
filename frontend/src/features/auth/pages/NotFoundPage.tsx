import { useNavigate } from 'react-router-dom';
import { Ghost } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';

export default function NotFoundPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const goHome = () => {
    const isVendor = user?.roles.some(r => ['vendor_admin', 'vendor_staff'].includes(r));
    if (isVendor && user?.vendorId) {
      navigate(`/app/vendor-portal/${user.vendorId}/profile`, { replace: true });
    } else {
      navigate('/app/dashboard', { replace: true });
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-slate-50 p-6 text-center">
      <div className="w-24 h-24 rounded-full bg-slate-200 flex items-center justify-center mb-6">
        <Ghost className="h-12 w-12 text-slate-400" />
      </div>

      <p className="text-8xl font-black text-slate-200 leading-none select-none mb-2">404</p>

      <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3">Page Not Found</h1>
      <p className="text-slate-500 max-w-sm mb-8">
        The page you're looking for doesn't exist or may have been moved. Double-check the URL or head back home.
      </p>

      <div className="flex gap-3">
        <Button variant="outline" onClick={() => navigate(-1)}>
          Go Back
        </Button>
        <Button onClick={goHome}>
          Go to Dashboard
        </Button>
      </div>
    </div>
  );
}
