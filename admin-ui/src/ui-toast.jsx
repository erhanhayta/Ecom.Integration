import React, { createContext, useCallback, useContext, useState } from "react";

const ToastCtx = createContext(null);
export function useToast(){ return useContext(ToastCtx); }

export function ToastProvider({ children }){
  const [items, setItems] = useState([]); // {id,type,msg}
  const push = useCallback((msg, type="success")=>{
    const id = Math.random().toString(36).slice(2);
    setItems(list => [...list, { id, type, msg }]);
    setTimeout(() => setItems(list => list.filter(x=>x.id!==id)), 2500);
  },[]);

  return (
    <ToastCtx.Provider value={{ push }}>
      {children}
      <div className="fixed bottom-4 right-4 z-50 space-y-2">
        {items.map(t => (
          <div key={t.id}
            className={"rounded-lg px-3 py-2 text-sm shadow text-white " + (t.type==="error"?"bg-red-600":"bg-emerald-600")}>
            {t.msg}
          </div>
        ))}
      </div>
    </ToastCtx.Provider>
  );
}
