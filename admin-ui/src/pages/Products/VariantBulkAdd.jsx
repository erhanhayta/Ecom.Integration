import React, { useMemo, useState } from "react";
import { addVariant } from "../../api/variants";
import { useToast } from "../../ui-toast";

/**
 * Toplu varyant oluşturma aracı.
 * - Renkler (serbest metin, virgülle ayır)
 * - Bedenler (örn. 42,40,38; virgülle ayır)
 * - Fiyat/Stok/IsActive defaultları
 * - SKU şablonu: {CODE}-{COLOR}-{SIZE}
 *
 * Props:
 *  - product: { id, productCode, name, ... }
 *  - onCreated?: (count) => void   // başarıyla eklenen sayısı
 */
export default function VariantBulkAdd({ product, onCreated }) {
  const { toast } = useToast();

  const [colorsRaw, setColorsRaw] = useState("");
  const [sizesRaw, setSizesRaw] = useState("");
  const [price, setPrice] = useState("");
  const [stock, setStock] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [skuTpl, setSkuTpl] = useState("{CODE}-{COLOR}-{SIZE}");
  const [barcodeTpl, setBarcodeTpl] = useState(""); // opsiyonel

  const colors = useMemo(() => dedupSplit(colorsRaw), [colorsRaw]);
  const sizes = useMemo(() => dedupSplit(sizesRaw), [sizesRaw]);

  const combos = useMemo(() => {
    if (!colors.length && !sizes.length) return [];
    if (!colors.length) return sizes.map(sz => ({ color: "", size: sz }));
    if (!sizes.length) return colors.map(cl => ({ color: cl, size: "" }));
    const arr = [];
    for (const c of colors) for (const s of sizes) arr.push({ color: c, size: s });
    return arr;
  }, [colors, sizes]);

  function dedupSplit(s) {
    return (s || "")
      .split(/[,;\n]+/g)
      .map(x => x.trim())
      .filter(Boolean)
      .filter((x, i, a) => a.indexOf(x) === i);
  }

  function makeSku(color, size) {
    const code = (product?.productCode || "").toString().trim();
    return skuTpl
      .replace("{CODE}", code)
      .replace("{COLOR}", (color || "").replace(/\s+/g, "").toUpperCase())
      .replace("{SIZE}", (size || "").toString());
  }

  function makeBarcode(color, size) {
    if (!barcodeTpl) return "";
    const code = (product?.productCode || "").toString().trim();
    return barcodeTpl
      .replace("{CODE}", code)
      .replace("{COLOR}", (color || "").replace(/\s+/g, "").toUpperCase())
      .replace("{SIZE}", (size || "").toString());
  }

  async function handleCreateAll() {
    if (!product?.id) {
      toast({ title: "Ürün bulunamadı", variant: "destructive" });
      return;
    }
    if (!combos.length) {
      toast({ title: "Oluşturulacak kombinasyon yok", description: "Renk veya beden giriniz.", variant: "destructive" });
      return;
    }
    const priceNum = parseFloat(price || "0") || 0;
    const stockNum = parseInt(stock || "0", 10) || 0;

    let ok = 0, fail = 0;
    for (const k of combos) {
      const payload = {
        color: k.color || "",
        size: k.size || "",
        sku: makeSku(k.color, k.size),
        barcode: makeBarcode(k.color, k.size),
        price: priceNum,
        stock: stockNum,
        isActive: !!isActive,
      };
      try {
        await addVariant(product.id, payload);
        ok++;
      } catch (e) {
        fail++;
        console.error("variant add failed", payload, e);
      }
    }
    if (ok) toast({ title: `${ok} varyant eklendi`, description: fail ? `${fail} tanesi hatalı.` : undefined });
    if (typeof onCreated === "function") onCreated(ok);
  }

  return (
    <div className="space-y-4">
      <div className="text-lg font-semibold">Toplu Varyant Oluştur</div>

      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Renkler (virgülle ayır)</label>
          <textarea
            className="w-full border rounded px-3 py-2 h-24"
            placeholder="Siyah, Beyaz, Lacivert"
            value={colorsRaw}
            onChange={e => setColorsRaw(e.target.value)}
          />
          {!!colors.length && <div className="text-xs text-gray-500 mt-1">{colors.length} renk</div>}
        </div>
        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Bedenler (virgülle ayır)</label>
          <textarea
            className="w-full border rounded px-3 py-2 h-24"
            placeholder="42, 40, 38"
            value={sizesRaw}
            onChange={e => setSizesRaw(e.target.value)}
          />
          {!!sizes.length && <div className="text-xs text-gray-500 mt-1">{sizes.length} beden</div>}
        </div>
      </div>

      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-6 md:col-span-3">
          <label className="block text-sm text-gray-600 mb-1">Fiyat</label>
          <input className="w-full border rounded px-3 py-2" type="number" min="0" step="0.01" value={price} onChange={e=>setPrice(e.target.value)} />
        </div>
        <div className="col-span-6 md:col-span-3">
          <label className="block text-sm text-gray-600 mb-1">Stok</label>
          <input className="w-full border rounded px-3 py-2" type="number" min="0" step="1" value={stock} onChange={e=>setStock(e.target.value)} />
        </div>
        <div className="col-span-12 md:col-span-3">
          <label className="block text-sm text-gray-600 mb-1">SKU Şablonu</label>
          <input className="w-full border rounded px-3 py-2" value={skuTpl} onChange={e=>setSkuTpl(e.target.value)} />
          <div className="text-xs text-gray-500 mt-1">{`Değişkenler: {CODE}, {COLOR}, {SIZE}`}</div>
        </div>
        <div className="col-span-12 md:col-span-3">
          <label className="block text-sm text-gray-600 mb-1">Barkod Şablonu</label>
          <input className="w-full border rounded px-3 py-2" value={barcodeTpl} onChange={e=>setBarcodeTpl(e.target.value)} placeholder="örn. {CODE}{SIZE}" />
        </div>
      </div>

      <div className="flex items-center gap-3">
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isActive} onChange={e=>setIsActive(e.target.checked)} />
          Aktif
        </label>
        <div className="text-sm text-gray-500">Oluşturulacak kombinasyon: <b>{combos.length}</b></div>
      </div>

      {/* Önizleme */}
      <div className="border rounded-lg overflow-hidden">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50 text-left">
            <tr>
              <th className="px-3 py-2">Renk</th>
              <th className="px-3 py-2">Beden</th>
              <th className="px-3 py-2">SKU</th>
              <th className="px-3 py-2">Barkod</th>
              <th className="px-3 py-2">Fiyat</th>
              <th className="px-3 py-2">Stok</th>
              <th className="px-3 py-2">Aktif</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {combos.length ? combos.map((k,i)=>(
              <tr key={i}>
                <td className="px-3 py-2">{k.color || "-"}</td>
                <td className="px-3 py-2">{k.size || "-"}</td>
                <td className="px-3 py-2">{makeSku(k.color, k.size)}</td>
                <td className="px-3 py-2">{makeBarcode(k.color, k.size) || "-"}</td>
                <td className="px-3 py-2">{price || 0}</td>
                <td className="px-3 py-2">{stock || 0}</td>
                <td className="px-3 py-2">{isActive ? "Evet" : "Hayır"}</td>
              </tr>
            )) : (
              <tr><td colSpan="7" className="px-3 py-4 text-gray-500">Önizlenecek kombinasyon yok.</td></tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="flex justify-end">
        <button
          className="px-4 py-2 border rounded hover:bg-gray-50"
          onClick={handleCreateAll}
          disabled={!combos.length}
        >
          Varyantları Oluştur
        </button>
      </div>
    </div>
  );
}
