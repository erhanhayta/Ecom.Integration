import { api } from "./client";

/**
 * Tek bir varyant ekler.
 * POST /admin/products/{productId}/variants
 * body: { color, size, sku, barcode, price, stock, isActive }
 */
export async function addVariant(productId, variant) {
  return api(`/admin/products/${productId}/variants`, {
    method: "POST",
    body: JSON.stringify(variant),
  });
}
