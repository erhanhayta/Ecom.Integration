import React, { useEffect, useState } from "react";

/**
 * Basit varyant seçimi (Renk & Beden) ve fotoğraf eşleme UI'ı.
 * Props:
 *  - productImages: [{ id, url, fileName }]
 *  - onChange?: (state) => void
 */
export default function VariantPanel({ productImages = [], onChange }) {
  const [color, setColor] = useState("");
  const [size, setSize] = useState("");
  const [imageMap, setImageMap] = useState({}); // key: `${color}|${size}` -> imageId[]

  function key(c, s){ return `${c || ""}|${s || ""}`; }

  function toggleImageForSelection(imgId){
    const k = key(color, size);
    setImageMap(prev => {
      const arr = new Set(prev[k] || []);
      if (arr.has(imgId)) arr.delete(imgId); else arr.add(imgId);
      const next = { ...prev, [k]: Array.from(arr) };
      onChange?.({ color, size, imageMap: next });
      return next;
    });
  }

  useEffect(()=> {
    onChange?.({ color, size, imageMap });
  }, [color, size]); // eslint-disable-line

  const selectedKey = key(color, size);
  const selectedImages = new Set(imageMap[selectedKey] || []);

  return (
    <div className="space-y-3">
      <div className="text-lg font-semibold">Varyantlar</div>

      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-sm text-gray-600 mb-1">Renk</label>
          <input className="w-full border rounded px-3 py-2" value={color} onChange={e=>setColor(e.target.value)} placeholder="örn. Siyah" />
        </div>
        <div>
          <label className="block text-sm text-gray-600 mb-1">Beden</label>
          <input className="w-full border rounded px-3 py-2" value={size} onChange={e=>setSize(e.target.value)} placeholder="örn. 42" />
        </div>
      </div>

      <div className="mt-2">
        <div className="text-sm text-gray-600 mb-2">Fotoğraflar (seçili varyanta bağla)</div>
        <div className="grid grid-cols-6 gap-2">
          {productImages.map(img => (
            <button
              key={img.id}
              type="button"
              onClick={()=>toggleImageForSelection(img.id)}
              className={`border rounded p-1 ${selectedImages.has(img.id) ? "ring-2 ring-blue-500" : ""}`}
              title={img.fileName || ""}
            >
              <img src={img.url} alt={img.fileName || "image"} className="w-full h-16 object-cover rounded" />
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
