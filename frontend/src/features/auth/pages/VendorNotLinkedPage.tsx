import { Unlink } from 'lucide-react';
import { keycloak } from '@/shared/lib/keycloak';

// "Account Not Linked" = 18 chars → steps(36, end) so typing phase (50%) = 18 steps
export default function VendorNotLinkedPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-slate-50 p-6 text-center">
      <div
        className="w-24 h-24 rounded-full bg-amber-100 flex items-center justify-center mb-6"
        style={{ animation: 'float 3s ease-in-out infinite' }}
      >
        <Unlink
          className="h-12 w-12 text-amber-500"
          style={{ animation: 'wobble 4s ease-in-out infinite' }}
        />
      </div>

      <p
        className="text-8xl font-black text-amber-100 leading-none select-none mb-2"
        style={{ animation: 'fade-up 0.4s ease-out 0.1s both' }}
      >
        401
      </p>

      <h1
        className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3 whitespace-nowrap"
        style={{ clipPath: 'inset(0 100% 0 0)', animation: 'type-loop 5.5s steps(36, end) 0.4s infinite' }}
      >
        Account Not Linked
      </h1>

      <p
        className="text-slate-500 max-w-sm mb-6"
        style={{ animation: 'fade-up 0.5s ease-out 3.2s both' }}
      >
        Your account has not been linked to a vendor profile yet. Please contact your administrator.
      </p>

      <button
        className="px-4 py-2 text-sm bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition-colors"
        style={{ animation: 'fade-up 0.5s ease-out 3.4s both' }}
        onClick={() => keycloak.logout({ redirectUri: window.location.origin })}
      >
        Sign Out
      </button>
    </div>
  );
}
