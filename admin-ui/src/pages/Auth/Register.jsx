import React, { useState } from "react";
import { api } from "../../api/client";
import WindsterLayout from "../../components/WindsterLayout";

export default function Register(){
  const [f,setF] = useState({ username:"", password:"", role:"admin" });
  const [msg,setMsg] = useState("");

  async function submit(e){
    e.preventDefault();
    setMsg("");
    try{
      await api("/admin/users", { method:"POST", body:f });
      setMsg("Kullanıcı oluşturuldu.");
      setF({ username:"", password:"", role:"admin" });
    }catch(err){ setMsg(err.message); }
  }

  return (
    <WindsterLayout title="Kullanıcı Oluştur">
      <form onSubmit={submit} className="max-w-md space-y-3 bg-white p-4 rounded-xl border">
        <div>
          <label className="text-sm text-gray-600">Kullanıcı Adı</label>
          <input className="w-full border rounded px-3 py-2"
                 value={f.username} onChange={e=>setF({...f, username:e.target.value})}/>
        </div>
        <div>
          <label className="text-sm text-gray-600">Şifre</label>
          <input type="password" className="w-full border rounded px-3 py-2"
                 value={f.password} onChange={e=>setF({...f, password:e.target.value})}/>
        </div>
        <div>
          <label className="text-sm text-gray-600">Rol</label>
          <select className="w-full border rounded px-3 py-2"
                  value={f.role} onChange={e=>setF({...f, role:e.target.value})}>
            <option value="Admin">admin</option>
            <option value="Editor">editor</option>
          </select>
        </div>
        <button className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700">Kaydet</button>
        {msg && <div className="text-sm mt-2">{msg}</div>}
      </form>
    </WindsterLayout>
  );
}
