import React, { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { setToken } from "../api/client";

export default function WindsterLayout({ children, title="Ecom Admin" }){
  const [open, setOpen] = useState(false);
  const { pathname } = useLocation();
  const nav = useNavigate();
  const isActive = (to) => pathname.startsWith(to);
  const logout = () => { setToken(""); nav("/login"); };

  return (
    <div className="bg-gray-50 text-gray-900 min-h-screen">
      {/* Topbar */}
      <nav className="bg-white border-b border-gray-200 fixed z-30 w-full">
        <div className="px-3 py-3 lg:px-5 lg:pl-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <button onClick={()=>setOpen(true)} className="p-2 text-gray-600 rounded-lg hover:bg-gray-100 lg:hidden" aria-label="Open sidebar">☰</button>
              <Link to="/products" className="ml-2 md:mr-24 text-xl font-semibold">{title}</Link>
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden md:block">
                <input className="bg-gray-50 border border-gray-300 text-sm rounded-lg w-72 p-2" placeholder="Search…" />
              </div>
              <button onClick={logout} className="bg-gray-900 text-white text-sm rounded-lg px-3 py-2 hover:bg-gray-800">Logout</button>
            </div>
          </div>
        </div>
      </nav>

      {/* Sidebar */}
      <div className={`fixed inset-0 z-20 bg-black/40 lg:hidden ${open? "": "hidden"}`} onClick={()=>setOpen(false)} />
      <aside className={`fixed z-30 top-0 left-0 h-full w-64 bg-white border-r border-gray-200 pt-16 transition-transform lg:translate-x-0 ${open? "translate-x-0":"-translate-x-full"}`}>
        <div className="h-full overflow-y-auto px-3 py-4">
          <ul className="space-y-1">
            <li>
              <Link to="/products" onClick={()=>setOpen(false)}
                className={`block rounded-lg px-3 py-2 text-sm ${isActive("/products")? "bg-blue-100 text-blue-700":"text-gray-700 hover:bg-gray-100"}`}>
                📦 Products
              </Link>
            </li>
            <li>
              <Link to="/shops" onClick={()=>setOpen(false)}
                className={`block rounded-lg px-3 py-2 text-sm ${isActive("/shops")? "bg-blue-100 text-blue-700":"text-gray-700 hover:bg-gray-100"}`}>
                🏬 Shops
              </Link>
            </li>
          </ul>
        </div>
        <button onClick={()=>setOpen(false)} className="absolute top-3 right-3 p-2 rounded-lg text-gray-600 hover:bg-gray-100 lg:hidden">✕</button>
      </aside>

      {/* Content */}
      <div className="pt-16 lg:ml-64">
        <div className="mx-auto max-w-7xl px-4 py-6">{children}</div>
      </div>
    </div>
  );
}
