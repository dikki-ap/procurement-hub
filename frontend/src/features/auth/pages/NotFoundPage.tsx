import { Ghost } from 'lucide-react';

// "Page Not Found" = 14 chars → steps(14) gives char-by-char reveal
export default function NotFoundPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-slate-50 p-6 text-center">
      <div
        className="w-24 h-24 rounded-full bg-slate-200 flex items-center justify-center mb-6"
        style={{ animation: 'float 3s ease-in-out infinite' }}
      >
        <Ghost className="h-12 w-12 text-slate-400" />
      </div>

      <p
        className="text-8xl font-black text-slate-200 leading-none select-none mb-2"
        style={{ animation: 'fade-up 0.4s ease-out 0.1s both' }}
      >
        404
      </p>

      <h1
        className="text-2xl sm:text-3xl font-bold text-slate-900 mb-3 whitespace-nowrap"
        style={{ clipPath: 'inset(0 100% 0 0)', animation: 'type-loop 5s steps(28, end) 0.4s infinite' }}
      >
        Page Not Found
      </h1>

      <p
        className="text-slate-500 max-w-sm"
        style={{ animation: 'fade-up 0.5s ease-out 3s both' }}
      >
        The page you're looking for doesn't exist or may have been moved.
      </p>
    </div>
  );
}
