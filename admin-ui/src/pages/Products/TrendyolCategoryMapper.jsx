// src/pages/Products/TrendyolCategoryMapper.jsx
import React, { useEffect, useMemo, useState } from "react";
import { getShops, getDefaultActiveTrendyolShop } from "../../api/shops";
import { getCategories, getCategoryAttributes } from "../../api/catalog";
import { useToast } from "../../ui-toast";

export default function TrendyolCategoryMapper({ product, onChange }) {
  const { toast } = useToast();

  // Shops
  const [shops, setShops] = useState([]);
  const [shop, setShop] = useState(null);
  const [shopId, setShopId] = useState("");

  // Kategori seviyeleri: [{ parentId, items, selectedId }]
  const [levels, setLevels] = useState([]);
  const [query, setQuery] = useState("");

  // Seçilen (en son) kategori + attributes
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [attributes, setAttributes] = useState([]);
  const [attrValues, setAttrValues] = useState({});

  const currentCategoryId = useMemo(() => {
    if (!levels.length) return "";
    const last = levels[levels.length - 1];
    return last.selectedId || "";
  }, [levels]);

  // Mağazalar + varsayılan aktif Trendyol
  useEffect(() => {
    (async () => {
      try {
        const list = await getShops();
        setShops(list || []);
        const def = await getDefaultActiveTrendyolShop();
        if (def) {
          setShop(def);
          setShopId(def.id);
        }
      } catch (e) {
        toast({ title: "Mağazalar yüklenemedi", description: e.message, variant: "destructive" });
      }
    })();
  }, []);

  // Shop değişti → kök kategoriler
  useEffect(() => {
    (async () => {
      if (!shop?.id) { setLevels([]); return; }
      try {
        const rootCats = await getCategories(shop.id, { parentId: null, q: query });
        setLevels([{ parentId: null, items: rootCats || [], selectedId: "" }]);
        setSelectedCategory(null);
        setAttributes([]);
        setAttrValues({});
      } catch (e) {
        toast({ title: "Kategoriler alınamadı", description: e.message, variant: "destructive" });
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shop?.id]);

  // Kök arama → kök seviyeyi filtrele
  useEffect(() => {
    (async () => {
      if (!shop?.id) return;
      try {
        const rootCats = await getCategories(shop.id, { parentId: null, q: query });
        setLevels([{ parentId: null, items: rootCats || [], selectedId: "" }]);
        setSelectedCategory(null);
        setAttributes([]);
        setAttrValues({});
      } catch { /* sessiz */ }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query]);

  // Seviye değişimi
  const onLevelChange = async (levelIndex, newCategoryId) => {
    if (!shop?.id) return;

    let nextLevels;
    let pickedCat = null;

    // 1) Bu seviyeye kadar tut, seçimi güncelle, alttakileri temizle
    setLevels(prev => {
      nextLevels = prev.slice(0, levelIndex + 1).map((lv, i) => {
        if (i !== levelIndex) return lv;
        const updated = { ...lv, selectedId: newCategoryId };
        pickedCat = (lv.items || []).find(x => String(x.id) === String(newCategoryId)) || null;
        return updated;
      });
      return nextLevels;
    });

    // 2) Seçili kategori state
    setSelectedCategory(pickedCat || null);

    // 3) Kategori seçilmediyse attributes sıfırla
    if (!pickedCat) {
      setAttributes([]);
      setAttrValues({});
      setLevels(prev => prev.slice(0, levelIndex + 1));
      return;
    }

    // --- KRİTİK: leaf === true → alt var, leaf === false → en alt ---
    const hasChildren = pickedCat.leaf === true || pickedCat.hasChildren === true;
    const isLeaf = !hasChildren;

    if (hasChildren) {
      // Çocukları getir ve yeni dropdown ekle
      try {
        const children = await getCategories(shop.id, { parentId: pickedCat.id, q: "" });
        setAttributes([]);
        setAttrValues({});
        setLevels(prev => {
          const base = prev.slice(0, levelIndex + 1);
          return [...base, { parentId: pickedCat.id, items: children || [], selectedId: "" }];
        });
      } catch (e) {
        toast({ title: "Alt kategoriler alınamadı", description: e.message, variant: "destructive" });
        setLevels(prev => prev.slice(0, levelIndex + 1));
      }
    } else if (isLeaf) {
      // En alt seviye → attributes çek
      try {
        const attrs = await getCategoryAttributes(shop.id, Number(pickedCat.id));
        setAttributes(attrs || []);
        const init = {};
        (attrs || []).forEach(a => { init[a.attributeId] = ""; });
        setAttrValues(init);
      } catch {
        setAttributes([]);
        setAttrValues({});
      }
      // Daha alt seviye yok
      setLevels(prev => prev.slice(0, levelIndex + 1));
    }
  };

  // Shop combobox değişimi
  function onShopChange(e) {
    const id = e.target.value;
    setShopId(id);
    const s = shops.find(x => String(x.id) === String(id)) || null;
    setShop(s);
  }

  function changeAttr(attrId, value) {
    setAttrValues(prev => ({ ...prev, [attrId]: value }));
  }

  const missingRequired = useMemo(() => {
    return (attributes || []).some(a => a.required && !attrValues[a.attributeId]);
  }, [attributes, attrValues]);

  useEffect(() => {
    onChange?.({
      shop,
      selectedCategory,
      selectedCategoryId: currentCategoryId,
      attributes,
      attrValues,
      missingRequired
    });
  }, [shop, selectedCategory, currentCategoryId, attributes, attrValues, missingRequired, onChange]);

  const hasChildrenSelected = selectedCategory
    ? (selectedCategory.leaf === true || selectedCategory.hasChildren === true)
    : false;
  const isLeafSelected = selectedCategory ? !hasChildrenSelected : false;

  return (
    <div className="space-y-4">
      <div className="text-lg font-semibold">Trendyol Kategori & Özellik</div>

      {/* Mağaza combobox (ShopName göster) */}
      <div>
        <label className="block text-sm mb-1 text-gray-600">Aktif Trendyol Mağazası</label>
        <select className="w-full border rounded px-3 py-2" value={shopId} onChange={onShopChange}>
          <option value="">Seçiniz…</option>
          {(shops || [])
            .filter(s => s.firm === 1)
            .map(s => (
              <option key={s.id} value={s.id}>
                {s.shopName || s.integrationRefCode || s.id}{s.isActive ? " (aktif)" : ""}
              </option>
            ))}
        </select>
        <div className="text-xs text-gray-500 mt-1">
          Varsayılan aktif Trendyol mağazası otomatik gelir; istersen değiştir.
        </div>
      </div>

      {/* Kök arama */}
      <input
        className="border rounded px-3 py-2 w-full"
        placeholder="Kök kategoride ara…"
        value={query}
        onChange={e => setQuery(e.target.value)}
        disabled={!shop?.id}
      />

      {/* Çok kademeli dropdown’lar */}
      <div className="grid gap-3">
        {levels.map((lv, i) => (
          <div key={i}>
            <label className="block text-sm mb-1 text-gray-600">
              {i === 0 ? "Kategori" : `Alt Kategori ${i}`}
            </label>
            <select
              className="w-full border rounded px-3 py-2"
              value={lv.selectedId}
              onChange={e => onLevelChange(i, e.target.value)}
              disabled={!lv.items.length}
            >
              <option value="">Seçiniz…</option>
              {lv.items.map(c => {
                const hasChildren = c.leaf === true || c.hasChildren === true;
                return (
                  <option key={c.id} value={c.id}>
                    {c.name} {hasChildren ? " (alt var)" : ""}
                  </option>
                );
              })}
            </select>
          </div>
        ))}
      </div>

      {/* Attributes: sadece en alt (leaf=false) seçildiğinde */}
      <div className="border rounded-lg overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="text-left p-3">Attribute</th>
              <th className="text-left p-3">Flags</th>
              <th className="text-left p-3">Değer</th>
            </tr>
          </thead>
        <tbody>
          {currentCategoryId && isLeafSelected ? (
            (attributes || []).length ? (attributes || []).map(a => (
              <tr key={a.attributeId} className="border-t">
                <td className="p-3">
                  <div className="font-medium">{a.name}</div>
                  <div className="text-xs text-gray-500">ID: {a.attributeId}</div>
                </td>
                <td className="p-3">
                  <span className={`inline-block px-2 py-0.5 rounded text-xs mr-1 ${a.required?'bg-red-100 text-red-700':'bg-gray-100 text-gray-700'}`}>required</span>
                  <span className={`inline-block px-2 py-0.5 rounded text-xs mr-1 ${a.allowCustom?'bg-green-100 text-green-700':'bg-gray-100 text-gray-700'}`}>allowCustom</span>
                  <span className={`inline-block px-2 py-0.5 rounded text-xs mr-1 ${a.varianter?'bg-blue-100 text-blue-700':'bg-gray-100 text-gray-700'}`}>varianter</span>
                  <span className={`inline-block px-2 py-0.5 rounded text-xs ${a.slicer?'bg-purple-100 text-purple-700':'bg-gray-100 text-gray-700'}`}>slicer</span>
                </td>
                <td className="p-3">
                  {a.values?.length ? (
                    <select
                      className="border rounded px-2 py-1"
                      value={attrValues[a.attributeId] || ""}
                      onChange={e => changeAttr(a.attributeId, e.target.value)}
                    >
                      <option value="">Seçin…{a.required ? " (zorunlu)" : ""}</option>
                      {a.values.map(v => (
                        <option key={v.id} value={v.id}>{v.name}</option>
                      ))}
                    </select>
                  ) : (
                    a.allowCustom
                      ? <input
                          className="border rounded px-2 py-1"
                          placeholder="Serbest metin"
                          value={attrValues[a.attributeId] || ""}
                          onChange={e => changeAttr(a.attributeId, e.target.value)}
                        />
                      : <span className="text-gray-400">Değer yok</span>
                  )}
                </td>
              </tr>
            )) : (
              <tr><td colSpan="3" className="p-4 text-gray-500">Bu kategori için özellik bulunamadı.</td></tr>
            )
          ) : (
            <tr><td colSpan="3" className="p-4 text-gray-500">
              {currentCategoryId
                ? "Alt kategori seçmeye devam edin; en alt seviyeye indiğinizde özellikler gelir."
                : "Kategori seçiniz."}
            </td></tr>
          )}
        </tbody>
        </table>
      </div>

      {(attributes || []).length > 0 && (attributes || []).some(a => a.required && !attrValues[a.attributeId]) && (
        <div className="text-sm text-red-600 mt-2">Zorunlu alanları doldurmadan kaydedemezsiniz.</div>
      )}
    </div>
  );
}
