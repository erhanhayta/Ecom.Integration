import React, { useEffect, useMemo, useState } from "react";
import WindsterLayout from "../../components/WindsterLayout";
import { getCategories, getCategoryAttributes } from "../../api/catalog";
import { useToast } from "../../ui-toast";

export default function CategoryExplorer() {
  const { toast } = useToast();
  const [shopId, setShopId] = useState(""); // Shops/List’deki kayıtlı mağazalardan seçilecek
  const [parentId, setParentId] = useState(null);
  const [query, setQuery] = useState("");
  const [cats, setCats] = useState([]);
  const [path, setPath] = useState([]);     // breadcrumb
  const [selected, setSelected] = useState(null);
  const [attrs, setAttrs] = useState([]);

  async function loadCats() {
    try {
      const res = await getCategories(shopId, { parentId, q: query });
      setCats(res);
    } catch (e) { toast({ title: "Kategori alınamadı", description: e.message, variant: "destructive" }); }
  }
  useEffect(() => { if (shopId) loadCats(); /* eslint-disable-next-line */ }, [shopId, parentId, query]);

  async function selectCat(c) {
    setSelected(c);
    try {
      const res = await getCategoryAttributes(shopId, c.id);
      setAttrs(res);
    } catch (e) { toast({ title: "Özellik alınamadı", description: e.message, variant: "destructive" }); }
  }

  return (
    <WindsterLayout title="Trendyol Kategoriler">
      <div className="grid grid-cols-12 gap-4">
        <div className="col-span-4 space-y-3">
          <div className="flex gap-2">
            <input className="border rounded px-3 py-2 w-full" placeholder="ShopId (GUID)" value={shopId} onChange={e=>setShopId(e.target.value)} />
          </div>
          <div className="flex gap-2">
            <input className="border rounded px-3 py-2 w-full" placeholder="Kategori ara..." value={query} onChange={e=>setQuery(e.target.value)} />
            {parentId!=null && <button className="border rounded px-3" onClick={()=>setParentId(null)}>Köke dön</button>}
          </div>
          <div className="border rounded-lg divide-y">
            {cats.map(c=>(
              <div key={c.id} className="p-3 flex items-center justify-between hover:bg-gray-50">
                <button className="text-left" onClick={()=>selectCat(c)}>
                  <div className="font-medium">{c.name}</div>
                  <div className="text-xs text-gray-500">ID: {c.id} {c.leaf ? "(leaf)" : ""}</div>
                </button>
                {!c.leaf && (
                  <button className="text-sm text-blue-600" onClick={()=>setParentId(c.id)}>
                    İndir (alt)
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        <div className="col-span-8">
          {selected ? (
            <div className="space-y-4">
              <div className="text-lg font-semibold">Seçili Kategori: {selected.name} (ID: {selected.id})</div>
              <div className="border rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="text-left p-3">Attribute</th>
                      <th className="text-left p-3">Flags</th>
                      <th className="text-left p-3">Values</th>
                    </tr>
                  </thead>
                  <tbody>
                    {attrs.map(a=>(
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
                          {a.values?.length
                            ? <div className="flex flex-wrap gap-2">{a.values.map(v=>(<span key={v.id} className="px-2 py-0.5 bg-gray-100 rounded">{v.name}</span>))}</div>
                            : <span className="text-gray-400">—</span>}
                        </td>
                      </tr>
                    ))}
                    {!attrs.length && (
                      <tr><td colSpan="3" className="p-4 text-gray-500">Bu kategori için özellik bulunamadı.</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="text-gray-500">Bir kategori seçiniz.</div>
          )}
        </div>
      </div>
    </WindsterLayout>
  );
}
