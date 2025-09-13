import React from "react";

export default function Modal({ open, title, children, onClose }){
  if(!open) return null;
  return (
    <div className="fixed inset-0 z-50">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className="w-full max-w-lg rounded-xl bg-white shadow-lg">
          <div className="flex items-center justify-between border-b px-4 py-3">
            <h3 className="font-semibold">{title}</h3>
            <button onClick={onClose} className="text-gray-500 hover:text-gray-700">✕</button>
          </div>
          <div className="p-4">{children}</div>
        </div>
      </div>
    </div>
  );
}
