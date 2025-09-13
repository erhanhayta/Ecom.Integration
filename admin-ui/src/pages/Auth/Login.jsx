import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../../api/auth";
import { useToast } from "../../ui-toast";

export default function Login(){
  const nav = useNavigate();
  const toast = useToast();
  const [u,setU]=useState(""); const [p,setP]=useState("");
  const [err,setErr]=useState(""); const [loading,setL]=useState(false);

  async function submit(e){
    e.preventDefault(); setErr(""); setL(true);
    try { await login(u,p); toast.push("Hoş geldin!"); nav("/products"); }
    catch(e){ setErr("Giriş başarısız. Bilgileri kontrol edin."); toast.push("Giriş başarısız","error"); }
    finally{ setL(false); }
  }

  return (
    <div className="min-h-screen grid place-items-center bg-gray-50 px-4">
      <div className="w-full max-w-md rounded-xl border bg-white p-6 shadow">
        <h2 className="text-xl font-semibold text-center">Yönetim Paneli</h2>
        <p className="text-gray-500 text-center mb-6">Giriş yap</p>
        <form onSubmit={submit} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-600 mb-1">Kullanıcı Adı</label>
            <input className="w-full rounded-lg border px-3 py-2" value={u} onChange={e=>setU(e.target.value)} />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">Şifre</label>
            <input type="password" className="w-full rounded-lg border px-3 py-2" value={p} onChange={e=>setP(e.target.value)} />
          </div>
          {err && <div className="text-sm text-red-600 bg-red-100 p-2 rounded">{err}</div>}
          <button disabled={loading} className="w-full rounded-lg bg-blue-600 text-white py-2 hover:bg-blue-700 disabled:opacity-50">
            {loading ? "Giriş yapılıyor…" : "Giriş Yap"}
          </button>
        </form>
      </div>
    </div>
  );
}
