import React, { useEffect, useMemo, useState } from "react";
import { useToast } from "../../ui-toast";
import {
    listShops,
    getTyCategories,            // getTyCategories(shopId, { parentId })
    getTyCategoryAttributes,
    saveTyConfig,
    mapTyVariants,
} from "../../api/ty";

export default function TrendyolCategoryMapper({ product }) {
    const { toast } = useToast();

    // shops
    const [shops, setShops] = useState([]);
    const [shopId, setShopId] = useState("");

    // drill-down state
    const [levels, setLevels] = useState([]);      // her seviye: [{id,name,hasChildren}]
    const [selectedIds, setSelectedIds] = useState([]);

    // attributes
    const [attrs, setAttrs] = useState([]);
    const [attrsLoading, setAttrsLoading] = useState(false);

    const [saving, setSaving] = useState(false);
    const [mapping, setMapping] = useState(false);

    // mağazaları yükle
    useEffect(() => {
        (async () => {
            try {
                const s = await listShops();
                setShops(s || []);
                const def = (s || []).find(x => x.isActive) || (s || [])[0];
                if (def) setShopId(def.id);
            } catch (e) {
                toast({ title: "Mağazalar yüklenemedi", description: e.message, variant: "destructive" });
            }
        })();
    }, []);

    // shop değişince root kategorileri yükle
    useEffect(() => {
        if (!shopId) return;
        (async () => {
            try {
                const root = await getTyCategories(shopId); // parentId verilmez → kök
                setLevels([normalizeCats(root)]);
                setSelectedIds([]);
                setAttrs([]);
            } catch (e) {
                toast({ title: "Kategoriler yüklenemedi", description: e.message, variant: "destructive" });
            }
        })();
        // eslint-disable-next-line
    }, [shopId]);

    // helpers
    function normalizeCats(list) {
        return (list || []).map(c => ({
            id: c.id,
            name: c.name || c.displayName || c.Name,
            hasChildren: !!c.hasChildren || (c.subCategories?.length ?? c.SubCategories?.length ?? 0) > 0
        }));
    }

    const selectedCategory = useMemo(() => {
        if (!selectedIds.length) return { id: null, name: "" };
        const lastLevel = levels[selectedIds.length - 1] || [];
        const node = lastLevel.find(x => x.id === selectedIds[selectedIds.length - 1]);
        return node ? { id: node.id, name: node.name, hasChildren: node.hasChildren } : { id: null, name: "" };
    }, [selectedIds, levels]);

    // seviye seçimi
    async function onSelect(levelIndex, id) {
        // bu seviyeyi güncelle, alt seviyeleri sıfırla
        const nextSel = selectedIds.slice(0, levelIndex);
        nextSel[levelIndex] = id;
        setSelectedIds(nextSel);
        setAttrs([]);

        // bir alt seviye kategorilerini al
        try {
            const children = await getTyCategories(shopId, { parentId: id });
            const norm = normalizeCats(children);

            if (norm.length > 0) {
                // alt seviye var → yeni dropdown ekle
                const nextLevels = levels.slice(0, levelIndex + 1);
                nextLevels[levelIndex + 1] = norm;
                setLevels(nextLevels);
            } else {
                // leaf → levels’ı bu seviyeye kadar bırak ve attributes çek
                setLevels(levels.slice(0, levelIndex + 1));
                await loadAttributes(id);
            }
        } catch (e) {
            toast({ title: "Alt kategori yüklenemedi", description: e.message, variant: "destructive" });
        }
    }

    async function loadAttributes(categoryId) {
        if (!shopId || !categoryId) return;
        setAttrsLoading(true);
        try {
            const res = await getTyCategoryAttributes(shopId, categoryId);
            const list = (res || []).map(r => ({
                id: r.attributeId,
                name: r.name,
                required: r.required,
                varianter: r.varianter,
                allowCustom: r.allowCustom,
                slicer: r.slicer,
                values: r.values || [],
                selectedValueId: undefined,
                inputValue: ""
            }));
            setAttrs(list);
        } catch (e) {
            toast({ title: "Özellikler yüklenemedi", description: e.message, variant: "destructive" });
        } finally {
            setAttrsLoading(false);
        }
    }

    async function handleSave() {
        const sel = selectedCategory;
        if (!sel.id) {
            toast({ title: "Kategori seçin", variant: "destructive" });
            return;
        }
        setSaving(true);
        try {
            await saveTyConfig(shopId, product.id, {
                categoryId: sel.id,
                categoryName: sel.name,
                attributes: attrs.map(a => ({
                    attributeId: a.id,
                    attributeName: a.name,
                    required: a.required,
                    varianter: a.varianter,
                    allowCustom: a.allowCustom,
                    slicer: a.slicer,
                    valueId: a.allowCustom ? null : (a.selectedValueId ?? null),
                    valueName: a.allowCustom ? (a.inputValue || null) : null
                }))
            });
            toast({ title: "Kaydedildi" });
        } catch (e) {
            toast({ title: "Kaydedilemedi", description: e.message, variant: "destructive" });
        } finally {
            setSaving(false);
        }
    }

    async function handleMapVariants() {
        setMapping(true);
        try {
            await mapTyVariants(shopId, product.id);
            toast({ title: "Varyantlar eşleştirildi" });
        } catch (e) {
            toast({ title: "Hata", description: e.message, variant: "destructive" });
        } finally {
            setMapping(false);
        }
    }

    return (
        <div className="space-y-6">
            {/* mağaza */}
            <div className="rounded-xl border bg-white/60 p-4">
                <label className="block text-sm text-gray-600 mb-1">Mağaza</label>
                <select
                    className="border rounded px-3 py-2 w-full md:w-96"
                    value={shopId}
                    onChange={(e) => setShopId(e.target.value)}
                >
                    {shops.map(s => (
                        <option key={s.id} value={s.id}>
                            {s.shopName || s.name || s.id}
                            {s.isActive ? " (Aktif)" : ""}
                        </option>
                    ))}
                </select>
            </div>

            {/* drill-down */}
            <div className="rounded-xl border bg-white/60 p-4">
                <div className="font-medium mb-3">Kategori Seçimi</div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                    {levels.map((list, idx) => (
                        <div key={idx} className="flex flex-col">
                            <label className="text-xs text-gray-500 mb-1">Seviye {idx + 1}</label>
                            <select
                                className="border rounded px-3 py-2"
                                value={selectedIds[idx] || ""}
                                onChange={(e) => onSelect(idx, Number(e.target.value))}
                            >
                                <option value="">Seçiniz</option>
                                {list.map(c => (
                                    <option key={c.id} value={c.id}>
                                        {c.name}{c.hasChildren ? " ›" : ""}
                                    </option>
                                ))}
                            </select>
                        </div>
                    ))}
                </div>
                <div className="mt-2 text-xs text-gray-500">
                    Seçili: <b>{selectedCategory.name || "-"}</b>
                </div>
            </div>

            {/* attributes */}
            <div className="rounded-xl border bg-white/60 p-4">
                <div className="flex items-center justify-between mb-3">
                    <div className="font-medium">Kategori Özellikleri</div>
                    {attrsLoading && <div className="text-xs text-blue-600 animate-pulse">Yükleniyor…</div>}
                </div>

                {!attrs.length ? (
                    <div className="text-sm text-gray-500">Leaf kategori seçtiğinizde özellikler burada görünecek.</div>
                ) : (
                    <div className="space-y-3">
                        {attrs.map((a, i) => (
                            <div key={a.id} className="p-3 rounded border bg-white">
                                <div className="flex items-center justify-between">
                                    <div className="font-medium">{a.name}</div>
                                    <div className="flex gap-1 text-[11px]">
                                        {a.required && <span className="px-1.5 py-0.5 rounded bg-rose-100 text-rose-700">Zorunlu</span>}
                                        {a.varianter && <span className="px-1.5 py-0.5 rounded bg-blue-100 text-blue-700">Varianter</span>}
                                        {a.allowCustom && <span className="px-1.5 py-0.5 rounded bg-emerald-100 text-emerald-700">Custom</span>}
                                    </div>
                                </div>

                                <div className="mt-2">
                                    {!a.allowCustom ? (
                                        <select
                                            className="border rounded px-3 py-2 w-full md:w-96"
                                            value={a.selectedValueId ?? ""}
                                            onChange={(e) => {
                                                const v = e.target.value ? Number(e.target.value) : undefined;
                                                setAttrs(prev => prev.map((x, idx) => idx === i ? { ...x, selectedValueId: v } : x));
                                            }}
                                        >
                                            <option value="">Seçiniz</option>
                                            {(a.values || []).map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
                                        </select>
                                    ) : (
                                        <input
                                            className="border rounded px-3 py-2 w-full md:w-96"
                                            value={a.inputValue || ""}
                                            onChange={(e) => {
                                                const t = e.target.value;
                                                setAttrs(prev => prev.map((x, idx) => idx === i ? { ...x, inputValue: t } : x));
                                            }}
                                            placeholder="Serbest metin"
                                        />
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* actions */}
            <div className="flex justify-end gap-3">
                <button
                    onClick={handleMapVariants}
                    disabled={mapping}
                    className={`px-4 py-2 rounded text-white ${mapping ? "bg-indigo-400" : "bg-indigo-600 hover:bg-indigo-700"}`}
                >
                    {mapping ? "Eşleştiriliyor…" : "Varyantları Eşleştir"}
                </button>
                <button
                    onClick={handleSave}
                    disabled={saving || !selectedCategory.id}
                    className={`px-4 py-2 rounded text-white ${saving ? "bg-blue-400" : "bg-blue-600 hover:bg-blue-700"}`}
                >
                    {saving ? "Kaydediliyor…" : "Kaydet"}
                </button>
            </div>
        </div>
    );
}
