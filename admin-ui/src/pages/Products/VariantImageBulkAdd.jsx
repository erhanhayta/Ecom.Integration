import React, { useMemo, useState } from "react";
import {
  uploadProductImageFilesBulk,
  linkImages
} from "../../api/images";
import { useToast } from "../../ui-toast";

export default function VariantImageBulkAdd({ product, onCreated }) {
  const t = useToast();
  // Hem { push } dönen, hem direkt fonksiyon dönen hook'ları destekle
  const push = typeof t === "function" ? t : (t?.push || (() => { }));
  const variants = product?.variants || [];
  const colors = useMemo(() => {
    const set = new Set(variants.map(v => (v.color || "").trim()).filter(Boolean));
    return Array.from(set);
  }, [variants]);

  const [files, setFiles] = useState([]);
  const [targetProduct, setTargetProduct] = useState(true);
  const [targetVariants, setTargetVariants] = useState(true);
  const [variantScope, setVariantScope] = useState("all"); // 'all' | 'byColor'
  const [selectedColor, setSelectedColor] = useState(colors[0] || "");
  const [isMain, setIsMain] = useState(false);
  const [sortOrder, setSortOrder] = useState(0);
  const [isUploading, setIsUploading] = useState(false);

  function onFileChange(e) { setFiles(Array.from(e.target.files || [])); }

  function variantIdsByScope() {
    if (!targetVariants) return [];
    if (variantScope === "byColor" && selectedColor) {
      return variants
        .filter(v => (v.color || "").trim().toLowerCase() === selectedColor.trim().toLowerCase())
        .map(v => v.id);
    }
    return variants.map(v => v.id);
  }

  async function handleUpload() {
    if (!product?.id) { push("Ürün bulunamadı", "error"); return; }
    if (!files.length) { push("Dosya seçin", "error"); return; }
    if (!targetProduct && !targetVariants) {
      push("Hedef seçiniz: Ürüne ve/veya varyantlara ekleyin.", "error");
      return;
    }

    setIsUploading(true);
    try {
      // 1) DOSYALARI YÜKLE → imageId listesi al
      const imageIds = await uploadProductImageFilesBulk(product.id, files, {
        sortOrder,
        isMain: isMain && targetProduct,   // ana görsel sadece üründe mantıklı
        linkToProduct: false               // ilk upload'ta link oluşturma → linkImages ile yöneteceğiz
      });

      // 2) LINK OLUŞTUR
      const vIds = variantIdsByScope();
      await linkImages(product.id, {
        imageIds,
        linkToProduct: targetProduct,
        variantIds: vIds,
        sortOrder,
        isMain
      });

      push(`${imageIds.length} görsel bağlandı`);
      onCreated && onCreated(imageIds.length);
      setFiles([]);
    } catch (e) {
      push(`Hata: ${e.message || e}`, "error");
    } finally {
      setIsUploading(false);
    }
  }

  return (
    <div className="space-y-4">
      <div className="text-lg font-semibold flex items-center justify-between">
        <span>Toplu Görsel Ekle</span>
        {isUploading && <span className="text-xs text-blue-600 animate-pulse">Yükleniyor…</span>}
      </div>

      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Dosyalar</label>
          <input type="file" multiple onChange={onFileChange} className="block w-full text-sm" />
          {!!files.length && <div className="text-xs text-gray-500 mt-1">{files.length} dosya seçildi.</div>}
        </div>

        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Hedef</label>
          <div className="flex items-center gap-4">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={targetProduct} onChange={e => setTargetProduct(e.target.checked)} />
              Ürüne ekle
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={targetVariants} onChange={e => setTargetVariants(e.target.checked)} />
              Varyantlara ekle
            </label>
          </div>

          {targetVariants && (
            <div className="mt-2 flex flex-wrap items-center gap-3">
              <select className="border rounded px-3 py-2" value={variantScope} onChange={e => setVariantScope(e.target.value)}>
                <option value="all">Tüm varyantlar</option>
                <option value="byColor">Sadece renge göre</option>
              </select>

              {variantScope === "byColor" && (
                <select className="border rounded px-3 py-2 w-full sm:w-auto" value={selectedColor} onChange={e => setSelectedColor(e.target.value)}>
                  {colors.length ? colors.map(c => <option key={c} value={c}>{c}</option>) : <option value="">Renk yok</option>}
                </select>
              )}
            </div>
          )}
        </div>
      </div>

      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-6 md:col-span-2">
          <label className="block text-sm text-gray-600 mb-1">Sıra</label>
          <input type="number" className="w-full border rounded px-3 py-2" value={sortOrder} onChange={e => setSortOrder(Number(e.target.value))} />
        </div>
        <div className="col-span-6 md:col-span-4">
          <label className="flex items-center gap-2 text-sm mt-6">
            <input type="checkbox" checked={isMain} onChange={e => setIsMain(e.target.checked)} />
            Ürün için ana görsel yap
          </label>
        </div>
      </div>

      <div className="flex justify-end">
        <button
          className="px-4 py-2 rounded bg-gray-900 text-white hover:bg-gray-800 disabled:opacity-50"
          onClick={handleUpload}
          disabled={!files.length || isUploading}
        >
          Görselleri Ekle
        </button>
      </div>
    </div>
  );
}
