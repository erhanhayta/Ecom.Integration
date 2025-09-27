// src/pages/Products/BulkPriceModal.jsx
import React, { useMemo, useState } from "react";
import { bulkPrice } from "../../api/variants";
import { useToast } from "../../ui-toast";

export default function BulkPriceModal({ product, open, onClose, onDone }) {
  const { toast } = useToast();
  const variants = product?.variants || [];

  const colors = useMemo(() => {
    const set = new Set(variants.map(v => (v.color || "").trim()).filter(Boolean));
    return Array.from(set);
  }, [variants]);

  const sizes = useMemo(() => {
    const set = new Set(variants.map(v => (v.size || "").trim()).filter(Boolean));
    return Array.from(set);
  }, [variants]);

  const [scope, setScope] = useState("all"); // all | color | size | both
  const [color, setColor] = useState(colors[0] || "");
  const [size, setSize] = useState(sizes[0] || "");
  const [op, setOp] = useState("set"); // set, inc, dec, incp, decp
  const [value, setValue] = useState("");

  if (!open) return null;

  async function submit() {
    const payload = {
      op,
      value: Number(value || 0),
      color: scope === "color" || scope === "both" ? (color || undefined) : undefined,
      size: scope === "size" || scope === "both" ? (size || undefined) : undefined,
    };
    try {
      const res = await bulkPrice(product.id, payload);
      toast({ title: `Güncellendi`, description: `${res.updated} varyant` });
      onDone && onDone(res.updated);
      onClose && onClose();
    } catch (e) {
      toast({ title: "Hata", description: e.message, variant: "destructive" });
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-2xl bg-white shadow-xl">
        <div className="p-4 border-b flex items-center justify-between">
          <div className="font-semibold">Toplu Fiyat Güncelle</div>
          <button className="text-gray-500 hover:text-gray-800" onClick={onClose}>✕</button>
        </div>

        <div className="p-4 space-y-4">
          <div>
            <label className="block text-sm mb-1">Kapsam</label>
            <select className="border rounded px-3 py-2 w-full"
              value={scope} onChange={e => setScope(e.target.value)}>
              <option value="all">Tüm varyantlar</option>
              <option value="color">Sadece renge göre</option>
              <option value="size">Sadece bedene göre</option>
              <option value="both">Renk + Beden</option>
            </select>
          </div>

          {(scope === "color" || scope === "both") && (
            <div>
              <label className="block text-sm mb-1">Renk</label>
              <select className="border rounded px-3 py-2 w-full"
                value={color} onChange={e => setColor(e.target.value)}>
                {colors.length ? colors.map(c => <option key={c} value={c}>{c}</option>) : <option>Renk yok</option>}
              </select>
            </div>
          )}

          {(scope === "size" || scope === "both") && (
            <div>
              <label className="block text-sm mb-1">Beden</label>
              <select className="border rounded px-3 py-2 w-full"
                value={size} onChange={e => setSize(e.target.value)}>
                {sizes.length ? sizes.map(s => <option key={s} value={s}>{s}</option>) : <option>Beden yok</option>}
              </select>
            </div>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm mb-1">İşlem</label>
              <select className="border rounded px-3 py-2 w-full"
                value={op} onChange={e => setOp(e.target.value)}>
                <option value="set">= Ayarla</option>
                <option value="inc">+ Ekle</option>
                <option value="dec">- Çıkar</option>
                <option value="incp">% Artır</option>
                <option value="decp">% Azalt</option>
              </select>
            </div>
            <div>
              <label className="block text-sm mb-1">Değer</label>
              <input className="border rounded px-3 py-2 w-full" type="number" step="0.01"
                value={value} onChange={e => setValue(e.target.value)} />
            </div>
          </div>
        </div>

        <div className="p-4 border-t flex justify-end gap-3">
          <button className="px-4 py-2 rounded border hover:bg-gray-50" onClick={onClose}>Vazgeç</button>
          <button className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700" onClick={submit}>Uygula</button>
        </div>
      </div>
    </div>
  );
}
