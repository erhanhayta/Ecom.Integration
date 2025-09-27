import { api } from "./client";

// 1) Tek dosya upload → { imageId, url }
export async function uploadProductImageFile(productId, file, { sortOrder=0, isMain=false, linkToProduct=false } = {}) {
  const fd = new FormData();
  fd.append("file", file);
  const qs = new URLSearchParams({ sortOrder, isMain, linkToProduct });
  return api(`/admin/products/${productId}/images?${qs.toString()}`, {
    method: "POST",
    body: fd, // Content-Type’ı client.js ayarlamayacak; browser boundary’yi koyacak
  });
}


// 2) Birden çok dosya yükle (tek tek çağırıp imageId listesi döner)
export async function uploadProductImageFilesBulk(productId, files, opts) {
  const out = [];
  for (const f of files) {
    const r = await uploadProductImageFile(productId, f, opts);
    out.push(r.imageId);
  }
  return out; // imageId listesi
}

// 3) Link oluştur (çoğaltma)
export async function linkImages(productId, { imageIds, linkToProduct=false, variantIds=[], sortOrder=0, isMain=false }) {
  return api(`/admin/products/${productId}/images/links`, {
    method: "POST",
    body: JSON.stringify({ imageIds, linkToProduct, variantIds, sortOrder, isMain })
  });
}

// 4) Liste (ürün/variant)
export async function listProductImages(productId, { variantId } = {}) {
  const qs = new URLSearchParams();
  if (variantId) qs.set("variantId", variantId);
  return api(`/admin/products/${productId}/images${qs.toString() ? `?${qs}` : ""}`);
}

// 5) Link sil
export async function deleteProductImageLink(productId, linkId) {
  return api(`/admin/products/${productId}/images/links/${linkId}`, { method: "DELETE" });
}
