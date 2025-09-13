import React, { useEffect, useMemo, useState } from "react";
import WindsterLayout from "../../components/WindsterLayout";
import { api } from "../../api/client";
import { Link } from "react-router-dom";
import Modal from "../../components/ui/Modal";
import { useToast } from "../../ui-toast";

function ProductForm({ initial, onSubmit, submitting }){
  const [form, setForm] = useState({
    name: "", productCode: "", barcode: "",
    description: "", brand: "",
    basePrice: 0, taxRate: 20, isActive: true
  });

  useEffect(()=>{
    if(initial){
      setForm({
        name: initial.name ?? "",
        productCode: initial.productCode ?? "",
        barcode: initial.barcode ?? "",
        description: initial.description ?? "",
        brand: initial.brand ?? initial.brandRef?.name ?? "",
        basePrice: initial.basePrice ?? 0,
        taxRate: initial.taxRate ?? 20,
        isActive: initial.isActive ?? true,
      });
    }
  },[initial]);

  const change = (k)=> (e)=>{
    const val = e.target.type === "checkbox" ? e.target.checked : e.target.value;
    setForm(f => ({ ...f, [k]: e.target.type==="number" ? Number(val) : val }));
  };

  function submit(e){ e.preventDefault(); onSubmit(form); }

  return (
    <form onSubmit={submit} className="grid grid-cols-2 gap-3">
      <div>
        <label className="block text-sm text-gray-600 mb-1">Ad</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.name} onChange={change("name")} required />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Kod</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.productCode} onChange={change("productCode")} required />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Barkod</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.barcode} onChange={change("barcode")} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Marka</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.brand} onChange={change("brand")} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Liste Fiyatı</label>
        <input type="number" step="0.01" className="w-full rounded-lg border px-3 py-2" value={form.basePrice} onChange={change("basePrice")} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">KDV</label>
        <input type="number" className="w-full rounded-lg border px-3 py-2" value={form.taxRate} onChange={change("taxRate")} />
      </div>
      <div className="col-span-2">
        <label className="block text-sm text-gray-600 mb-1">Açıklama</label>
        <textarea className="w-full rounded-lg border px-3 py-2" rows={3} value={form.description} onChange={change("description")} />
      </div>
      <div className="col-span-2 flex items-center justify-between mt-1">
        <label className="inline-flex items-center gap-2 text-sm">
          <input type="checkbox" checked={form.isActive} onChange={change("isActive")} /> Aktif
        </label>
        <button disabled={submitting} className="rounded-lg bg-blue-600 text-white px-4 py-2 hover:bg-blue-700 disabled:opacity-50">
          {submitting ? "Kaydediliyor…" : "Kaydet"}
        </button>
      </div>
    </form>
  );
}

