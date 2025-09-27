// src/pages/Products/ProductImages.jsx
import React, { useEffect, useMemo, useState } from "react";
import { listProductImages, deleteProductImageLink } from "../../api/images";
import { useToast } from "../../ui-toast";

export default function ProductImages({ product, refreshKey = 0 }) {
    const { toast } = useToast();
    const [prodImages, setProdImages] = useState([]);
    const [byVariant, setByVariant] = useState({});
    const [loading, setLoading] = useState(false);

    const base = import.meta.env.VITE_API_BASE || "";
    const variants = product?.variants || [];

    const variantMap = useMemo(() => {
        const m = new Map();
        variants.forEach(v => m.set(v.id, v));
        return m;
    }, [variants]);

    // url birleştirme helper’ı
    function getImgSrc(url = "") {
        if (!url) return "";
        const u = String(url);
        if (u.startsWith("http://") || u.startsWith("https://")) return u; // absolute
        // api base sonunda slash varsa iki kez olmasın
        const left = base.endsWith("/") ? base.slice(0, -1) : base;
        const right = u.startsWith("/") ? u : `/${u}`;
        return left + right;
    }

    async function load() {
        if (!product?.id) return;
        setLoading(true);
        try {
            // 1) Ürün (variantId olmadan) görüntüler
            const p = await listProductImages(product.id);
            setProdImages(Array.isArray(p) ? p.filter(x => !x.productVariantId) : []);

            // 2) Varyant görüntülerini paralel çek
            const tasks = variants.map(v => listProductImages(product.id, { variantId: v.id }));
            const results = await Promise.allSettled(tasks);

            const dict = {};
            variants.forEach((v, idx) => {
                const r = results[idx];
                if (r.status === "fulfilled" && Array.isArray(r.value)) dict[v.id] = r.value;
                else dict[v.id] = [];
            });
            setByVariant(dict);
        } catch (e) {
            toast({ title: "Görseller yüklenemedi", description: e.message, variant: "destructive" });
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => { load(); /* eslint-disable-next-line */ }, [product?.id, refreshKey]);

    async function handleDelete(linkId) {
        try {
            await deleteProductImageLink(product.id, linkId);
            await load();
        } catch (e) {
            toast({ title: "Silinemedi", description: e.message, variant: "destructive" });
        }
    }

    function Grid({ items }) {
        if (!items?.length) {
            return (
                <div className="rounded-lg border border-dashed bg-gray-50 text-gray-500 text-sm p-6 text-center">
                    Henüz görsel yok.
                </div>
            );
        }
        return (
            <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-3">
                {items.map(img => {
                    // Key önceliği: linkId (benzersiz) → imageId → bileşik anahtar
                    const key =
                        img.linkId ??
                        img.id ??
                        img.imageId ??
                        `${img.productImageId || "img"}-${img.productVariantId || "p"}`;
                    const delId = img.linkId ?? img.id; // silme için link id
                    return (
                        <div key={key} className="relative group rounded-xl border overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow">
                            <img src={getImgSrc(img.url)} alt="" className="w-full h-28 object-cover" loading="lazy" />
                            <div className="px-2 py-1 text-[11px] text-gray-600 flex items-center justify-between bg-white/70">
                                <span className="inline-flex items-center gap-1">
                                    <span className="rounded px-1.5 py-0.5 bg-gray-100">#{img.sortOrder ?? 0}</span>
                                    {img.isMain && <span className="rounded px-1.5 py-0.5 bg-blue-100 text-blue-700">Ana</span>}
                                </span>
                                <button
                                    onClick={() => handleDelete(delId)}
                                    className="opacity-0 group-hover:opacity-100 text-red-600 hover:text-red-700 hover:underline transition-opacity"
                                >
                                    Sil
                                </button>

                            </div>
                        </div>
                    )
                })}
            </div>
        );
    }

    return (
        <div className="space-y-5">
            <div className="flex items-center justify-between">
                <h3 className="font-semibold">Görseller</h3>
                {loading && <span className="text-xs text-gray-500 animate-pulse">Yükleniyor…</span>}
            </div>

            <section>
                <div className="text-sm font-medium mb-2">Ürün</div>
                <Grid items={prodImages} />
            </section>

            {!!variants.length && (
                <section className="space-y-4">
                    <div className="text-sm font-medium">Varyantlar</div>
                    {variants.map(v => (
                        <div key={v.id} className="space-y-2">
                            <div className="text-xs text-gray-600">
                                <span className="font-medium">{v.color || "-"}</span>
                                {v.size ? <span className="ml-2">({v.size})</span> : null}
                            </div>
                            <Grid items={byVariant[v.id]} />
                        </div>
                    ))}
                </section>
            )}
        </div>
    );
}
