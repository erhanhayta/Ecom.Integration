// src/pages/Products/VariantImageBulkAdd.jsx
import React, { useMemo, useState } from "react";
import {
  uploadProductImageFile,
  uploadProductImageFilesBulk,
} from "../../api/images";
import { useToast } from "../../ui-toast";

export default function VariantImageBulkAdd({ product, onCreated }) {
  const { toast } = useToast();

  const variants = product?.variants || [];
  const colors = useMemo(() => {
    const set = new Set(
      variants.map((v) => (v.color || "").trim()).filter(Boolean)
    );
    return Array.from(set);
  }, [variants]);

  const [files, setFiles] = useState([]);
  const [targetProduct, setTargetProduct] = useState(true);
  const [targetVariants, setTargetVariants] = useState(true);
  const [variantScope, setVariantScope] = useState("all"); // 'all' | 'byColor'
  const [selectedColor, setSelectedColor] = useState(colors[0] || "");

  const [isMain, setIsMain] = useState(false); // ürün için
  const [sortOrder, setSortOrder] = useState(0);
  const [isUploading, setIsUploading] = useState(false);

  function onFileChange(e) {
    setFiles(Array.from(e.target.files || []));
  }

  function variantIdsByScope() {
    if (!targetVariants) return [];
    if (variantScope === "byColor" && selectedColor) {
      return variants
        .filter(
          (v) =>
            (v.color || "").trim().toLowerCase() ===
            selectedColor.trim().toLowerCase()
        )
        .map((v) => v.id);
    }
    return variants.map((v) => v.id);
  }

  async function handleUpload() {
    if (!product?.id) {
      toast({ title: "Ürün bulunamadı", variant: "destructive" });
      return;
    }
    if (!files.length) {
      toast({ title: "Dosya seçin", variant: "destructive" });
      return;
    }
    if (!targetProduct && !targetVariants) {
      toast({
        title: "Hedef seçiniz",
        description: "Ürüne ve/veya varyantlara ekleyin.",
        variant: "destructive",
      });
      return;
    }

    let ok = 0,
      fail = 0;
    setIsUploading(true);

    try {
      // 1) ÜRÜNE bulk yükleme
      if (targetProduct) {
        try {
          await uploadProductImageFilesBulk(product.id, files, {
            sortOrder,
            isMain,
          });
          ok += files.length;
        } catch (e) {
          fail += files.length;
          console.error("bulk product upload failed", e);
        }
      }

      // 2) VARYANTLARA yükleme (tek tek)
      const vIds = variantIdsByScope();
      if (targetVariants && vIds.length) {
        for (const vid of vIds) {
          for (const f of files) {
            try {
              await uploadProductImageFile(product.id, f, {
                variantId: vid,
                sortOrder,
                isMain: false, // varyant için ana bayrağı kapalı bırakıyoruz
              });
              ok++;
            } catch (e) {
              fail++;
              console.error("variant upload failed", { vid, f }, e);
            }
          }
        }
      }
    } finally {
      setIsUploading(false);
      if (ok)
        toast({
          title: `${ok} kayıt eklendi`,
          description: fail ? `${fail} tanesi hatalı.` : undefined,
        });
      if (typeof onCreated === "function") onCreated(ok);
      setFiles([]);
    }
  }

  return (
    <div className="space-y-4">
      <div className="text-lg font-semibold flex items-center justify-between">
        <span>Toplu Görsel Ekle</span>
        {isUploading && (
          <span className="text-xs text-blue-600 animate-pulse">Yükleniyor…</span>
        )}
      </div>

      {/* Kaynak */}
      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Dosyalar</label>
          <input
            type="file"
            multiple
            onChange={onFileChange}
            className="block w-full text-sm file:mr-4 file:py-1.5 file:px-3 file:rounded-md file:border-0 file:bg-blue-600 file:text-white hover:file:bg-blue-700 cursor-pointer"
          />
          {!!files.length && (
            <div className="text-xs text-gray-500 mt-1">
              {files.length} dosya seçildi.
            </div>
          )}
        </div>

        {/* Hedef */}
        <div className="col-span-12 md:col-span-6">
          <label className="block text-sm text-gray-600 mb-1">Hedef</label>
          <div className="flex items-center gap-4">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={targetProduct}
                onChange={(e) => setTargetProduct(e.target.checked)}
              />
              Ürüne ekle
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={targetVariants}
                onChange={(e) => setTargetVariants(e.target.checked)}
              />
              Varyantlara ekle
            </label>
          </div>

          {targetVariants && (
            <div className="mt-3 flex flex-wrap items-center gap-3">
              <select
                className="border rounded px-3 py-2 focus:ring-2 focus:ring-blue-500"
                value={variantScope}
                onChange={(e) => setVariantScope(e.target.value)}
              >
                <option value="all">Tüm varyantlar</option>
                <option value="byColor">Sadece renge göre</option>
              </select>

              {variantScope === "byColor" && (
                <select
                  className="border rounded px-3 py-2 w-full sm:w-auto focus:ring-2 focus:ring-blue-500"
                  value={selectedColor}
                  onChange={(e) => setSelectedColor(e.target.value)}
                >
                  {colors.length ? (
                    colors.map((c) => (
                      <option key={c} value={c}>
                        {c}
                      </option>
                    ))
                  ) : (
                    <option value="">Renk yok</option>
                  )}
                </select>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Özellikler */}
      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-6 md:col-span-2">
          <label className="block text-sm text-gray-600 mb-1">Sıra</label>
          <input
            type="number"
            className="w-full border rounded px-3 py-2 focus:ring-2 focus:ring-blue-500"
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value)}
          />
        </div>
        <div className="col-span-6 md:col-span-4 flex items-end">
          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={isMain}
              onChange={(e) => setIsMain(e.target.checked)}
            />
            Ürün için ana görsel yap
          </label>
        </div>
        {targetVariants && (
          <div className="col-span-12 md:col-span-6 text-sm text-gray-500 flex items-end">
            {variantScope === "byColor" ? (
              <>
                Seçilen renk: <b className="ml-1">{selectedColor || "-"}</b> 
                :{" "}
                <b className="ml-1">
                  {
                    variants.filter(
                      (v) =>
                        (v.color || "").trim().toLowerCase() ===
                        (selectedColor || "").trim().toLowerCase()
                    ).length
                  }
                </b>
              </>
            ) : (
              <>
                Tüm varyantlara eklenecek:{" "}
                <b className="ml-1">{variants.length}</b>
              </>
            )}
          </div>
        )}
      </div>

      <div className="flex justify-end">
        <button
          className={`inline-flex items-center gap-2 px-4 py-2 rounded-md text-white ${
            isUploading ? "bg-blue-400 cursor-wait" : "bg-blue-600 hover:bg-blue-700"
          } shadow-sm`}
          onClick={handleUpload}
          disabled={!files.length || isUploading}
        >
          {isUploading && (
            <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              />
              <path
                className="opacity-75"
                d="M4 12a8 8 0 018-8v4"
                stroke="currentColor"
                strokeWidth="4"
              />
            </svg>
          )}
          {isUploading ? "Yükleniyor..." : "Görselleri Ekle"}
        </button>
      </div>
    </div>
  );
}
