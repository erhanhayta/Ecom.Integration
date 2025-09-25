import React, { useEffect, useMemo, useState } from "react";
import { listProductImages, deleteProductImage } from "../../api/images";
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

    async function load() {
        if (!product?.id) return;
        setLoading(true);
        try {
            const p = await listProductImages(product.id);
            setProdImages(Array.isArray(p) ? p.filter(x => !x.productVariantId) : []);
            const dict = {};
            for (const v of variants) {
                const vi = await listProductImages(product.id, { variantId: v.id });
                dict[v.id] = Array.isArray(vi) ? vi : [];
            }
            setByVariant(dict);
        } catch (e) {
            toast({ title: "Görseller yüklenemedi", description: e.message, variant: "destructive" });
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => { load(); /* eslint-disable-next-line */ }, [product?.id, refreshKey]);

    async function handleDelete(imageId) {
        try {
            await deleteProductImage(product.id, imageId);
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
                {items.map(img => (
                    <div key={img.id} className="relative group rounded-xl border overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow">
                        <img src={base + img.url} alt="" className="w-full h-28 object-cover" loading="lazy" />
                        <div className="px-2 py-1 text-[11px] text-gray-600 flex items-center justify-between bg-white/70">
                            <span className="inline-flex items-center gap-1">
                                <span className="rounded px-1.5 py-0.5 bg-gray-100">#{img.sortOrder ?? 0}</span>
                                {img.isMain && <span className="rounded px-1.5 py-0.5 bg-blue-100 text-blue-700">Ana</span>}
                            </span>
                            <button onClick={() => handleDelete(img.id)}
                                className="opacity-0 group-hover:opacity-100 text-red-600 hover:underline">
                                Sil
                            </button>
                        </div>
                    </div>
                ))}
            </div>
        );
    }
    return (
        <div className="space-y-5">
            <div className="flex items-center justify-between">
                <h3 className="font-semibold">Görseller</h3>
                {loading && <span className="text-xs text-gray-500">Yükleniyor…</span>}
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
