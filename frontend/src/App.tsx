import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

export default function App() {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center">
      <div className="text-center space-y-4">
        <h1 className="text-3xl font-bold text-foreground">ProcureHub</h1>
        <p className="text-muted-foreground">Supplier Management &amp; Procurement Portal</p>
        <div className="flex gap-2 justify-center">
          <Badge variant="outline">React 19</Badge>
          <Badge variant="outline">Vite 8</Badge>
          <Badge variant="outline">Tailwind v4</Badge>
          <Badge variant="outline">shadcn/ui</Badge>
        </div>
        <Button>Get Started</Button>
      </div>
    </div>
  );
}
