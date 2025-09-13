import React, { useEffect, useMemo, useState } from "react";
import WindsterLayout from "../../components/WindsterLayout";
import Modal from "../../components/ui/Modal";
import { api } from "../../api/client";
import { useToast } from "../../ui-toast";

/**
 * Beklenen API yolları:
 * GET    /admin/MarketplaceShops
 * POST   /admin/MarketplaceShops
 * PUT    /admin/MarketplaceShops/{id}
 * DELETE /admin/MarketplaceShops/{id}
 *
 * Model:
 * {
 *   id: "guid",
 *   firm: number,
 *   shopName: string,
 *   baseUrl: string,
 *   apiKey: string,
 *   apiSecret: string,
 *   token: string,
 *   accountId: string,
 *   integrationRefCode: string,
 *   isActive: boolean,
 *   createdAtUtc?: string,
 *   updatedAtUtc?: string
 * }
 */

function ShopForm({ initial, onSubmit, submitting }) {
  const [form, setForm] = useState({
    firm: 1,
    shopName: "",
    baseUrl: "",
    apiKey: "",
    apiSecret: "",
    token: "",
    accountId: "",
    integrationRefCode: "",
    isActive: true,
  });

  useEffect(() => {
    if (initial) {
      setForm({
        firm: Number(initial.firm ?? 1),
        shopName: initial.shopName ?? "",
        baseUrl: initial.baseUrl ?? "",
        apiKey: initial.apiKey ?? "",
        apiSecret: initial.apiSecret ?? "",
        token: initial.token ?? "",
        accountId: initial.accountId ?? "",
        integrationRefCode: initial.integrationRefCode ?? "",
        isActive: initial.isActive ?? true,
      });
    }
  }, [initial]);

  const change = (k) => (e) => {
    const isCheckbox = e.target.type === "checkbox";
    const isNumber = e.target.type === "number";
    const val = isCheckbox ? e.target.checked : e.target.value;
    setForm((f) => ({ ...f, [k]: isNumber ? Number(val) : val }));
  };

  function submit(e) {
    e.preventDefault();
    onSubmit(form);
  }

  return (
    <form onSubmit={submit} className="grid grid-cols-2 gap-3">
      <div>
        <label className="block text-sm text-gray-600 mb-1">Firma</label>
        <input type="number" className="w-full rounded-lg border px-3 py-2" value={form.firm} onChange={change("firm")} min={1} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Mağaza Adı</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.shopName} onChange={change("shopName")} required />
      </div>

      <div className="col-span-2">
        <label className="block text-sm text-gray-600 mb-1">URL</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.baseUrl} onChange={change("baseUrl")} placeholder="https://apigw.trendyol.com / https://..." />
      </div>

      <div>
        <label className="block text-sm text-gray-600 mb-1">API Key</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.apiKey} onChange={change("apiKey")} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">API Secret</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.apiSecret} onChange={change("apiSecret")} />
      </div>

      <div className="col-span-2">
        <label className="block text-sm text-gray-600 mb-1">Token</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.token} onChange={change("token")} />
      </div>

      <div>
        <label className="block text-sm text-gray-600 mb-1">Satıcı Id</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.accountId} onChange={change("accountId")} />
      </div>
      <div>
        <label className="block text-sm text-gray-600 mb-1">Entegrasyon Referans Kodu</label>
        <input className="w-full rounded-lg border px-3 py-2" value={form.integrationRefCode} onChange={change("integrationRefCode")} />
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

