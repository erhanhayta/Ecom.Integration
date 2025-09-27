// src/pages/Products/TrendyolCategoryMapper.jsx
import React, { useEffect, useMemo, useState } from "react";
import { useToast } from "../../ui-toast";
import {
  listShops,
  getTyCategories,
  getTyCategoryAttributes,
  getTyConfig,
  saveTyConfig,
  mapTyVariants,
} from "../../api/ty";

export default function TrendyolCategoryMapper({ product }) {
  const { toast } = useToast();

  const [shops, setShops] = useState([]);
  const [shopId, setShopId] = useState("");

  // drilldown
  const [levels, setLevels] = useState([]);        // her seviye için kategori listesi
  const [selectedIds, setSelectedIds] = useState([]); // her seviyede seçili kategori id

  // attributes
  const [attrs, setAttrs] = useState([]);
  const [attrsLoading, setAttrsLoading] = useState(false);

  const [saving, setSaving] = useState(false);
  const [mapping, setMapping] = useState(false);

  // Mağazaları yükle
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

  // shop değişince: kökü yükle + varsa config.categoryPath ile seviyeleri geri kur
  useEffect(() => {
    if (!shopId || !product?.id) return;
    (async () => {
      try {
        // 1) root seviyeyi yükle
        const root = await getTyCategories(shopId);
        setLevels([normalize(root)]);
        setSelectedIds([]);
        setAttrs([]);

        // 2) kayıtlı config varsa path'le doldur
        const cfg = await getTyConfig(shopId, product.id); // { categoryId, categoryName, categoryPath, attributes:[...] }
        const path = (cfg?.categoryPath || "").trim();
        if (!path) return;

        const ids = path.split("/").map(n => Number(n)).filter(Boolean);
        if (!ids.length) return;

        let parentId = null;
        const newLevels = [];
        const newSelected = [];

        for (const cid of ids) {
          const list = await getTyCategories(shopId, { parentId });
          const norm = normalize(list);
          newLevels.push(norm);
          newSelected.push(cid);
          parentId = cid;
        }

        setLevels(newLevels);
        setSelectedIds(newSelected);

        // 3) leaf attributes’ı çek + kayıtlı seçimleri uygula + zorunluları üste sırala
        const leafId = ids[ids.length - 1];
        await loadAttributes(leafId);

        if (Array.isArray(cfg.attributes) && cfg.attributes.length) {
          setAttrs(prev => {
            const merged = prev.map(p => {
              const saved = cfg.attributes.find(a => a.attributeId === p.id);
              return {
                ...p,
                selectedValueId: p.allowCustom ? undefined : (saved?.valueId ?? undefined),
                inputValue: p.allowCustom ? (saved?.valueName ?? "") : (p.inputValue || "")
              };
            });
            merged.sort((a, b) => Number(b.required) - Number(a.required));
            return merged;
          });
        }
      } catch (e) {
        console.warn("Init mapper failed", e);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shopId, product?.id]);

  function normalize(list) {
    return (list || []).map(c => ({
      id: c.id,
      name: c.name || c.displayName || c.Name,
      hasChildren:
        !!c.hasChildren ||
        ((c.subCategories?.length ?? c.SubCategories?.length ?? 0) > 0),
    }));
  }

  const selectedCategory = useMemo(() => {
    if (!selectedIds.length) return { id: null, name: "", hasChildren: false };
    const lvl = levels[selectedIds.length - 1] || [];
    const node = lvl.find(x => x.id === selectedIds[selectedIds.length - 1]);
    return node || { id: null, name: "", hasChildren: false };
  }, [levels, selectedIds]);

  async function onSelect(levelIndex, id) {
    const nextSel = selectedIds.slice(0, levelIndex);
    nextSel[levelIndex] = id;
    setSelectedIds(nextSel);
    setAttrs([]);

    const children = await getTyCategories(shopId, { parentId: id });
    const norm = normalize(children);
    if (norm.length) {
      const nextLevels = levels.slice(0, levelIndex + 1);
      nextLevels[levelIndex + 1] = norm;
      setLevels(nextLevels);
    } else {
      // leaf: alt seviye yok → attributes yükle
      setLevels(levels.slice(0, levelIndex + 1));
      await loadAttributes(id);
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
      list.sort((a, b) => Number(b.required) - Number(a.required)); // zorunlular üste
      setAttrs(list);
    } finally {
      setAttrsLoading(false);
    }
  }

  async function handleSave() {
    const sel = selectedCategory;
    if (!sel.id) { toast({ title: "Kategori seçin", variant: "destructive" }); return; }
    setSaving(true);
    try {
      // PATH'i üret
      const path = selectedIds.join("/");

      await saveTyConfig(shopId, product.id, {
        categoryId: sel.id,
        categoryName: sel.name,
        selectedPath: path, // <-- önemli
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

  async function handleMap() {
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
      {/* shop */}
      <div className="rounded-xl border bg-white/60 p-4">
        <label className="text-sm text-gray-600">Mağaza</label>
        <select className="border rounded px-3 py-2 w-full md:w-96"
          value={shopId} onChange={e => { setShopId(e.target.value); }}>
          {shops.map(s => <option key={s.id} value={s.id}>
            {(s.shopName || s.name || s.id) + (s.isActive ? " (Aktif)" : "")}
          </option>)}
        </select>
      </div>

      {/* drilldown */}
      <div className="rounded-xl border bg-white/60 p-4">
        <div className="font-medium mb-3">Kategori Seçimi</div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {levels.map((list, idx) => (
            <div key={idx}>
              <label className="text-xs text-gray-500 mb-1 block">Seviye {idx + 1}</label>
              <select className="border rounded px-3 py-2 w-full"
                value={selectedIds[idx] || ""} onChange={e => onSelect(idx, Number(e.target.value))}>
                <option value="">Seçiniz</option>
                {list.map(c => <option key={c.id} value={c.id}>
                  {c.name}{c.hasChildren ? " ›" : ""}
                </option>)}
              </select>
            </div>
          ))}
        </div>
        <div className="mt-2 text-xs text-gray-500">Seçili: <b>{selectedCategory.name || "-"}</b></div>
      </div>

      {/* attributes */}
      <div className="rounded-xl border bg-white/60 p-4">
        <div className="flex items-center justify-between mb-3">
          <div className="font-medium">Kategori Özellikleri</div>
          {attrsLoading && <div className="text-xs text-blue-600 animate-pulse">Yükleniyor…</div>}
        </div>

        {!attrs.length ? (
          <div className="text-sm text-gray-500">Leaf kategori seçince burada görünecek.</div>
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
                    <select className="border rounded px-3 py-2 w-full md:w-96"
                      value={a.selectedValueId ?? ""} onChange={e => {
                        const v = e.target.value ? Number(e.target.value) : undefined;
                        setAttrs(prev => prev.map((x, idx) => idx === i ? { ...x, selectedValueId: v } : x));
                      }}>
                      <option value="">Seçiniz</option>
                      {(a.values || []).map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
                    </select>
                  ) : (
                    <input className="border rounded px-3 py-2 w-full md:w-96"
                      value={a.inputValue || ""} onChange={e => {
                        const t = e.target.value;
                        setAttrs(prev => prev.map((x, idx) => idx === i ? { ...x, inputValue: t } : x));
                      }} placeholder="Serbest metin" />
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* actions */}
      <div className="flex justify-end gap-3">
        <button onClick={handleMap} disabled={mapping}
          className={`px-4 py-2 rounded text-white ${mapping ? "bg-indigo-400" : "bg-indigo-600 hover:bg-indigo-700"}`}>
          {mapping ? "Eşleştiriliyor…" : "Varyantları Eşleştir"}
        </button>
        <button onClick={handleSave} disabled={saving || !selectedCategory.id}
          className={`px-4 py-2 rounded text-white ${saving ? "bg-blue-400" : "bg-blue-600 hover:bg-blue-700"}`}>
          {saving ? "Kaydediliyor…" : "Kaydet"}
        </button>
      </div>
    </div>
  );
}