export default function ProductsList(){
  const toast = useToast();
  const [rows, setRows] = useState([]);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [sort, setSort] = useState({ by:"name", dir:"asc" });
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const [modalOpen, setModalOpen] = useState(false);
  const [editRow, setEditRow] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [confirm, setConfirm] = useState(null); // { id, name }

  async function load(){
    setLoading(true); setErr("");
    try { setRows(await api("/admin/products")); }
    catch(e){ setErr(e.message); }
    finally{ setLoading(false); }
  }
  useEffect(()=>{ load(); },[]);

  const filtered = useMemo(()=>{
    const t = q.toLowerCase().trim();
    let data = !t ? rows : rows.filter(r =>
      (r.name||"").toLowerCase().includes(t) ||
      (r.productCode||"").toLowerCase().includes(t) ||
      (r.barcode||"").toLowerCase().includes(t)
    );
    const { by, dir } = sort;
    data = [...data].sort((a,b)=>{
      const va = (a[by] ?? "").toString().toLowerCase();
      const vb = (b[by] ?? "").toString().toLowerCase();
      if (va < vb) return dir==="asc"?-1:1;
      if (va > vb) return dir==="asc"?1:-1;
      return 0;
    });
    return data;
  }, [rows, q, sort]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paged = filtered.slice((page-1)*pageSize, page*pageSize);
  const toggleSort = (by)=> setSort(s => s.by===by ? { by, dir:s.dir==="asc"?"desc":"asc"} : { by, dir:"asc" });

  const openCreate = ()=> { setEditRow(null); setModalOpen(true); };
  const openEdit = (row)=> { setEditRow(row); setModalOpen(true); };

  async function save(form){
    setSubmitting(true);
    try{
      if(editRow) { await api(`/admin/products/${editRow.id}`, { method:"PUT", body: JSON.stringify(form) }); toast.push("Ürün güncellendi"); }
      else { await api(`/admin/products`, { method:"POST", body: JSON.stringify(form) }); toast.push("Ürün eklendi"); }
      setModalOpen(false);
      await load();
    }catch(e){ toast.push(e.message,"error"); }
    finally{ setSubmitting(false); }
  }

  async function doDelete(){
    if (!confirm) return;
    try{ await api(`/admin/products/${confirm.id}`, { method:"DELETE" }); setConfirm(null); toast.push("Ürün silindi"); await load(); }
    catch(e){ toast.push(e.message,"error"); }
  }

  return (
    <WindsterLayout>
      {/* Toolbar */}
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Products</h2>
        <div className="flex items-center gap-2">
          <input
            placeholder="Ara (ad/kod/barkod)"
            value={q} onChange={e=>{ setQ(e.target.value); setPage(1); }}
            className="w-72 rounded-lg border px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button onClick={openCreate} className="rounded-lg bg-blue-600 text-white px-3 py-2 hover:bg-blue-700">+ Yeni</button>
        </div>
      </div>

      {/* Grid */}
      <div className="rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-full text-sm">
            <thead className="sticky top-0 bg-gray-50 text-left">
              <tr>
                {[
                  ["name","Name","w-64"],
                  ["productCode","Code","w-40"],
                  ["barcode","Barcode","w-40"],
                  ["brand","Brand","w-40"],
                  ["basePrice","BasePrice","w-24"],
                  ["taxRate","Tax","w-16"],
                  ["isActive","Active","w-20"],
                ].map(([key,label,width])=>(
                  <th key={key} className={"px-4 py-3 cursor-pointer select-none " + width} onClick={()=>toggleSort(key)}>
                    <div className="inline-flex items-center gap-1">
                      {label}{sort.by===key && <span className="text-gray-400">{sort.dir==="asc"?"▲":"▼"}</span>}
                    </div>
                  </th>
                ))}
                <th className="px-4 py-3 w-40">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {loading && <tr><td className="px-4 py-6" colSpan={8}>Yükleniyor…</td></tr>}
              {err && !loading && <tr><td className="px-4 py-6 text-red-600" colSpan={8}>{err}</td></tr>}

              {!loading && !err && paged.map(r=>(
                <tr key={r.id} className="hover:bg-gray-50 group">
                  <td className="px-4 py-3">{r.name}</td>
                  <td className="px-4 py-3">{r.productCode}</td>
                  <td className="px-4 py-3">{r.barcode}</td>
                  <td className="px-4 py-3">{r.brand ?? r.brandRef?.name}</td>
                  <td className="px-4 py-3">{r.basePrice}</td>
                  <td className="px-4 py-3">{r.taxRate}</td>
                  <td className="px-4 py-3">{r.isActive ? "✔️" : "—"}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
                      <Link className="text-blue-600 hover:underline" to={`/products/${r.id}`}>Detay</Link>
                      <button className="text-emerald-700 hover:underline" onClick={()=>openEdit(r)}>Düzenle</button>
                      <button className="text-red-600 hover:underline" onClick={()=>setConfirm({ id:r.id, name:r.name })}>Sil</button>
                    </div>
                  </td>
                </tr>
              ))}

              {!loading && !err && !paged.length && (
                <tr><td className="px-4 py-6 text-center text-gray-500" colSpan={8}>Kayıt yok</td></tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Footer / Pagination */}
        <div className="flex items-center justify-between px-4 py-3 border-t text-sm">
          <div>
            Toplam <b>{filtered.length}</b> kayıt — Sayfa <b>{page}</b>/<b>{totalPages}</b>
          </div>
          <div className="flex items-center gap-2">
            <button
              className="px-3 py-1 rounded-lg border hover:bg-gray-100 disabled:opacity-50"
              onClick={()=>setPage(p=>Math.max(1, p-1))}
              disabled={page<=1}
            >
              ‹ Önceki
            </button>
            <button
              className="px-3 py-1 rounded-lg border hover:bg-gray-100 disabled:opacity-50"
              onClick={()=>setPage(p=>Math.min(totalPages, p+1))}
              disabled={page>=totalPages}
            >
              Sonraki ›
            </button>
          </div>
        </div>
      </div>

      {/* Create/Edit Modal */}
      <Modal open={modalOpen} title={editRow ? "Ürün Düzenle" : "Yeni Ürün"} onClose={()=>setModalOpen(false)}>
        <ProductForm initial={editRow} onSubmit={save} submitting={submitting} />
      </Modal>

      {/* Delete Confirm */}
      <Modal open={!!confirm} title="Silme Onayı" onClose={()=>setConfirm(null)}>
        <div className="space-y-4">
          <p><b>{confirm?.name}</b> adlı ürünü silmek istiyor musunuz?</p>
          <div className="flex items-center justify-end gap-2">
            <button className="px-3 py-2 rounded-lg border" onClick={()=>setConfirm(null)}>Vazgeç</button>
            <button className="px-3 py-2 rounded-lg bg-red-600 text-white hover:bg-red-700" onClick={doDelete}>Sil</button>
          </div>
        </div>
      </Modal>
    </WindsterLayout>
  );
}
