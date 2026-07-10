import { ShieldX } from 'lucide-react';

export default function UnauthorizedPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-slate-50 p-6 text-center">
      <div
        className="w-24 h-24 rounded-full bg-red-100 flex items-center justify-center mb-6"
        style={{ animation: 'ring-pulse 2.5s ease-in-out infinite' }}
      >
        <ShieldX
          className="h-12 w-12 text-red-500"
          style={{ animation: 'wobble 4s ease-in-out infinite' }}
        />
      </div>

      <p className="text-8xl font-black text-red-100 leading-none select-none mb-2">403</p>

      <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3">Access Denied</h1>
      <p className="text-slate-500 max-w-sm">
        You don't have permission to view this page. Contact your administrator if you believe this is a mistake.
      </p>
    </div>
  );
}
