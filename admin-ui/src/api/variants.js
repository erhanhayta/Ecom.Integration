// src/api/variants.js
import { api } from "./client";

/** Tek varyant ekle */
export function addVariant(productId, variant) {
  return api(`/admin/products/${productId}/variants`, {
    method: "POST",
    body: JSON.stringify(variant),
  });
}

/** Tek varyant kısmi güncelle (PATCH) */
export function patchVariant(productId, variantId, payload) {
  return api(`/admin/products/${productId}/variants/${variantId}`, {
    method: "PATCH",
    body: JSON.stringify(payload),
  });
}

/** Toplu fiyat güncelle (op: set|inc|dec|incp|decp) */
export function bulkPrice(productId, { color, size, op, value }) {
  return api(`/admin/products/${productId}/variants/bulk-price`, {
    method: "POST",
    body: JSON.stringify({ color, size, op, value }),
  });
}
