import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import WindsterLayout from "../../components/WindsterLayout";
import { api } from "../../api/client";

export default function ProductDetail(){
  const { id } = useParams();
  const nav = useNavigate();
  const [data,setData] = useState(null);
  const [msg,setMsg] = useState("");

  useEffect(()=>{
    (async()=>{
      try{ setData(await api(`/admin/products/${id}`)); }
      catch(e){ setMsg(e.message); }
    })();
  },[id]);

  return (
    <WindsterLayout>
      <button onClick={()=>nav(-1)} className="text-sm text-gray-600 hover:text-gray-900 mb-4">&larr; Geri</button>
      {!data ? (<div>Yükleniyor… {msg && <span className="text-red-600">({msg})</span>}</div>) : (
        <div className="grid md:grid-cols-2 gap-4">
          <div className="rounded-xl border bg-white p-4">
            <h3 className="font-semibold mb-3">Özet</h3>
            <div className="grid grid-cols-2 gap-y-2 text-sm">
              <div className="text-gray-500">Ad</div><div>{data.name}</div>
              <div className="text-gray-500">Kod</div><div>{data.productCode}</div>
              <div className="text-gray-500">Barkod</div><div>{data.barcode}</div>
              <div className="text-gray-500">Marka</div><div>{data.brand ?? data.brandRef?.name ?? "-"}</div>
              <div className="text-gray-500">BasePrice</div><div>{data.basePrice}</div>
              <div className="text-gray-500">KDV</div><div>{data.taxRate}</div>
              <div className="text-gray-500">Aktif</div><div>{data.isActive ? "Evet":"Hayır"}</div>
            </div>
          </div>

          <div className="rounded-xl border bg-white p-4">
            <h3 className="font-semibold mb-3">Varyantlar</h3>
            {!data.variants?.length ? <div className="text-sm text-gray-500">Varyant yok.</div> :
              <div className="overflow-x-auto rounded-lg border">
                <table className="min-w-full text-sm">
                  <thead className="bg-gray-50 text-left">
                    <tr>
                      <th className="px-3 py-2">Renk</th>
                      <th className="px-3 py-2">Beden</th>
                      <th className="px-3 py-2">SKU</th>
                      <th className="px-3 py-2">Barkod</th>
                      <th className="px-3 py-2">Stok</th>
                      <th className="px-3 py-2">Satış</th>
                      <th className="px-3 py-2">Liste</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {data.variants.map(v=>(
                      <tr key={v.id} className="hover:bg-gray-50">
                        <td className="px-3 py-2">{v.color}</td>
                        <td className="px-3 py-2">{v.size}</td>
                        <td className="px-3 py-2">{v.sku}</td>
                        <td className="px-3 py-2">{v.barcode}</td>
                        <td className="px-3 py-2">{v.stock}</td>
                        <td className="px-3 py-2">{v.salePrice ?? v.price ?? "-"}</td>
                        <td className="px-3 py-2">{v.listPrice ?? "-"}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      )}
    </WindsterLayout>
  );
}
