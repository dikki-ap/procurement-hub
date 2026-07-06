import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Trophy, BarChart2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { procurementApi, type VendorScoreInput } from '@/features/procurement/api/procurementApi';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

export default function BidEvaluationPage() {
  const { id }      = useParams<{ id: string }>();
  const navigate    = useNavigate();
  const queryClient = useQueryClient();

  const { data: comparison, isLoading: loadingComp } = useQuery({
    queryKey: ['bid-comparison', id],
    queryFn:  () => procurementApi.getBidComparison(id!).then(r => r.data),
    enabled:  !!id,
  });

  const { data: evaluation } = useQuery({
    queryKey: ['bid-evaluation', id],
    queryFn:  () => procurementApi.getBidEvaluationResult(id!).then(r => r.data),
    enabled:  !!id,
  });

  // Weights
  const [priceWeight,    setPriceWeight]    = useState('50');
  const [qualityWeight,  setQualityWeight]  = useState('30');
  const [deliveryWeight, setDeliveryWeight] = useState('20');

  // Scores per vendor
  const [scores, setScores] = useState<Record<string, { quality: string; delivery: string }>>({});

  const setScore = (quotationId: string, field: 'quality' | 'delivery', value: string) =>
    setScores(prev => ({ ...prev, [quotationId]: { ...prev[quotationId], [field]: value } }));

  const weightsSum = parseFloat(priceWeight || '0') + parseFloat(qualityWeight || '0') + parseFloat(deliveryWeight || '0');
  const weightsValid = Math.abs(weightsSum - 100) <= 0.01;

  const evaluateMutation = useMutation({
    mutationFn: () => procurementApi.evaluateBids(id!, {
      rfqId:         id!,
      priceWeight:   parseFloat(priceWeight),
      qualityWeight: parseFloat(qualityWeight),
      deliveryWeight: parseFloat(deliveryWeight),
      scores: (comparison?.vendors ?? [])
        .filter(v => v.status === 'Submitted')
        .map((v): VendorScoreInput => ({
          quotationId:  v.quotationId,
          vendorId:     v.vendorId,
          qualityScore:  parseFloat(scores[v.quotationId]?.quality ?? '0'),
          deliveryScore: parseFloat(scores[v.quotationId]?.delivery ?? '0'),
        })),
    }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bid-evaluation', id] }),
  });

  const awardMutation = useMutation({
    mutationFn: (args: { quotationId: string; vendorId: string }) =>
      procurementApi.awardVendor(id!, args.quotationId, args.vendorId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bid-evaluation', id] }),
  });

  if (loadingComp) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!comparison) return <div className="p-6 text-red-500">Comparison data not available.</div>;

  const isAwarded = evaluation?.status === 'Awarded';

  return (
    <div className="space-y-8 max-w-5xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(`/app/procurement/rfqs/${id}`)}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back to RFQ
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-semibold">{comparison.rfqNumber} — Bid Evaluation</h1>
          <p className="text-sm text-muted-foreground">{comparison.rfqTitle}</p>
        </div>
      </div>

      {/* Bid Comparison Matrix */}
      <section>
        <h2 className="text-base font-semibold mb-3 flex items-center gap-2">
          <BarChart2 className="h-4 w-4" /> Bid Comparison Matrix
        </h2>
        <div className="overflow-x-auto rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-3 py-2 text-left font-medium text-muted-foreground min-w-[200px]">Item</th>
                <th className="px-3 py-2 text-right font-medium text-muted-foreground">Qty</th>
                {comparison.vendors.map(v => (
                  <th key={v.vendorId} className="px-3 py-2 text-right font-medium text-muted-foreground whitespace-nowrap">
                    {v.vendorName}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {comparison.items.map(item => {
                const prices = comparison.vendors.map(v =>
                  v.itemPrices.find(p => p.rfqItemId === item.rfqItemId)?.unitPrice ?? 0);
                const minPrice = Math.min(...prices.filter(p => p > 0));
                return (
                  <tr key={item.rfqItemId} className="border-t">
                    <td className="px-3 py-2 font-medium">{item.itemDescription}</td>
                    <td className="px-3 py-2 text-right text-muted-foreground">{item.quantity} {item.unitLabel ?? ''}</td>
                    {comparison.vendors.map(v => {
                      const p = v.itemPrices.find(ip => ip.rfqItemId === item.rfqItemId);
                      const isCheapest = p && p.unitPrice === minPrice && minPrice > 0;
                      return (
                        <td key={v.vendorId} className={`px-3 py-2 text-right ${isCheapest ? 'text-green-700 font-semibold' : ''}`}>
                          {p ? `Rp ${fmt(p.unitPrice)}` : '—'}
                        </td>
                      );
                    })}
                  </tr>
                );
              })}
              {/* Total row */}
              <tr className="border-t bg-muted/30 font-semibold">
                <td className="px-3 py-2" colSpan={2}>Total Price</td>
                {comparison.vendors.map(v => {
                  const minTotal = Math.min(...comparison.vendors.map(vv => vv.totalPrice).filter(p => p > 0));
                  return (
                    <td key={v.vendorId} className={`px-3 py-2 text-right ${v.totalPrice === minTotal ? 'text-green-700' : ''}`}>
                      Rp {fmt(v.totalPrice)}
                    </td>
                  );
                })}
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      {/* Weighted Evaluation Form */}
      {!isAwarded && (
        <section className="space-y-4">
          <h2 className="text-base font-semibold">Weighted Evaluation</h2>

          {/* Weights */}
          <div className="grid grid-cols-3 gap-4 p-4 bg-muted/30 rounded-lg">
            {[
              { label: 'Price Weight (%)', value: priceWeight,    set: setPriceWeight },
              { label: 'Quality Weight (%)', value: qualityWeight,  set: setQualityWeight },
              { label: 'Delivery Weight (%)', value: deliveryWeight, set: setDeliveryWeight },
            ].map(({ label, value, set }) => (
              <div key={label}>
                <label className="text-xs text-muted-foreground font-medium">{label}</label>
                <Input
                  type="number" min={0} max={100} step="0.01"
                  value={value}
                  onChange={e => set(e.target.value)}
                  className="mt-1 h-8"
                />
              </div>
            ))}
          </div>
          {!weightsValid && (
            <p className="text-xs text-amber-600">Weights must sum to 100. Current: {weightsSum.toFixed(2)}</p>
          )}

          {/* Per-vendor scores */}
          <div className="rounded-md border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-3 py-2 text-left font-medium text-muted-foreground">Vendor</th>
                  <th className="px-3 py-2 text-right font-medium text-muted-foreground">Total Price</th>
                  <th className="px-3 py-2 text-center font-medium text-muted-foreground">Quality (0–100)</th>
                  <th className="px-3 py-2 text-center font-medium text-muted-foreground">Delivery (0–100)</th>
                </tr>
              </thead>
              <tbody>
                {comparison.vendors.filter(v => v.status === 'Submitted').map(v => (
                  <tr key={v.vendorId} className="border-t">
                    <td className="px-3 py-2 font-medium">{v.vendorName}</td>
                    <td className="px-3 py-2 text-right">Rp {fmt(v.totalPrice)}</td>
                    <td className="px-3 py-2 w-32">
                      <Input type="number" min={0} max={100} step="0.01" placeholder="0"
                        value={scores[v.quotationId]?.quality ?? ''}
                        onChange={e => setScore(v.quotationId, 'quality', e.target.value)}
                        className="h-8 text-center" />
                    </td>
                    <td className="px-3 py-2 w-32">
                      <Input type="number" min={0} max={100} step="0.01" placeholder="0"
                        value={scores[v.quotationId]?.delivery ?? ''}
                        onChange={e => setScore(v.quotationId, 'delivery', e.target.value)}
                        className="h-8 text-center" />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <Button onClick={() => evaluateMutation.mutate()} disabled={!weightsValid || evaluateMutation.isPending}>
            {evaluateMutation.isPending ? 'Calculating…' : 'Calculate Scores'}
          </Button>
        </section>
      )}

      {/* Evaluation Result */}
      {evaluation && (
        <section className="space-y-4">
          <h2 className="text-base font-semibold flex items-center gap-2">
            <Trophy className="h-4 w-4 text-amber-500" /> Evaluation Results
          </h2>
          <div className="p-3 bg-muted/30 rounded-lg text-xs text-muted-foreground">
            Weights — Price: <strong>{evaluation.priceWeight}%</strong> · Quality: <strong>{evaluation.qualityWeight}%</strong> · Delivery: <strong>{evaluation.deliveryWeight}%</strong>
          </div>
          <div className="rounded-md border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-3 py-2 text-left font-medium text-muted-foreground">Vendor</th>
                  <th className="px-3 py-2 text-right font-medium text-muted-foreground">Price Score</th>
                  <th className="px-3 py-2 text-right font-medium text-muted-foreground">Quality Score</th>
                  <th className="px-3 py-2 text-right font-medium text-muted-foreground">Delivery Score</th>
                  <th className="px-3 py-2 text-right font-medium text-muted-foreground font-bold">Weighted Total</th>
                  {!isAwarded && <th className="px-3 py-2" />}
                </tr>
              </thead>
              <tbody>
                {evaluation.scores.map((s, i) => (
                  <tr key={s.quotationId} className={`border-t ${i === 0 ? 'bg-green-50/50' : ''}`}>
                    <td className="px-3 py-2 font-medium">
                      {i === 0 && <Trophy className="inline h-3.5 w-3.5 text-amber-500 mr-1" />}
                      {s.vendorName}
                    </td>
                    <td className="px-3 py-2 text-right">{s.priceScore.toFixed(2)}</td>
                    <td className="px-3 py-2 text-right">{s.qualityScore.toFixed(2)}</td>
                    <td className="px-3 py-2 text-right">{s.deliveryScore.toFixed(2)}</td>
                    <td className="px-3 py-2 text-right font-bold">{s.weightedTotal.toFixed(2)}</td>
                    {!isAwarded && (
                      <td className="px-3 py-2">
                        <Button size="sm" onClick={() => awardMutation.mutate({ quotationId: s.quotationId, vendorId: s.vendorId })}
                          disabled={awardMutation.isPending}>
                          Award
                        </Button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {isAwarded && (
            <div className="p-3 rounded-lg bg-green-50 border border-green-200 text-sm text-green-800 font-medium">
              Bid awarded. The selected vendor will be notified. Proceed to PO creation in Phase 7.
            </div>
          )}
        </section>
      )}
    </div>
  );
}