export default function Shops() {
  const toast = useToast();
  const [rows, setRows] = useState([]);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [modalOpen, setModalOpen] = useState(false);
  const [editRow, setEditRow] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [confirm, setConfirm] = useState(null);

  async function load() {
    setLoading(true);
    setErr("");
    try {
      const data = await api("/admin/MarketplaceShops");
      setRows(Array.isArray(data) ? data : (data.items ?? []));
    } catch (e) {
      setErr(e.message);
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => { load(); }, []);

  const filtered = useMemo(() => {
    const t = q.toLowerCase().trim();
    if (!t) return rows;
    return rows.filter(r =>
      (r.shopName || "").toLowerCase().includes(t) ||
      (r.accountId || "").toLowerCase().includes(t) ||
      (r.integrationRefCode || "").toLowerCase().includes(t) ||
      (String(r.firm || "")).toLowerCase().includes(t)
    );
  }, [rows, q]);

  const openCreate = () => { setEditRow(null); setModalOpen(true); };
  const openEdit = (row) => { setEditRow(row); setModalOpen(true); };

  async function save(form) {
    setSubmitting(true);
    try {
      if (editRow) {
        await api(`/admin/MarketplaceShops/${editRow.id}`, { method: "PUT", body: JSON.stringify(form) });
        toast.push("Mağaza güncellendi");
      } else {
        await api(`/admin/MarketplaceShops`, { method: "POST", body: JSON.stringify(form) });
        toast.push("Mağaza eklendi");
      }
      setModalOpen(false);
      await load();
    } catch (e) {
      toast.push(e.message, "error");
    } finally {
      setSubmitting(false);
    }
  }

  async function doDelete() {
    if (!confirm) return;
    try {
      await api(`/admin/MarketplaceShops/${confirm.id}`, { method: "DELETE" });
      setConfirm(null);
      toast.push("Mağaza silindi");
      await load();
    } catch (e) {
      toast.push(e.message, "error");
    }
  }

  return (
    <WindsterLayout>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Marketplace Shops</h2>
        <div className="flex items-center gap-2">
          <input
            placeholder="Ara (shopName / accountId / refCode / firm)"
            value={q} onChange={e=>{ setQ(e.target.value); }}
            className="w-80 rounded-lg border px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button onClick={openCreate} className="rounded-lg bg-blue-600 text-white px-3 py-2 hover:bg-blue-700">+ Yeni</button>
        </div>
      </div>

      <div className="rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 w-56">Mağaza Adı</th>
                <th className="px-4 py-3 w-28">Firma</th>
                <th className="px-4 py-3 w-64">Satıcı Id</th>
                <th className="px-4 py-3 w-64">URL</th>
                <th className="px-4 py-3 w-28">Active</th>
                <th className="px-4 py-3 w-44">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {loading && <tr><td className="px-4 py-6" colSpan={6}>Yükleniyor…</td></tr>}
              {err && !loading && <tr><td className="px-4 py-6 text-red-600" colSpan={6}>{err}</td></tr>}

              {!loading && !err && filtered.map(r => (
                <tr key={r.id} className="hover:bg-gray-50 group">
                  <td className="px-4 py-3">{r.shopName}</td>
                  <td className="px-4 py-3">{r.firm}</td>
                  <td className="px-4 py-3">{r.accountId}</td>
                  <td className="px-4 py-3">{r.baseUrl}</td>
                  <td className="px-4 py-3">{r.isActive ? "✔️" : "—"}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button className="text-emerald-700 hover:underline" onClick={() => openEdit(r)}>Düzenle</button>
                      <button className="text-red-600 hover:underline" onClick={() => setConfirm({ id: r.id, name: r.shopName })}>Sil</button>
                    </div>
                  </td>
                </tr>
              ))}

              {!loading && !err && !filtered.length && (
                <tr><td className="px-4 py-6 text-center text-gray-500" colSpan={6}>Kayıt yok</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create/Edit */}
      <Modal open={modalOpen} title={editRow ? "Mağaza Düzenle" : "Yeni Mağaza"} onClose={() => setModalOpen(false)}>
        <ShopForm initial={editRow} onSubmit={save} submitting={submitting} />
      </Modal>

      {/* Delete Confirm */}
      <Modal open={!!confirm} title="Silme Onayı" onClose={() => setConfirm(null)}>
        <div className="space-y-4">
          <p><b>{confirm?.name ?? confirm?.shopName}</b> mağazasını silmek istiyor musunuz?</p>
          <div className="flex items-center justify-end gap-2">
            <button className="px-3 py-2 rounded-lg border" onClick={() => setConfirm(null)}>Vazgeç</button>
            <button className="px-3 py-2 rounded-lg bg-red-600 text-white hover:bg-red-700" onClick={doDelete}>Sil</button>
          </div>
        </div>
      </Modal>
    </WindsterLayout>
  );
}
