import {
  AlertCircle, Award, Trophy, TrendingDown, TrendingUp, Minus, Star,
} from 'lucide-react';
import type { VendorTier } from '../api/vendorApi';

/* ── Tier ─────────────────────────────────────────────────────────── */

type TierCfg = { icon: React.ElementType; pill: string; iconCls: string };

const TIER_CFG: Record<VendorTier, TierCfg> = {
  Probation: { icon: AlertCircle, pill: 'bg-slate-100 text-slate-600',   iconCls: 'text-slate-400' },
  Bronze:    { icon: Award,       pill: 'bg-amber-100 text-amber-800',   iconCls: 'text-amber-600' },
  Silver:    { icon: Award,       pill: 'bg-slate-200 text-slate-700',   iconCls: 'text-slate-500' },
  Gold:      { icon: Trophy,      pill: 'bg-yellow-100 text-yellow-700', iconCls: 'text-yellow-500' },
};

export function TierBadge({ tier }: { tier: VendorTier }) {
  const { icon: Icon, pill, iconCls } = TIER_CFG[tier];
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${pill}`}>
      <Icon className={`h-3 w-3 ${iconCls}`} />
      {tier}
    </span>
  );
}

/* ── Score ────────────────────────────────────────────────────────── */

type ScoreCfg = { icon: React.ElementType; text: string; bg: string };

function scoreCfg(score: number): ScoreCfg {
  if (score < 4) return { icon: TrendingDown, text: 'text-red-500',     bg: 'bg-red-50'     };
  if (score < 6) return { icon: Minus,        text: 'text-amber-500',   bg: 'bg-amber-50'   };
  if (score < 8) return { icon: TrendingUp,   text: 'text-blue-500',    bg: 'bg-blue-50'    };
  return              { icon: Star,          text: 'text-emerald-500', bg: 'bg-emerald-50' };
}

/** Compact pill for list tables */
export function ScoreBadge({ score }: { score: number }) {
  const { icon: Icon, text, bg } = scoreCfg(score);
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold ${bg} ${text}`}>
      <Icon className="h-3 w-3" />
      {score.toFixed(1)}
    </span>
  );
}

/** Larger display for detail cards */
export function ScoreDisplay({ score }: { score: number }) {
  const { icon: Icon, text, bg } = scoreCfg(score);
  return (
    <div className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-lg ${bg} ${text}`}>
      <Icon className="h-4 w-4" />
      <span className="text-xl font-bold">{score.toFixed(1)}</span>
    </div>
  );
}
