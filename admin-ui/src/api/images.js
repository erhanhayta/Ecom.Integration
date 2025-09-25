// src/api/images.js
import { api } from "./client";

/** Tek dosya upload (form-data) */
export async function uploadProductImageFile(productId, file, { variantId, sortOrder, isMain } = {}) {
  const fd = new FormData();
  fd.append("file", file);
  if (sortOrder !== undefined) fd.append("sortOrder", String(sortOrder));
  if (isMain !== undefined) fd.append("isMain", String(isMain));
  const q = variantId ? `?variantId=${encodeURIComponent(variantId)}` : "";
  return api(`/admin/products/${productId}/images${q}`, { method: "POST", body: fd });
}

export async function uploadProductImageFilesBulk(productId, files, { variantId, sortOrder, isMain } = {}) {
  const fd = new FormData();
  for (const f of files) fd.append("files", f);
  if (sortOrder !== undefined) fd.append("sortOrder", String(sortOrder));
  if (isMain !== undefined) fd.append("isMain", String(isMain));
  const q = variantId ? `?variantId=${encodeURIComponent(variantId)}` : "";
  return api(`/admin/products/${productId}/images/bulk${q}`, { method: "POST", body: fd });
}

export async function listProductImages(productId, { variantId } = {}) {
  const q = variantId ? `?variantId=${encodeURIComponent(variantId)}` : "";
  return api(`/admin/products/${productId}/images${q}`);
}

export async function deleteProductImage(productId, imageId) {
  return api(`/admin/products/${productId}/images/${imageId}`, { method: "DELETE" });
}
